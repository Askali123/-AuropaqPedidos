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

// D-04/RN-046 (01-reglas-negocio.md §12) + B1 (cierre técnico 2026-09-11): REGISTRADA -> ANULADA,
// con recálculo del estado del PedidoProveedor según las entregas VÁLIDAS restantes, y bloqueo
// cuando el pedido ya está CERRADO.
public class AnularEntregaUseCaseTests
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

        public AnularEntregaUseCase AnularUseCase() => new(Entregas, Pedidos, Transaccion, NullLogger<AnularEntregaUseCase>.Instance);
        public AgregarDetalleEntregaUseCase AgregarDetalleUseCase() => new(Entregas, Pedidos, Ids, Transaccion);
        public CerrarPedidoProveedorUseCase CerrarUseCase() => new(Pedidos, NullLogger<CerrarPedidoProveedorUseCase>.Instance);
        public CancelarPedidoProveedorUseCase CancelarUseCase() => new(Pedidos, NullLogger<CancelarPedidoProveedorUseCase>.Instance);

        // Pedido ENVIADO con un único DetallePedidoProveedor de "cantidadPedida" unidades.
        public (PedidoProveedorResponse Pedido, int DetallePedidoId) CrearPedidoEnviado(int cantidadPedida = 100)
        {
            var producto = new Producto(Ids.Siguiente(), "Papel higiénico", new Categoria(Ids.Siguiente(), "Aseo"), new UnidadMedida(Ids.Siguiente(), "UNIDAD", "Unidad"));
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
            var pedido = pedidoUseCase.Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, Proveedor.Id, "PO-001"));

            var detallePedidoUseCase = new AgregarDetallePedidoProveedorUseCase(Pedidos, Ids);
            pedido = detallePedidoUseCase.Ejecutar(pedido.Id, new AgregarDetallePedidoProveedorRequest(consolidacion.Detalles[0].Id, cantidadPedida));

            pedido = new EnviarPedidoProveedorUseCase(Pedidos).Ejecutar(pedido.Id);

            return (pedido, pedido.Detalles[0].Id);
        }

        public EntregaResponse CrearEntrega(int pedidoId, string numeroRemision) =>
            new CrearEntregaUseCase(Entregas, Pedidos, Auditoria, Ids, NullLogger<CrearEntregaUseCase>.Instance)
                .Ejecutar(Fecha, new CrearEntregaRequest(pedidoId, numeroRemision));

        public PedidoProveedorEstado EstadoActualDelPedido(int pedidoId) => Pedidos.ObtenerPorId(pedidoId)!.Estado;
    }

    [Fact]
    public void Anular_una_entrega_sin_detalles_de_un_pedido_enviado_lo_deja_igual()
    {
        var escenario = new Escenario();
        var (pedido, _) = escenario.CrearPedidoEnviado();
        var entrega = escenario.CrearEntrega(pedido.Id, "REM-001");

        var respuesta = escenario.AnularUseCase().Ejecutar(entrega.Id);

        Assert.Equal(EntregaEstado.Anulada.ToString(), respuesta.Estado);
        Assert.Equal(PedidoProveedorEstado.Enviado, escenario.EstadoActualDelPedido(pedido.Id));
    }

    [Fact]
    public void Anular_la_unica_entrega_que_dejaba_el_pedido_parcialmente_entregado_lo_revierte_a_enviado()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoEnviado(cantidadPedida: 100);
        var entrega = escenario.CrearEntrega(pedido.Id, "REM-001");
        escenario.AgregarDetalleUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(detalleId, 60));
        Assert.Equal(PedidoProveedorEstado.ParcialmenteEntregado, escenario.EstadoActualDelPedido(pedido.Id));

        escenario.AnularUseCase().Ejecutar(entrega.Id);

        Assert.Equal(PedidoProveedorEstado.Enviado, escenario.EstadoActualDelPedido(pedido.Id));
    }

    [Fact]
    public void Anular_la_unica_entrega_que_dejaba_el_pedido_entregado_lo_revierte_a_enviado()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoEnviado(cantidadPedida: 100);
        var entrega = escenario.CrearEntrega(pedido.Id, "REM-001");
        escenario.AgregarDetalleUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(detalleId, 100));
        Assert.Equal(PedidoProveedorEstado.Entregado, escenario.EstadoActualDelPedido(pedido.Id));

        escenario.AnularUseCase().Ejecutar(entrega.Id);

        Assert.Equal(PedidoProveedorEstado.Enviado, escenario.EstadoActualDelPedido(pedido.Id));
    }

    // Múltiples entregas parciales: anular una de ellas debe recalcular con base en las que
    // quedan válidas, no revertir a ENVIADO si todavía queda alguna cantidad entregada.
    [Fact]
    public void Anular_una_de_dos_entregas_parciales_mantiene_parcialmente_entregado_segun_la_restante()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoEnviado(cantidadPedida: 100);
        var entrega1 = escenario.CrearEntrega(pedido.Id, "REM-001");
        escenario.AgregarDetalleUseCase().Ejecutar(entrega1.Id, new AgregarDetalleEntregaRequest(detalleId, 60));
        var entrega2 = escenario.CrearEntrega(pedido.Id, "REM-002");
        escenario.AgregarDetalleUseCase().Ejecutar(entrega2.Id, new AgregarDetalleEntregaRequest(detalleId, 30));
        Assert.Equal(PedidoProveedorEstado.ParcialmenteEntregado, escenario.EstadoActualDelPedido(pedido.Id));

        escenario.AnularUseCase().Ejecutar(entrega2.Id);

        // Solo quedan los 60 válidos de entrega1: sigue habiendo cantidad pendiente (40) y
        // todavía hay alguna cantidad entregada válida -> PARCIALMENTE_ENTREGADO (no ENVIADO).
        Assert.Equal(PedidoProveedorEstado.ParcialmenteEntregado, escenario.EstadoActualDelPedido(pedido.Id));
    }

    // La entrega que completó el pedido se anula, pero queda otra entrega parcial válida.
    [Fact]
    public void Anular_la_entrega_que_completaba_el_pedido_lo_deja_parcialmente_entregado_si_queda_otra_valida()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoEnviado(cantidadPedida: 100);
        var entrega1 = escenario.CrearEntrega(pedido.Id, "REM-001");
        escenario.AgregarDetalleUseCase().Ejecutar(entrega1.Id, new AgregarDetalleEntregaRequest(detalleId, 60));
        var entrega2 = escenario.CrearEntrega(pedido.Id, "REM-002");
        escenario.AgregarDetalleUseCase().Ejecutar(entrega2.Id, new AgregarDetalleEntregaRequest(detalleId, 40));
        Assert.Equal(PedidoProveedorEstado.Entregado, escenario.EstadoActualDelPedido(pedido.Id));

        escenario.AnularUseCase().Ejecutar(entrega2.Id);

        Assert.Equal(PedidoProveedorEstado.ParcialmenteEntregado, escenario.EstadoActualDelPedido(pedido.Id));
    }

    [Fact]
    public void No_permite_anular_una_entrega_de_un_pedido_ya_cerrado()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoEnviado(cantidadPedida: 100);
        var entrega = escenario.CrearEntrega(pedido.Id, "REM-001");
        escenario.AgregarDetalleUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(detalleId, 100));
        escenario.CerrarUseCase().Ejecutar(pedido.Id);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.AnularUseCase().Ejecutar(entrega.Id));

        // La entrega permanece sin cambios.
        Assert.Equal(EntregaEstado.Registrada, escenario.Entregas.ObtenerPorId(entrega.Id)!.Estado);
        Assert.Equal(PedidoProveedorEstado.Cerrado, escenario.EstadoActualDelPedido(pedido.Id));
    }

    // Una entrega ANULADA deja de contar para el tope de "no superar la cantidad pedida":
    // se libera capacidad para nuevas entregas.
    [Fact]
    public void Una_entrega_anulada_ya_no_participa_en_el_calculo_de_cantidad_entregada()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoEnviado(cantidadPedida: 100);
        var entrega1 = escenario.CrearEntrega(pedido.Id, "REM-001");
        escenario.AgregarDetalleUseCase().Ejecutar(entrega1.Id, new AgregarDetalleEntregaRequest(detalleId, 60));
        escenario.AnularUseCase().Ejecutar(entrega1.Id);

        // Sin la anulación, 60 + 100 = 160 > 100 habría fallado. Con la anulación, los 60 ya no
        // cuentan y la entrega nueva puede registrar la cantidad pedida completa.
        var entrega2 = escenario.CrearEntrega(pedido.Id, "REM-002");
        var respuesta = escenario.AgregarDetalleUseCase().Ejecutar(entrega2.Id, new AgregarDetalleEntregaRequest(detalleId, 100));

        Assert.Equal(100, respuesta.Detalles[0].CantidadEntregada);
        Assert.Equal(PedidoProveedorEstado.Entregado, escenario.EstadoActualDelPedido(pedido.Id));
    }

    [Fact]
    public void No_permite_anular_una_entrega_ya_anulada()
    {
        var escenario = new Escenario();
        var (pedido, _) = escenario.CrearPedidoEnviado();
        var entrega = escenario.CrearEntrega(pedido.Id, "REM-001");
        escenario.AnularUseCase().Ejecutar(entrega.Id);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.AnularUseCase().Ejecutar(entrega.Id));
    }

    [Fact]
    public void Anular_entrega_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.AnularUseCase().Ejecutar(999));
    }

    // RN-054 (cierre 2026-09-11): un pedido CANCELADO queda fuera de operación.

    [Fact]
    public void No_permite_crear_una_entrega_para_un_pedido_cancelado()
    {
        var escenario = new Escenario();
        var (pedido, _) = escenario.CrearPedidoEnviado();
        escenario.CancelarUseCase().Ejecutar(pedido.Id);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.CrearEntrega(pedido.Id, "REM-001"));
    }

    [Fact]
    public void No_permite_anular_una_entrega_de_un_pedido_cancelado()
    {
        var escenario = new Escenario();
        var (pedido, _) = escenario.CrearPedidoEnviado();
        var entrega = escenario.CrearEntrega(pedido.Id, "REM-001"); // creada mientras Enviado.
        escenario.CancelarUseCase().Ejecutar(pedido.Id);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.AnularUseCase().Ejecutar(entrega.Id));

        // La operación rechazada no modifica el estado existente de la entrega ni del pedido.
        Assert.Equal(EntregaEstado.Registrada, escenario.Entregas.ObtenerPorId(entrega.Id)!.Estado);
        Assert.Equal(PedidoProveedorEstado.Cancelado, escenario.EstadoActualDelPedido(pedido.Id));
    }
}
