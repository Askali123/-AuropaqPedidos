using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de Factura (incremento MVP, TASK-046) a través de la Api real
// (Controllers + Application + Infrastructure + SQL Server), según 03-arquitectura.md §42.
//
// Autorización real agregada 2026-09-17 (RN-063/ADR-066): estos endpoints ya exigen JWT +
// FACTURA_REGISTRAR/ANULAR (sin alcance por empresa, CLAUDE.md §27). Este archivo prueba
// comportamiento de negocio, no autorización granular — un único token, autenticado en
// `_cliente` por defecto desde `NuevoEscenarioAsync`, alcanza para todo el flujo.
public sealed class FacturasFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public FacturasFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<EscenarioFactura> NuevoEscenarioAsync(int numero, int cantidadPedida = 100)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await EscenarioFactura.CrearAsync(db, numero, cantidadPedida);

        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, numero, escenario.EmpresaId, "FACTURA_REGISTRAR", "FACTURA_ANULAR", "FACTURA_VER");
        _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return escenario;
    }

    // Solo para autenticar `_cliente` en pruebas que no necesitan ningún grafo de datos (ej. 404
    // contra un id inexistente) — evita pasar por EscenarioFactura.CrearAsync (que siembra un
    // Periodo con Anio fijo 2026, cuyo Mes ya está agotado por los `numero` 1-11 usados arriba;
    // reutilizarlo con otro `numero` produciría un choque de UNIQUE(Anio, Mes)).
    private async Task AutenticarSoloAsync(int numero)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresa = new Empresa(numero, $"Empresa autorizacion {numero}");
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync();

        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, numero, empresa.Id, "FACTURA_REGISTRAR", "FACTURA_ANULAR", "FACTURA_VER");
        _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Registrar_factura_y_agregar_detalle_calcula_totales_correctamente()
    {
        var escenario = await NuevoEscenarioAsync(1);

        var respuestaRegistrar = await _cliente.PostAsJsonAsync("/api/v1/facturas", new
        {
            proveedorId = escenario.ProveedorId,
            pedidoProveedorId = escenario.PedidoProveedorId,
            numeroFactura = "F-001",
            impuestos = 19m
        });
        Assert.Equal(HttpStatusCode.Created, respuestaRegistrar.StatusCode);
        var factura = await respuestaRegistrar.Content.ReadFromJsonAsync<Envoltorio<FacturaDto>>();
        Assert.Equal(escenario.ProveedorId, factura!.Data.ProveedorId);
        Assert.Equal(escenario.PedidoProveedorId, factura.Data.PedidoProveedorId);
        Assert.Equal(0m, factura.Data.Subtotal);
        Assert.Equal(19m, factura.Data.Total);

        var respuestaDetalle = await _cliente.PostAsJsonAsync($"/api/v1/facturas/{factura.Data.Id}/detalles", new
        {
            detallePedidoProveedorId = escenario.DetallePedidoProveedorId,
            cantidadFacturada = 60,
            precioUnitario = 12.5m
        });
        Assert.Equal(HttpStatusCode.Created, respuestaDetalle.StatusCode);
        var conDetalle = await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<FacturaDto>>();

        var detalle = Assert.Single(conDetalle!.Data.Detalles);
        Assert.Equal(escenario.DetallePedidoProveedorId, detalle.DetallePedidoProveedorId);
        Assert.Equal(60, detalle.CantidadFacturada);
        Assert.Equal(750m, detalle.Subtotal);
        Assert.Equal(750m, conDetalle.Data.Subtotal);
        Assert.Equal(769m, conDetalle.Data.Total);
    }

    [Fact]
    public async Task Listar_facturas_de_un_pedido_devuelve_las_registradas()
    {
        var escenario = await NuevoEscenarioAsync(12);
        var creada = await (await _cliente.PostAsJsonAsync("/api/v1/facturas", new
        {
            proveedorId = escenario.ProveedorId,
            pedidoProveedorId = escenario.PedidoProveedorId,
            numeroFactura = "F-012",
            impuestos = 0m
        })).Content.ReadFromJsonAsync<Envoltorio<FacturaDto>>();

        var respuesta = await _cliente.GetAsync($"/api/v1/facturas?pedidoProveedorId={escenario.PedidoProveedorId}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var lista = await respuesta.Content.ReadFromJsonAsync<Envoltorio<IReadOnlyList<FacturaDto>>>();
        Assert.Contains(lista!.Data, f => f.Id == creada!.Data.Id);
    }

    [Fact]
    public async Task Obtener_factura_inexistente_devuelve_404()
    {
        await AutenticarSoloAsync(103);

        var respuesta = await _cliente.GetAsync("/api/v1/facturas/999999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Registrar_factura_con_proveedor_inexistente_devuelve_404()
    {
        var escenario = await NuevoEscenarioAsync(2);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/facturas", new
        {
            proveedorId = 999999,
            pedidoProveedorId = escenario.PedidoProveedorId,
            numeroFactura = "F-002",
            impuestos = 0m
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Registrar_factura_con_pedido_inexistente_devuelve_404()
    {
        var escenario = await NuevoEscenarioAsync(3);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/facturas", new
        {
            proveedorId = escenario.ProveedorId,
            pedidoProveedorId = 999999,
            numeroFactura = "F-003",
            impuestos = 0m
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Registrar_factura_con_proveedor_que_no_corresponde_al_pedido_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(4);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/facturas", new
        {
            proveedorId = escenario.OtroProveedorId,
            pedidoProveedorId = escenario.PedidoProveedorId,
            numeroFactura = "F-004",
            impuestos = 0m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Agregar_detalle_a_factura_inexistente_devuelve_404()
    {
        await AutenticarSoloAsync(101);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/facturas/999999/detalles", new
        {
            detallePedidoProveedorId = 1,
            cantidadFacturada = 10,
            precioUnitario = 5m
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Agregar_detalle_inexistente_devuelve_404()
    {
        var escenario = await NuevoEscenarioAsync(5);
        var facturaId = await RegistrarFacturaAsync(escenario, "F-005");

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/facturas/{facturaId}/detalles", new
        {
            detallePedidoProveedorId = 999999,
            cantidadFacturada = 10,
            precioUnitario = 5m
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Agregar_detalle_perteneciente_a_otro_pedido_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(6);
        var otroEscenario = await NuevoEscenarioAsync(7);
        var facturaId = await RegistrarFacturaAsync(escenario, "F-006");

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/facturas/{facturaId}/detalles", new
        {
            detallePedidoProveedorId = otroEscenario.DetallePedidoProveedorId,
            cantidadFacturada = 10,
            precioUnitario = 5m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Agregar_detalle_con_cantidad_invalida_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(8);
        var facturaId = await RegistrarFacturaAsync(escenario, "F-008");

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/facturas/{facturaId}/detalles", new
        {
            detallePedidoProveedorId = escenario.DetallePedidoProveedorId,
            cantidadFacturada = 0,
            precioUnitario = 5m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Agregar_detalle_con_cantidad_acumulada_mayor_a_la_pedida_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(9, cantidadPedida: 100);
        var primeraFacturaId = await RegistrarFacturaAsync(escenario, "F-009-1");
        await _cliente.PostAsJsonAsync($"/api/v1/facturas/{primeraFacturaId}/detalles", new
        {
            detallePedidoProveedorId = escenario.DetallePedidoProveedorId,
            cantidadFacturada = 70,
            precioUnitario = 5m
        });

        var segundaFacturaId = await RegistrarFacturaAsync(escenario, "F-009-2");
        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/facturas/{segundaFacturaId}/detalles", new
        {
            detallePedidoProveedorId = escenario.DetallePedidoProveedorId,
            cantidadFacturada = 40,
            precioUnitario = 5m
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    // D-05/RN-047 (cierre técnico 2026-09-11): REGISTRADA -> ANULADA.
    [Fact]
    public async Task Anular_una_factura_registrada_devuelve_200_y_queda_anulada()
    {
        var escenario = await NuevoEscenarioAsync(10);
        var facturaId = await RegistrarFacturaAsync(escenario, "F-010");

        var respuesta = await _cliente.PostAsync($"/api/v1/facturas/{facturaId}/anular", content: null);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var factura = await respuesta.Content.ReadFromJsonAsync<Envoltorio<FacturaDto>>();
        Assert.Equal("Anulada", factura!.Data.Estado);
    }

    [Fact]
    public async Task Anular_una_factura_ya_anulada_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(11);
        var facturaId = await RegistrarFacturaAsync(escenario, "F-011");
        await _cliente.PostAsync($"/api/v1/facturas/{facturaId}/anular", content: null);

        var respuesta = await _cliente.PostAsync($"/api/v1/facturas/{facturaId}/anular", content: null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Anular_una_factura_inexistente_devuelve_404()
    {
        await AutenticarSoloAsync(102);

        var respuesta = await _cliente.PostAsync("/api/v1/facturas/999999/anular", content: null);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    private async Task<int> RegistrarFacturaAsync(EscenarioFactura escenario, string numeroFactura)
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/facturas", new
        {
            proveedorId = escenario.ProveedorId,
            pedidoProveedorId = escenario.PedidoProveedorId,
            numeroFactura,
            impuestos = 19m
        });
        var factura = await respuesta.Content.ReadFromJsonAsync<Envoltorio<FacturaDto>>();
        return factura!.Data.Id;
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record FacturaDto(
        int Id,
        int ProveedorId,
        int PedidoProveedorId,
        string NumeroFactura,
        decimal Subtotal,
        decimal Impuestos,
        decimal Total,
        string Estado,
        IReadOnlyList<DetalleFacturaDto> Detalles);

    private sealed record DetalleFacturaDto(
        int Id, int DetallePedidoProveedorId, int ProductoId, int CantidadFacturada, decimal PrecioUnitario, decimal Subtotal);
}
