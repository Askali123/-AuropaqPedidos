using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Facturas;
using AuropaqPedidos.Application.Facturas.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests;

public class RegistrarFacturaUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 2);

    private sealed class Escenario
    {
        public FakeFacturaRepository Facturas { get; } = new();
        public FakeProveedorRepository Proveedores { get; } = new();
        public FakePedidoProveedorRepository Pedidos { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();

        public Proveedor Proveedor { get; } = new(1, "Proveedor Uno");
        public PedidoProveedor Pedido { get; }

        public Escenario()
        {
            Proveedores.Agregar(Proveedor);

            var periodo = new Periodo(
                1, 2026, 9,
                new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
                new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
                "ABIERTO");
            var empresa = new Empresa(1, "Empresa 1");
            var producto = new Producto(1, "Papel higiénico", new Categoria(1, "Aseo"), new UnidadMedida(1, "UNIDAD", "Unidad"));
            var requisicion = new Requisicion(1, empresa, periodo, 10, Fecha);
            var detalleReq = requisicion.AgregarDetalle(1, producto, 100);
            var sede = new Sede(1, empresa, "Sede 1");
            requisicion.AgregarDistribucion(1, detalleReq, sede, 100);
            requisicion.Enviar(1, Fecha);
            requisicion.IniciarRevision(1, Fecha);
            requisicion.Aprobar(1, Fecha);

            var consolidacion = new Consolidacion(1, periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
            consolidacion.AgregarAsignacion(1, 1, requisicion, detalleReq, 100);

            Pedido = new PedidoProveedor(1, consolidacion, Proveedor, "PO-001", usuarioCreacionId: 10, Fecha);
            Pedido.AgregarDetalle(1, consolidacion.Detalles[0], 100);
            Pedidos.Guardar(Pedido);
        }

        public RegistrarFacturaUseCase CrearUseCase() => new(Facturas, Proveedores, Pedidos, Ids, NullLogger<RegistrarFacturaUseCase>.Instance);

        public RegistrarFacturaRequest CrearRequest(int? proveedorId = null, int? pedidoId = null) => new(
            ProveedorId: proveedorId ?? Proveedor.Id,
            PedidoProveedorId: pedidoId ?? Pedido.Id,
            NumeroFactura: "F-001",
            Impuestos: 19m);
    }

    [Fact]
    public void Registra_una_factura_para_un_proveedor_y_pedido_existentes()
    {
        var escenario = new Escenario();

        var respuesta = escenario.CrearUseCase().Ejecutar(Fecha, escenario.CrearRequest(), usuarioId: 10);

        Assert.Equal(escenario.Proveedor.Id, respuesta.ProveedorId);
        Assert.Equal(escenario.Pedido.Id, respuesta.PedidoProveedorId);
        Assert.Equal("F-001", respuesta.NumeroFactura);
        Assert.Equal(10, respuesta.UsuarioCreacionId);
        Assert.Equal(0m, respuesta.Subtotal);
        Assert.Equal(19m, respuesta.Impuestos);
        Assert.Equal(19m, respuesta.Total);
        Assert.Empty(respuesta.Detalles);
        Assert.NotNull(escenario.Facturas.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Proveedor_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() =>
            escenario.CrearUseCase().Ejecutar(Fecha, escenario.CrearRequest(proveedorId: 999), usuarioId: 10));
    }

    [Fact]
    public void Proveedor_inactivo_no_puede_recibir_una_factura()
    {
        var escenario = new Escenario();
        escenario.Proveedor.Desactivar();

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.CrearUseCase().Ejecutar(Fecha, escenario.CrearRequest(), usuarioId: 10));
    }

    [Fact]
    public void Pedido_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() =>
            escenario.CrearUseCase().Ejecutar(Fecha, escenario.CrearRequest(pedidoId: 999), usuarioId: 10));
    }

    [Fact]
    public void Proveedor_que_no_corresponde_al_pedido_lanza_regla_de_negocio()
    {
        var escenario = new Escenario();
        var otroProveedor = new Proveedor(2, "Otro proveedor");
        escenario.Proveedores.Agregar(otroProveedor);

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.CrearUseCase().Ejecutar(Fecha, escenario.CrearRequest(proveedorId: otroProveedor.Id), usuarioId: 10));
    }

    // D-09/RN-049 (cierre documental 2026-09-11): NumeroFactura único dentro del proveedor.
    [Fact]
    public void No_permite_dos_facturas_con_el_mismo_numero_para_el_mismo_proveedor()
    {
        var escenario = new Escenario();
        escenario.CrearUseCase().Ejecutar(Fecha, escenario.CrearRequest(), usuarioId: 10);

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.CrearUseCase().Ejecutar(Fecha, escenario.CrearRequest(), usuarioId: 10));
    }
}
