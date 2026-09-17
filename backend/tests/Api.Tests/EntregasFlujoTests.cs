using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de Entrega (creación por HTTP, 2026-09-11: crear cabecera, agregar
// detalle, agregar distribución, anular — D-04/RN-046) a través de la Api real (Controllers +
// Application + Infrastructure + SQL Server), según 03-arquitectura.md §42.
//
// Autorización real agregada 2026-09-17 (RN-063/ADR-066): estos endpoints ya exigen JWT +
// PEDIDO_*/ENTREGA_* (sin alcance por empresa, CLAUDE.md §27). Este archivo prueba comportamiento
// de negocio, no autorización granular — un único token con todos los permisos necesarios,
// autenticado en `_cliente` por defecto desde `NuevoEscenarioAsync`, alcanza para todo el flujo.
public sealed class EntregasFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public EntregasFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<EscenarioPedidoProveedor> NuevoEscenarioAsync(int numero, int cantidadNecesaria = 85)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await EscenarioPedidoProveedor.CrearAsync(db, numero, cantidadNecesaria);

        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, numero, escenario.EmpresaId,
            "PEDIDO_CREAR", "PEDIDO_ENVIAR", "PEDIDO_CERRAR", "PEDIDO_CANCELAR",
            "ENTREGA_REGISTRAR", "ENTREGA_ANULAR", "ENTREGA_VER");
        _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return escenario;
    }

    // Crea, agrega detalle (cantidadPedida) y envía un PedidoProveedor por HTTP — prerrequisito
    // (D-04/RN-046) para poder registrar una Entrega.
    private async Task<(int PedidoId, int DetallePedidoId)> CrearYEnviarPedidoAsync(
        EscenarioPedidoProveedor escenario, string numeroPedido, int cantidadPedida)
    {
        var respuestaCrear = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = escenario.ConsolidacionId,
            proveedorId = escenario.ProveedorId,
            numeroPedido
        });
        var pedido = await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();

        var respuestaDetalle = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido!.Data.Id}/detalles", new
        {
            detalleConsolidacionId = escenario.DetalleConsolidacionId,
            cantidadPedida
        });
        var conDetalle = await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        var detalleId = conDetalle!.Data.Detalles[0].Id;

        // RN-065/D-18 (2026-09-17): distribución completa exigida antes de enviar.
        await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/detalles/{detalleId}/distribuciones", new
        {
            sedeId = escenario.SedeId,
            cantidad = cantidadPedida
        });

        await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/enviar", content: null);

        return (pedido.Data.Id, detalleId);
    }

    [Fact]
    public async Task Crear_entrega_para_un_pedido_enviado_devuelve_201()
    {
        var escenario = await NuevoEscenarioAsync(1);
        var (pedidoId, _) = await CrearYEnviarPedidoAsync(escenario, "PO-001", cantidadPedida: 100);

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new
        {
            numeroRemision = "REM-001"
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var entrega = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        Assert.Equal(pedidoId, entrega!.Data.PedidoProveedorId);
        Assert.Equal("REM-001", entrega.Data.NumeroRemision);
        Assert.Equal("Registrada", entrega.Data.Estado);
        Assert.Empty(entrega.Data.Detalles);
    }

    [Fact]
    public async Task Obtener_entrega_por_id_devuelve_los_mismos_datos_que_la_creacion()
    {
        var escenario = await NuevoEscenarioAsync(14);
        var (pedidoId, _) = await CrearYEnviarPedidoAsync(escenario, "PO-014", cantidadPedida: 100);
        var creada = await (await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new
        {
            numeroRemision = "REM-014"
        })).Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();

        var respuesta = await _cliente.GetAsync($"/api/v1/entregas/{creada!.Data.Id}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var entrega = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        Assert.Equal("REM-014", entrega!.Data.NumeroRemision);
    }

    [Fact]
    public async Task Obtener_entrega_inexistente_devuelve_404()
    {
        await NuevoEscenarioAsync(15); // solo para autenticar _cliente.

        var respuesta = await _cliente.GetAsync("/api/v1/entregas/999999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_entrega_para_pedido_inexistente_devuelve_404()
    {
        await NuevoEscenarioAsync(101); // solo para autenticar _cliente.

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor/999999/entregas", new
        {
            numeroRemision = "REM-001"
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Crear_entrega_sin_numero_de_remision_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(2);
        var (pedidoId, _) = await CrearYEnviarPedidoAsync(escenario, "PO-002", cantidadPedida: 100);

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new
        {
            numeroRemision = ""
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Crear_entrega_para_un_pedido_todavia_en_borrador_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(3);

        var respuestaCrear = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = escenario.ConsolidacionId,
            proveedorId = escenario.ProveedorId,
            numeroPedido = "PO-003"
        });
        var pedido = await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        // Sin enviar: el pedido sigue en BORRADOR (D-04/RN-046 exige ENVIADO/PARCIALMENTE_ENTREGADO).

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido!.Data.Id}/entregas", new
        {
            numeroRemision = "REM-003"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Agregar_detalle_con_cantidad_valida_devuelve_201_y_dista_como_parcialmente_entregado()
    {
        var escenario = await NuevoEscenarioAsync(4);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-004", cantidadPedida: 100);
        var respuestaEntrega = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-004" });
        var entrega = await respuestaEntrega.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entrega!.Data.Id}/detalles", new
        {
            detallePedidoProveedorId = detalleId,
            cantidadEntregada = 60
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var conDetalle = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        var detalle = Assert.Single(conDetalle!.Data.Detalles);
        Assert.Equal(60, detalle.CantidadEntregada);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var pedidoActualizado = db.PedidosProveedor.Single(p => p.Id == pedidoId);
        Assert.Equal("ParcialmenteEntregado", pedidoActualizado.Estado.ToString());
    }

    [Fact]
    public async Task Agregar_detalle_con_cantidad_superior_a_la_pedida_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(5);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-005", cantidadPedida: 100);
        var respuestaEntrega = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-005" });
        var entrega = await respuestaEntrega.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entrega!.Data.Id}/detalles", new
        {
            detallePedidoProveedorId = detalleId,
            cantidadEntregada = 150
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    // Sección 3 de la tarea: entrega parcial + múltiples entregas para el mismo pedido; el
    // pedido termina ENTREGADO cuando la segunda entrega completa la cantidad pedida.
    [Fact]
    public async Task Dos_entregas_parciales_completan_el_pedido_y_lo_dejan_entregado()
    {
        var escenario = await NuevoEscenarioAsync(6);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-006", cantidadPedida: 100);

        var respuestaEntrega1 = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-006-1" });
        var entrega1 = await respuestaEntrega1.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entrega1!.Data.Id}/detalles", new { detallePedidoProveedorId = detalleId, cantidadEntregada = 60 });

        var respuestaEntrega2 = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-006-2" });
        var entrega2 = await respuestaEntrega2.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        var respuestaDetalle2 = await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entrega2!.Data.Id}/detalles", new { detallePedidoProveedorId = detalleId, cantidadEntregada = 40 });

        Assert.Equal(HttpStatusCode.Created, respuestaDetalle2.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var pedidoActualizado = db.PedidosProveedor.Single(p => p.Id == pedidoId);
        Assert.Equal("Entregado", pedidoActualizado.Estado.ToString());
    }

    [Fact]
    public async Task Agregar_distribucion_de_entrega_devuelve_201_con_snapshot_historico()
    {
        var escenario = await NuevoEscenarioAsync(7);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-007", cantidadPedida: 100);
        var respuestaEntrega = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-007" });
        var entrega = await respuestaEntrega.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        var respuestaDetalle = await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entrega!.Data.Id}/detalles", new { detallePedidoProveedorId = detalleId, cantidadEntregada = 60 });
        var conDetalle = await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        var detalleEntregaId = conDetalle!.Data.Detalles[0].Id;

        var respuesta = await _cliente.PostAsJsonAsync(
            $"/api/v1/entregas/{entrega.Data.Id}/detalles/{detalleEntregaId}/distribuciones", new
            {
                sedeId = escenario.SedeId,
                cantidad = 60
            });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var conDistribucion = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        var distribucion = Assert.Single(conDistribucion!.Data.Detalles[0].Distribuciones);
        Assert.Equal(escenario.SedeId, distribucion.SedeId);
        Assert.Equal(60, distribucion.Cantidad);
    }

    // D-04/RN-046: REGISTRADA -> ANULADA.
    [Fact]
    public async Task Anular_una_entrega_registrada_devuelve_200_y_queda_anulada()
    {
        var escenario = await NuevoEscenarioAsync(8);
        var (pedidoId, _) = await CrearYEnviarPedidoAsync(escenario, "PO-008", cantidadPedida: 100);
        var respuestaEntrega = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-008" });
        var entrega = await respuestaEntrega.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();

        var respuesta = await _cliente.PostAsync($"/api/v1/entregas/{entrega!.Data.Id}/anular", content: null);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var anulada = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        Assert.Equal("Anulada", anulada!.Data.Estado);
    }

    [Fact]
    public async Task Anular_una_entrega_ya_anulada_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(9);
        var (pedidoId, _) = await CrearYEnviarPedidoAsync(escenario, "PO-009", cantidadPedida: 100);
        var respuestaEntrega = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-009" });
        var entrega = await respuestaEntrega.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        await _cliente.PostAsync($"/api/v1/entregas/{entrega!.Data.Id}/anular", content: null);

        var respuesta = await _cliente.PostAsync($"/api/v1/entregas/{entrega.Data.Id}/anular", content: null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Anular_una_entrega_inexistente_devuelve_404()
    {
        await NuevoEscenarioAsync(102); // solo para autenticar _cliente.

        var respuesta = await _cliente.PostAsync("/api/v1/entregas/999999/anular", content: null);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    // B1 (cierre técnico 2026-09-11): anular una entrega recalcula PedidoProveedor.Estado.
    [Fact]
    public async Task Anular_la_unica_entrega_de_un_pedido_parcialmente_entregado_lo_revierte_a_enviado()
    {
        var escenario = await NuevoEscenarioAsync(10);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-010", cantidadPedida: 100);
        var respuestaEntrega = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-010" });
        var entrega = await respuestaEntrega.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entrega!.Data.Id}/detalles", new { detallePedidoProveedorId = detalleId, cantidadEntregada = 60 });

        await _cliente.PostAsync($"/api/v1/entregas/{entrega.Data.Id}/anular", content: null);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var pedidoActualizado = db.PedidosProveedor.Single(p => p.Id == pedidoId);
        Assert.Equal("Enviado", pedidoActualizado.Estado.ToString());
    }

    // Regla adicional (B1): una entrega no puede anularse si el PedidoProveedor ya está CERRADO.
    [Fact]
    public async Task No_permite_anular_una_entrega_de_un_pedido_ya_cerrado()
    {
        var escenario = await NuevoEscenarioAsync(11);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-011", cantidadPedida: 85);
        var respuestaEntrega = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-011" });
        var entrega = await respuestaEntrega.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entrega!.Data.Id}/detalles", new { detallePedidoProveedorId = detalleId, cantidadEntregada = 85 });
        await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedidoId}/cerrar", content: null);

        var respuesta = await _cliente.PostAsync($"/api/v1/entregas/{entrega.Data.Id}/anular", content: null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    // RN-054 (cierre 2026-09-11): un pedido CANCELADO queda fuera de operación.
    [Fact]
    public async Task No_permite_crear_una_entrega_para_un_pedido_cancelado()
    {
        var escenario = await NuevoEscenarioAsync(12);
        var (pedidoId, _) = await CrearYEnviarPedidoAsync(escenario, "PO-012", cantidadPedida: 100);
        await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedidoId}/cancelar", content: null);

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-012" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task No_permite_anular_una_entrega_de_un_pedido_cancelado()
    {
        var escenario = await NuevoEscenarioAsync(13);
        var (pedidoId, _) = await CrearYEnviarPedidoAsync(escenario, "PO-013", cantidadPedida: 100);
        var respuestaEntrega = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision = "REM-013" });
        var entrega = await respuestaEntrega.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedidoId}/cancelar", content: null);

        var respuesta = await _cliente.PostAsync($"/api/v1/entregas/{entrega!.Data.Id}/anular", content: null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record PedidoProveedorDto(int Id, string NumeroPedido, string Estado, IReadOnlyList<DetallePedidoProveedorDto> Detalles);

    private sealed record DetallePedidoProveedorDto(int Id, int CantidadNecesaria, int CantidadPedida);

    private sealed record EntregaDto(
        int Id, int PedidoProveedorId, string NumeroRemision, string Estado, IReadOnlyList<DetalleEntregaDto> Detalles);

    private sealed record DetalleEntregaDto(
        int Id, int DetallePedidoProveedorId, int ProductoId, int CantidadEntregada, IReadOnlyList<DistribucionEntregaDto> Distribuciones);

    private sealed record DistribucionEntregaDto(int Id, int SedeId, int Cantidad, string? CiudadEntrega);
}
