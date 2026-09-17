using Application.Tests.Fakes;
using AuropaqPedidos.Application.Entregas;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests;

// D-08/RN-044 (01-reglas-negocio.md §11, cierre documental 2026-09-11): ENTREGADO -> CERRADO,
// sin requerir Factura (RN-040). También cubre Enviar/Cancelar (D-03/RN-043).
public class CerrarPedidoProveedorUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 2);

    private sealed class Escenario
    {
        public FakeRequisicionRepository Requisiciones { get; } = new();
        public FakeConsolidacionRepository Consolidaciones { get; } = new();
        public FakePedidoProveedorRepository Pedidos { get; } = new();
        public FakeEntregaRepository Entregas { get; } = new();
        public FakePeriodoRepository Periodos { get; } = new();
        public FakeProveedorRepository Proveedores { get; } = new();
        public FakeSedeRepository Sedes { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();
        public FakeTransaccionDeEntrega Transaccion { get; } = new();
        public FakeAuditoriaRepository Auditoria { get; } = new();

        public Periodo Periodo { get; } = new(
            1, 2026, 9,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
            "ABIERTO");

        public Proveedor Proveedor { get; } = new(1, "Proveedor Uno");

        public Escenario()
        {
            Periodos.Agregar(Periodo);
            Proveedores.Agregar(Proveedor);
        }

        public CerrarPedidoProveedorUseCase CerrarUseCase() => new(Pedidos, NullLogger<CerrarPedidoProveedorUseCase>.Instance);
        public EnviarPedidoProveedorUseCase EnviarUseCase() => new(Pedidos);
        public CancelarPedidoProveedorUseCase CancelarUseCase() => new(Pedidos, NullLogger<CancelarPedidoProveedorUseCase>.Instance);
        public AgregarDetalleEntregaUseCase AgregarDetalleEntregaUseCase() => new(Entregas, Pedidos, Ids, Transaccion);

        public Producto CrearProducto(string nombre) =>
            new(Ids.Siguiente(), nombre, new Categoria(Ids.Siguiente(), "Aseo"), new UnidadMedida(Ids.Siguiente(), "UNIDAD", "Unidad"));

        // Crea un pedido ENVIADO con un único DetallePedidoProveedor de "cantidadPedida" unidades.
        public PedidoProveedorResponse CrearPedidoEnviado(int cantidadPedida = 100)
        {
            var producto = CrearProducto("Papel higiénico");
            var empresa = new Empresa(Ids.Siguiente(), "Empresa 1");
            var requisicion = new Requisicion(Ids.Siguiente(), empresa, Periodo, 10, Fecha);
            var detalleReq = requisicion.AgregarDetalle(Ids.Siguiente(), producto, cantidadPedida);
            var sede = new Sede(Ids.Siguiente(), empresa, "Sede 1");
            requisicion.AgregarDistribucion(Ids.Siguiente(), detalleReq, sede, cantidadPedida);
            requisicion.Enviar(1, Fecha);
            requisicion.IniciarRevision(1, Fecha);
            requisicion.Aprobar(1, Fecha);
            Requisiciones.Guardar(requisicion);

            var consolidacion = new Consolidacion(Ids.Siguiente(), Periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
            consolidacion.AgregarAsignacion(Ids.Siguiente(), Ids.Siguiente(), requisicion, detalleReq, cantidadPedida);
            Consolidaciones.Guardar(consolidacion);

            var pedidoUseCase = new CrearPedidoProveedorUseCase(
                Pedidos, Consolidaciones, Proveedores, Auditoria, Ids, NullLogger<CrearPedidoProveedorUseCase>.Instance);
            var pedido = pedidoUseCase.Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, Proveedor.Id, "PO-001"), usuarioId: 10);

            var detallePedidoUseCase = new AgregarDetallePedidoProveedorUseCase(Pedidos, Ids);
            pedido = detallePedidoUseCase.Ejecutar(pedido.Id, new AgregarDetallePedidoProveedorRequest(consolidacion.Detalles[0].Id, cantidadPedida));

            // RN-065/D-18 (2026-09-17): distribución completa exigida antes de enviar.
            Sedes.Agregar(sede);
            var distribucionUseCase = new AgregarDistribucionPedidoUseCase(Pedidos, Sedes, Ids);
            pedido = distribucionUseCase.Ejecutar(pedido.Id, pedido.Detalles[0].Id, sede.Id, cantidadPedida);

            return EnviarUseCase().Ejecutar(pedido.Id);
        }

        // Entrega completa (cantidadPedida completa) para dejar el pedido en ENTREGADO.
        public void CompletarEntrega(PedidoProveedorResponse pedido)
        {
            var crearEntregaUseCase = new CrearEntregaUseCase(Entregas, Pedidos, Auditoria, Ids, NullLogger<CrearEntregaUseCase>.Instance);
            var entrega = crearEntregaUseCase.Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"), usuarioId: 10);
            AgregarDetalleEntregaUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(pedido.Detalles[0].Id, pedido.Detalles[0].CantidadPedida));
        }
    }

    [Fact]
    public void Cierre_correcto_desde_entregado()
    {
        var escenario = new Escenario();
        var pedido = escenario.CrearPedidoEnviado();
        escenario.CompletarEntrega(pedido);

        var respuesta = escenario.CerrarUseCase().Ejecutar(pedido.Id);

        Assert.Equal(PedidoProveedorEstado.Cerrado.ToString(), respuesta.Estado);
    }

    [Fact]
    public void Cierre_sin_factura_es_valido()
    {
        // Ningún IFacturaRepository se inyecta a CerrarPedidoProveedorUseCase (RN-040): el
        // cierre no consulta facturas en absoluto.
        var escenario = new Escenario();
        var pedido = escenario.CrearPedidoEnviado();
        escenario.CompletarEntrega(pedido);

        var respuesta = escenario.CerrarUseCase().Ejecutar(pedido.Id);

        Assert.Equal(PedidoProveedorEstado.Cerrado.ToString(), respuesta.Estado);
    }

    [Fact]
    public void Cierre_con_cantidad_pendiente_lanza_regla_de_negocio()
    {
        var escenario = new Escenario();
        var pedido = escenario.CrearPedidoEnviado(cantidadPedida: 100);

        var crearEntregaUseCase = new CrearEntregaUseCase(
            escenario.Entregas, escenario.Pedidos, escenario.Auditoria, escenario.Ids, NullLogger<CrearEntregaUseCase>.Instance);
        var entrega = crearEntregaUseCase.Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"), usuarioId: 10);
        escenario.AgregarDetalleEntregaUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(pedido.Detalles[0].Id, 60));

        Assert.Throws<ReglaDeNegocioException>(() => escenario.CerrarUseCase().Ejecutar(pedido.Id));
    }

    [Fact]
    public void Cierre_desde_estado_invalido_lanza_regla_de_negocio()
    {
        var escenario = new Escenario();
        var pedido = escenario.CrearPedidoEnviado();

        // Pedido apenas ENVIADO (sin ninguna entrega registrada todavía): no está ENTREGADO.
        Assert.Throws<ReglaDeNegocioException>(() => escenario.CerrarUseCase().Ejecutar(pedido.Id));
    }

    [Fact]
    public void Cierre_repetido_lanza_regla_de_negocio()
    {
        var escenario = new Escenario();
        var pedido = escenario.CrearPedidoEnviado();
        escenario.CompletarEntrega(pedido);
        escenario.CerrarUseCase().Ejecutar(pedido.Id);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.CerrarUseCase().Ejecutar(pedido.Id));
    }

    [Fact]
    public void Cerrar_pedido_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.CerrarUseCase().Ejecutar(999));
    }

    [Fact]
    public void Cancelar_un_pedido_recien_creado_es_valido()
    {
        var escenario = new Escenario();
        var pedido = escenario.CrearPedidoEnviado();

        var respuesta = escenario.CancelarUseCase().Ejecutar(pedido.Id);

        Assert.Equal(PedidoProveedorEstado.Cancelado.ToString(), respuesta.Estado);
    }

    [Fact]
    public void No_permite_cancelar_un_pedido_ya_cerrado()
    {
        var escenario = new Escenario();
        var pedido = escenario.CrearPedidoEnviado();
        escenario.CompletarEntrega(pedido);
        escenario.CerrarUseCase().Ejecutar(pedido.Id);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.CancelarUseCase().Ejecutar(pedido.Id));
    }
}
