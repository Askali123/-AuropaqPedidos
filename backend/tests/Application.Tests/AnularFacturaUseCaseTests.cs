using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Facturas;
using AuropaqPedidos.Application.Facturas.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests;

// D-05/RN-047 (01-reglas-negocio.md §13, cierre técnico 2026-09-11): REGISTRADA -> ANULADA.
public class AnularFacturaUseCaseTests
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

        public AnularFacturaUseCase AnularUseCase() => new(Facturas, NullLogger<AnularFacturaUseCase>.Instance);

        public FacturaResponse RegistrarFactura(string numeroFactura = "F-001")
        {
            var registrarUseCase = new RegistrarFacturaUseCase(Facturas, Proveedores, Pedidos, Ids, NullLogger<RegistrarFacturaUseCase>.Instance);
            return registrarUseCase.Ejecutar(Fecha, new RegistrarFacturaRequest(Proveedor.Id, Pedido.Id, numeroFactura, Impuestos: 19m), usuarioId: 10);
        }
    }

    [Fact]
    public void Anulacion_correcta_desde_registrada()
    {
        var escenario = new Escenario();
        var factura = escenario.RegistrarFactura();

        var respuesta = escenario.AnularUseCase().Ejecutar(factura.Id);

        Assert.Equal(FacturaEstado.Anulada.ToString(), respuesta.Estado);
    }

    [Fact]
    public void No_permite_anular_una_factura_ya_anulada()
    {
        var escenario = new Escenario();
        var factura = escenario.RegistrarFactura();
        escenario.AnularUseCase().Ejecutar(factura.Id);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.AnularUseCase().Ejecutar(factura.Id));
    }

    [Fact]
    public void Anular_factura_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.AnularUseCase().Ejecutar(999));
    }
}
