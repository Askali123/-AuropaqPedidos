using Application.Tests.Fakes;
using AuropaqPedidos.Application.Entregas;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class CrearEntregaUseCaseTests
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

        public CrearEntregaUseCase CrearEntregaUseCase() => new(Entregas, Pedidos, Ids);
        public AgregarDetalleEntregaUseCase AgregarDetalleUseCase() => new(Entregas, Pedidos, Ids, Transaccion);
        public AgregarDistribucionEntregaUseCase AgregarDistribucionUseCase() => new(Entregas, Sedes, Ids);
        public CalcularCantidadPendienteUseCase CalcularPendienteUseCase() => new(Pedidos, Entregas);

        public Producto CrearProducto(string nombre) =>
            new(Ids.Siguiente(), nombre, new Categoria(Ids.Siguiente(), "Aseo"), new UnidadMedida(Ids.Siguiente(), "UNIDAD", "Unidad"));

        // Requisición APROBADA -> Consolidación -> PedidoProveedor con un DetallePedidoProveedor
        // de "cantidadPedida" unidades.
        public (PedidoProveedorResponse Pedido, int DetallePedidoId) CrearPedidoConUnDetalle(
            Producto producto, int cantidadNecesaria, int cantidadPedida)
        {
            var empresa = new Empresa(Ids.Siguiente(), "Empresa 1");
            var requisicion = new Requisicion(Ids.Siguiente(), empresa, Periodo, 10, Fecha);
            var detalleReq = requisicion.AgregarDetalle(Ids.Siguiente(), producto, cantidadNecesaria);
            var sede = new Sede(Ids.Siguiente(), empresa, "Sede origen");
            requisicion.AgregarDistribucion(Ids.Siguiente(), detalleReq, sede, cantidadNecesaria);
            requisicion.Enviar(1, Fecha);
            requisicion.IniciarRevision(1, Fecha);
            requisicion.Aprobar(1, Fecha);
            Requisiciones.Guardar(requisicion);

            var consolidacion = new Consolidacion(Ids.Siguiente(), Periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
            consolidacion.AgregarAsignacion(Ids.Siguiente(), Ids.Siguiente(), requisicion, detalleReq, cantidadNecesaria);
            Consolidaciones.Guardar(consolidacion);

            var pedidoUseCase = new CrearPedidoProveedorUseCase(Pedidos, Consolidaciones, Proveedores, Ids);
            var pedido = pedidoUseCase.Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, Proveedor.Id, "PO-001"));

            var detallePedidoUseCase = new AgregarDetallePedidoProveedorUseCase(Pedidos, Ids);
            var conDetalle = detallePedidoUseCase.Ejecutar(pedido.Id, new AgregarDetallePedidoProveedorRequest(consolidacion.Detalles[0].Id, cantidadPedida));

            // D-04/RN-046: una entrega solo puede registrarse contra un pedido ENVIADO o
            // PARCIALMENTE_ENTREGADO.
            var enviarUseCase = new EnviarPedidoProveedorUseCase(Pedidos);
            enviarUseCase.Ejecutar(pedido.Id);

            return (conDetalle, conDetalle.Detalles[0].Id);
        }
    }

    [Fact]
    public void Crea_una_entrega_valida_para_un_pedido_existente()
    {
        var escenario = new Escenario();
        var (pedido, _) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);

        var respuesta = escenario.CrearEntregaUseCase().Ejecutar(
            Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));

        Assert.Equal(pedido.Id, respuesta.PedidoProveedorId);
        Assert.Equal("REM-001", respuesta.NumeroRemision);
        Assert.Empty(respuesta.Detalles);
    }

    [Fact]
    public void Pedido_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() =>
            escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(999, "REM-001")));
    }

    [Fact]
    public void Agregar_detalle_permite_una_entrega_parcial_de_la_cantidad_pedida()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);
        var entrega = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));

        var respuesta = escenario.AgregarDetalleUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(detalleId, 60));

        var detalle = Assert.Single(respuesta.Detalles);
        Assert.Equal(60, detalle.CantidadEntregada);
    }

    [Fact]
    public void No_permite_que_dos_entregas_juntas_superen_la_cantidad_pedida()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);

        var entrega1 = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));
        escenario.AgregarDetalleUseCase().Ejecutar(entrega1.Id, new AgregarDetalleEntregaRequest(detalleId, 60));

        var entrega2 = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-002"));

        // 60 (entrega1) + 50 (entrega2) = 110 > 100 pedidos.
        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.AgregarDetalleUseCase().Ejecutar(entrega2.Id, new AgregarDetalleEntregaRequest(detalleId, 50)));
    }

    [Fact]
    public void Permite_completar_exactamente_la_cantidad_pedida_entre_dos_entregas()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);

        var entrega1 = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));
        escenario.AgregarDetalleUseCase().Ejecutar(entrega1.Id, new AgregarDetalleEntregaRequest(detalleId, 60));

        var entrega2 = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-002"));
        var respuesta = escenario.AgregarDetalleUseCase().Ejecutar(entrega2.Id, new AgregarDetalleEntregaRequest(detalleId, 40));

        Assert.Equal(40, respuesta.Detalles[0].CantidadEntregada);
    }

    [Fact]
    public void Agregar_distribucion_conserva_el_snapshot_historico_de_la_sede()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);
        var entrega = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));
        var conDetalle = escenario.AgregarDetalleUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(detalleId, 60));
        var detalleEntregaId = conDetalle.Detalles[0].Id;

        var empresaDestino = new Empresa(escenario.Ids.Siguiente(), "Empresa Destino");
        var sede = new Sede(escenario.Ids.Siguiente(), empresaDestino, "Sede Bogotá", direccion: "Carrera 10 # 20-30", contacto: "Recepción");
        escenario.Sedes.Agregar(sede);

        var respuesta = escenario.AgregarDistribucionUseCase().Ejecutar(entrega.Id, detalleEntregaId, sede.Id, 60);

        var distribucion = Assert.Single(respuesta.Detalles[0].Distribuciones);
        Assert.Equal("Carrera 10 # 20-30", distribucion.DireccionEntrega);
        Assert.Equal("Recepción", distribucion.ContactoEntrega);
    }

    [Fact]
    public void Sede_inactiva_no_puede_recibir_distribucion_de_entrega()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);
        var entrega = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));
        var conDetalle = escenario.AgregarDetalleUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(detalleId, 60));
        var detalleEntregaId = conDetalle.Detalles[0].Id;

        var empresa = new Empresa(escenario.Ids.Siguiente(), "Empresa 1");
        var sede = new Sede(escenario.Ids.Siguiente(), empresa, "Sede inactiva");
        sede.Desactivar();
        escenario.Sedes.Agregar(sede);

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.AgregarDistribucionUseCase().Ejecutar(entrega.Id, detalleEntregaId, sede.Id, 10));
    }

    [Fact]
    public void Calcula_la_cantidad_pendiente_sumando_todas_las_entregas_del_pedido()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);

        var entrega1 = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));
        escenario.AgregarDetalleUseCase().Ejecutar(entrega1.Id, new AgregarDetalleEntregaRequest(detalleId, 60));

        var pendiente = escenario.CalcularPendienteUseCase().Ejecutar(pedido.Id, detalleId);

        Assert.Equal(100, pendiente.CantidadPedida);
        Assert.Equal(60, pendiente.CantidadEntregadaAcumulada);
        Assert.Equal(40, pendiente.CantidadPendiente);
    }

    [Fact]
    public void Registrar_una_entrega_no_modifica_el_pedido_ni_la_consolidacion_original()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);
        var entrega = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));

        escenario.AgregarDetalleUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(detalleId, 60));

        var pedidoRecuperado = escenario.Pedidos.ObtenerPorId(pedido.Id)!;
        Assert.Equal(100, pedidoRecuperado.Detalles[0].CantidadPedida);
        Assert.Equal(85, pedidoRecuperado.Consolidacion.Detalles[0].CantidadNecesaria);
    }

    // Sección 9 del cierre documental 2026-09-11 (D-03/RN-043): actualización automática del
    // estado del PedidoProveedor al registrar entregas.

    [Fact]
    public void Registrar_una_entrega_parcial_deja_el_pedido_parcialmente_entregado()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);
        var entrega = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));

        escenario.AgregarDetalleUseCase().Ejecutar(entrega.Id, new AgregarDetalleEntregaRequest(detalleId, 60));

        var pedidoRecuperado = escenario.Pedidos.ObtenerPorId(pedido.Id)!;
        Assert.Equal(AuropaqPedidos.Domain.Enums.PedidoProveedorEstado.ParcialmenteEntregado, pedidoRecuperado.Estado);
    }

    [Fact]
    public void Completar_la_cantidad_pedida_deja_el_pedido_entregado()
    {
        var escenario = new Escenario();
        var (pedido, detalleId) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);
        var entrega1 = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));
        escenario.AgregarDetalleUseCase().Ejecutar(entrega1.Id, new AgregarDetalleEntregaRequest(detalleId, 60));

        var entrega2 = escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-002"));
        escenario.AgregarDetalleUseCase().Ejecutar(entrega2.Id, new AgregarDetalleEntregaRequest(detalleId, 40));

        var pedidoRecuperado = escenario.Pedidos.ObtenerPorId(pedido.Id)!;
        Assert.Equal(AuropaqPedidos.Domain.Enums.PedidoProveedorEstado.Entregado, pedidoRecuperado.Estado);
    }

    [Fact]
    public void No_permite_registrar_una_entrega_para_un_pedido_todavia_en_borrador()
    {
        var escenario = new Escenario();
        var empresa = new Empresa(escenario.Ids.Siguiente(), "Empresa 1");
        var requisicion = new Requisicion(escenario.Ids.Siguiente(), empresa, escenario.Periodo, 10, Fecha);
        var producto = escenario.CrearProducto("Papel higiénico");
        var detalleReq = requisicion.AgregarDetalle(escenario.Ids.Siguiente(), producto, 85);
        var sede = new Sede(escenario.Ids.Siguiente(), empresa, "Sede origen");
        requisicion.AgregarDistribucion(escenario.Ids.Siguiente(), detalleReq, sede, 85);
        requisicion.Enviar(1, Fecha);
        requisicion.IniciarRevision(1, Fecha);
        requisicion.Aprobar(1, Fecha);
        escenario.Requisiciones.Guardar(requisicion);

        var consolidacion = new Consolidacion(escenario.Ids.Siguiente(), escenario.Periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
        consolidacion.AgregarAsignacion(escenario.Ids.Siguiente(), escenario.Ids.Siguiente(), requisicion, detalleReq, 85);
        escenario.Consolidaciones.Guardar(consolidacion);

        // Pedido creado pero NUNCA enviado (sigue en BORRADOR).
        var pedidoUseCase = new CrearPedidoProveedorUseCase(escenario.Pedidos, escenario.Consolidaciones, escenario.Proveedores, escenario.Ids);
        var pedido = pedidoUseCase.Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-999"));

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001")));
    }

    [Fact]
    public void No_permite_dos_entregas_con_el_mismo_numero_de_remision_para_el_mismo_pedido()
    {
        var escenario = new Escenario();
        var (pedido, _) = escenario.CrearPedidoConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85, 100);
        escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001"));

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.CrearEntregaUseCase().Ejecutar(Fecha, new CrearEntregaRequest(pedido.Id, "REM-001")));
    }
}
