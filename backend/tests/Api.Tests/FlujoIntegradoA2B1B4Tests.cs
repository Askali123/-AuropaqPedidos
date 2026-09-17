using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Application.Consolidaciones;
using AuropaqPedidos.Application.Consolidaciones.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Prueba integrada de extremo a extremo (cierre de negocio 2026-09-11, sección 7 de la tarea):
//
// Requisición -> Aprobación -> Consolidación #1 -> nueva requisición aprobada ->
// Consolidación #2 -> PedidoProveedor -> Entrega parcial -> Anulación de entrega ->
// Recalcular estado -> Factura -> Anulación de factura -> nueva factura con cantidad liberada.
//
// Requisición y PedidoProveedor/Entrega/Factura se ejercitan por HTTP real (tienen Controller);
// Consolidación no tiene Controller (decisión ya documentada, 05-api.md §31) — se invoca
// directamente el UseCase de Application, resuelto desde el mismo contenedor de DI que usa la
// Api, contra la misma base de datos real.
public sealed class FlujoIntegradoA2B1B4Tests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public FlujoIntegradoA2B1B4Tests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task Flujo_completo_A2_B1_B4()
    {
        int empresa1Id, empresa2Id, periodoId, productoId, sede1Id, sede2Id, proveedorId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            var empresa1 = new Empresa(9001, "Empresa Integrada 1");
            var empresa2 = new Empresa(9002, "Empresa Integrada 2");
            var periodo = new Periodo(
                9000, 2026, 9,
                new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
                new DateTime(2026, 9, 1), new DateTime(2100, 1, 1),
                "ABIERTO");
            var categoria = new Categoria(9000, "Aseo");
            var unidadMedida = new UnidadMedida(9000, "UNIDAD", "Unidad");
            var producto = new Producto(9000, "Papel higiénico", categoria, unidadMedida);
            var sede1 = new Sede(9000, empresa1, "Sede Empresa 1");
            var sede2 = new Sede(9001, empresa2, "Sede Empresa 2");
            var proveedor = new Proveedor(9000, "Proveedor Integrado");

            db.Empresas.AddRange(empresa1, empresa2);
            db.Periodos.Add(periodo);
            db.Categorias.Add(categoria);
            db.UnidadesMedida.Add(unidadMedida);
            db.Productos.Add(producto);
            db.Sedes.AddRange(sede1, sede2);
            db.Proveedores.Add(proveedor);
            await db.SaveChangesAsync();

            empresa1Id = empresa1.Id;
            empresa2Id = empresa2.Id;
            periodoId = periodo.Id;
            productoId = producto.Id;
            sede1Id = sede1.Id;
            sede2Id = sede2.Id;
            proveedorId = proveedor.Id;
        }

        // Autorización real agregada 2026-09-17 (RN-063/ADR-066): PedidoProveedor/Entrega/Factura
        // ya exigen JWT (sin alcance por empresa, CLAUDE.md §27) — un único token con todos los
        // permisos necesarios, puesto como default en `_cliente`, alcanza para todo el tramo
        // Pedido→Entrega→Factura de este flujo. Los llamados de Requisición siguen usando su
        // propio token explícito por mensaje (CrearYAprobarRequisicionAsync) — un
        // HttpRequestMessage con su propio Authorization no se ve afectado por este default.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            var tokenPedidos = await AutorizacionHelper.CrearTokenConPermisosAsync(
                _factory, db, 9000, empresa1Id,
                "PEDIDO_CREAR", "PEDIDO_ENVIAR", "ENTREGA_REGISTRAR", "ENTREGA_ANULAR",
                "FACTURA_REGISTRAR", "FACTURA_ANULAR");
            _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenPedidos);
        }

        // ---- Requisición #1 (Empresa 1), aprobada, 30 unidades ----
        var requisicion1Id = await CrearYAprobarRequisicionAsync(empresa1Id, periodoId, productoId, sede1Id, cantidad: 30);

        // ---- Consolidación #1: toma la requisición #1 ----
        ConsolidacionResponse consolidacion1;
        using (var scope = _factory.Services.CreateScope())
        {
            var crearConsolidacion = scope.ServiceProvider.GetRequiredService<CrearConsolidacionUseCase>();
            consolidacion1 = crearConsolidacion.Ejecutar(usuarioId: 5, DateTime.UtcNow, new CrearConsolidacionRequest(periodoId, "GENERADA"));
        }
        var detalleConsolidado1 = Assert.Single(consolidacion1.Detalles);
        Assert.Equal(30, detalleConsolidado1.CantidadNecesaria);

        // ---- Requisición #2 (Empresa 2, mismo periodo), aprobada DESPUÉS de la primera
        // consolidación, 20 unidades ----
        var requisicion2Id = await CrearYAprobarRequisicionAsync(empresa2Id, periodoId, productoId, sede2Id, cantidad: 20);

        // ---- Consolidación #2 (A2): NO repite la requisición #1, solo toma la nueva ----
        ConsolidacionResponse consolidacion2;
        using (var scope = _factory.Services.CreateScope())
        {
            var crearConsolidacion = scope.ServiceProvider.GetRequiredService<CrearConsolidacionUseCase>();
            consolidacion2 = crearConsolidacion.Ejecutar(usuarioId: 5, DateTime.UtcNow, new CrearConsolidacionRequest(periodoId, "GENERADA"));
        }
        var detalleConsolidado2 = Assert.Single(consolidacion2.Detalles);
        Assert.Equal(20, detalleConsolidado2.CantidadNecesaria); // no 50: no se duplicó la necesidad ya consolidada.

        // ---- PedidoProveedor a partir de la Consolidación #2 (20 unidades) ----
        var respuestaCrearPedido = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = consolidacion2.Id,
            proveedorId,
            numeroPedido = "PO-INTEGRADO-001"
        });
        var pedido = await respuestaCrearPedido.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();

        var respuestaDetallePedido = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido!.Data.Id}/detalles", new
        {
            detalleConsolidacionId = detalleConsolidado2.Id,
            cantidadPedida = 20
        });
        var pedidoConDetalle = await respuestaDetallePedido.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        var detallePedidoId = pedidoConDetalle!.Data.Detalles[0].Id;

        // RN-065/D-18 (2026-09-17): distribución completa exigida antes de enviar.
        await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/detalles/{detallePedidoId}/distribuciones", new
        {
            sedeId = sede2Id,
            cantidad = 20
        });

        await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/enviar", content: null);

        // ---- Entrega parcial (12 de 20) ----
        var respuestaEntrega = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/entregas", new
        {
            numeroRemision = "REM-INTEGRADO-001"
        });
        var entrega = await respuestaEntrega.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();

        await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entrega!.Data.Id}/detalles", new
        {
            detallePedidoProveedorId = detallePedidoId,
            cantidadEntregada = 12
        });

        await AssertEstadoPedidoAsync(pedido.Data.Id, "ParcialmenteEntregado");

        // ---- Anulación de la entrega (B1): recalcula el pedido -> vuelve a ENVIADO,
        // era la única entrega ----
        var respuestaAnularEntrega = await _cliente.PostAsync($"/api/v1/entregas/{entrega.Data.Id}/anular", content: null);
        Assert.Equal(System.Net.HttpStatusCode.OK, respuestaAnularEntrega.StatusCode);
        await AssertEstadoPedidoAsync(pedido.Data.Id, "Enviado");

        // ---- Nueva entrega completa (20 de 20): la anterior anulada ya no cuenta ----
        var respuestaEntrega2 = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/entregas", new
        {
            numeroRemision = "REM-INTEGRADO-002"
        });
        var entrega2 = await respuestaEntrega2.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        var respuestaDetalleEntrega2 = await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entrega2!.Data.Id}/detalles", new
        {
            detallePedidoProveedorId = detallePedidoId,
            cantidadEntregada = 20
        });
        Assert.Equal(System.Net.HttpStatusCode.Created, respuestaDetalleEntrega2.StatusCode);
        await AssertEstadoPedidoAsync(pedido.Data.Id, "Entregado");

        // ---- Factura (12 de 20), luego anulada (B4), luego nueva factura con la cantidad
        // liberada (20 completos: la anulada ya no cuenta) ----
        var respuestaFactura1 = await _cliente.PostAsJsonAsync("/api/v1/facturas", new
        {
            proveedorId,
            pedidoProveedorId = pedido.Data.Id,
            numeroFactura = "F-INTEGRADO-001",
            impuestos = 0m
        });
        var factura1 = await respuestaFactura1.Content.ReadFromJsonAsync<Envoltorio<FacturaDto>>();
        await _cliente.PostAsJsonAsync($"/api/v1/facturas/{factura1!.Data.Id}/detalles", new
        {
            detallePedidoProveedorId = detallePedidoId,
            cantidadFacturada = 12,
            precioUnitario = 5m
        });

        var respuestaAnularFactura = await _cliente.PostAsync($"/api/v1/facturas/{factura1.Data.Id}/anular", content: null);
        Assert.Equal(System.Net.HttpStatusCode.OK, respuestaAnularFactura.StatusCode);

        var respuestaFactura2 = await _cliente.PostAsJsonAsync("/api/v1/facturas", new
        {
            proveedorId,
            pedidoProveedorId = pedido.Data.Id,
            numeroFactura = "F-INTEGRADO-002",
            impuestos = 0m
        });
        var factura2 = await respuestaFactura2.Content.ReadFromJsonAsync<Envoltorio<FacturaDto>>();
        var respuestaDetalleFactura2 = await _cliente.PostAsJsonAsync($"/api/v1/facturas/{factura2!.Data.Id}/detalles", new
        {
            detallePedidoProveedorId = detallePedidoId,
            cantidadFacturada = 20,
            precioUnitario = 5m
        });

        Assert.Equal(System.Net.HttpStatusCode.Created, respuestaDetalleFactura2.StatusCode);
        var conDetalleFactura2 = await respuestaDetalleFactura2.Content.ReadFromJsonAsync<Envoltorio<FacturaDto>>();
        Assert.Equal(20, conDetalleFactura2!.Data.Detalles[0].CantidadFacturada);
    }

    private async Task<int> CrearYAprobarRequisicionAsync(int empresaId, int periodoId, int productoId, int sedeId, int cantidad)
    {
        // RN-059/060 (punto 8): todas las rutas de Requisición exigen JWT + permiso + alcance
        // por empresa — este método se llama una vez por empresa (empresa1Id/empresa2Id), así
        // que "numero" se deriva de "empresaId" para que cada empresa tenga su propio
        // Usuario/token autorizado (nunca el mismo Usuario para dos empresas distintas).
        string token;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            token = await AutorizacionHelper.CrearTokenConPermisosAsync(
                _factory, db, empresaId, empresaId,
                "REQUISICION_CREAR", "REQUISICION_ENVIAR", "REQUISICION_APROBAR");
        }

        var crear = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requisiciones");
        crear.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        crear.Content = JsonContent.Create(new { periodoId });
        var respuestaCrear = await _cliente.SendAsync(crear);
        Assert.True(respuestaCrear.IsSuccessStatusCode, $"crear: {respuestaCrear.StatusCode} {await respuestaCrear.Content.ReadAsStringAsync()}");
        var requisicionId = (await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>())!.Data.Id;

        var agregarDetalle = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/detalles");
        agregarDetalle.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        agregarDetalle.Content = JsonContent.Create(new { productoId, cantidadSolicitada = cantidad, observacion = (string?)null });
        var respuestaDetalle = await _cliente.SendAsync(agregarDetalle);
        Assert.True(respuestaDetalle.IsSuccessStatusCode, $"detalle: {respuestaDetalle.StatusCode} {await respuestaDetalle.Content.ReadAsStringAsync()}");
        var detalleId = (await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>())!.Data.Detalles[0].Id;

        var distribuir = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/detalles/{detalleId}/distribuciones");
        distribuir.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        distribuir.Content = JsonContent.Create(new { sedeId, cantidad });
        var respuestaDistribucion = await _cliente.SendAsync(distribuir);
        Assert.True(respuestaDistribucion.IsSuccessStatusCode, $"distribucion: {respuestaDistribucion.StatusCode} {await respuestaDistribucion.Content.ReadAsStringAsync()}");

        var enviar = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/enviar");
        enviar.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuestaEnviar = await _cliente.SendAsync(enviar);
        Assert.True(respuestaEnviar.IsSuccessStatusCode, $"enviar: {respuestaEnviar.StatusCode} {await respuestaEnviar.Content.ReadAsStringAsync()}");

        var iniciarRevision = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/iniciar-revision");
        iniciarRevision.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuestaRevision = await _cliente.SendAsync(iniciarRevision);
        Assert.True(respuestaRevision.IsSuccessStatusCode, $"revision: {respuestaRevision.StatusCode} {await respuestaRevision.Content.ReadAsStringAsync()}");

        var aprobar = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/aprobar");
        aprobar.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        aprobar.Content = JsonContent.Create(new { observacion = (string?)null });
        var respuestaAprobar = await _cliente.SendAsync(aprobar);
        Assert.True(respuestaAprobar.IsSuccessStatusCode, $"aprobar: {respuestaAprobar.StatusCode} {await respuestaAprobar.Content.ReadAsStringAsync()}");

        return requisicionId;
    }

    private async Task AssertEstadoPedidoAsync(int pedidoId, string estadoEsperado)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var pedido = db.PedidosProveedor.Single(p => p.Id == pedidoId);
        Assert.Equal(estadoEsperado, pedido.Estado.ToString());
        await Task.CompletedTask;
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record RequisicionDto(int Id, IReadOnlyList<DetalleDto> Detalles);

    private sealed record DetalleDto(int Id);

    private sealed record PedidoProveedorDto(int Id, string Estado, IReadOnlyList<DetallePedidoProveedorDto> Detalles);

    private sealed record DetallePedidoProveedorDto(int Id);

    private sealed record EntregaDto(int Id);

    private sealed record FacturaDto(int Id, IReadOnlyList<DetalleFacturaDto> Detalles);

    private sealed record DetalleFacturaDto(int CantidadFacturada);
}
