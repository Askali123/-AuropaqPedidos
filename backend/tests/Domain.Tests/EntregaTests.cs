using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class EntregaTests
{
    private static readonly DateTime Fecha = new(2026, 9, 2);

    private static Periodo CrearPeriodo() => new(
        1, 2026, 9,
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
        "ABIERTO");

    private static Producto CrearProducto(int id, string nombre) =>
        new(id, nombre, new Categoria(id, "Aseo"), new UnidadMedida(id, "UNIDAD", "Unidad"));

    // Requisición APROBADA -> Consolidación -> PedidoProveedor con un DetallePedidoProveedor
    // de 100 unidades pedidas.
    private static (PedidoProveedor Pedido, DetallePedidoProveedor DetallePedido) CrearPedidoConUnDetalle(
        Producto producto, int cantidadNecesaria, int cantidadPedida)
    {
        var periodo = CrearPeriodo();
        var empresa = new Empresa(1, "Empresa 1");
        var requisicion = new Requisicion(1, empresa, periodo, 10, Fecha);
        var detalleReq = requisicion.AgregarDetalle(1, producto, cantidadNecesaria);
        var sedeOrigen = new Sede(1, empresa, "Sede origen");
        requisicion.AgregarDistribucion(2, detalleReq, sedeOrigen, cantidadNecesaria);
        requisicion.Enviar(1, Fecha);
        requisicion.IniciarRevision(1, Fecha);
        requisicion.Aprobar(1, Fecha);

        var consolidacion = new Consolidacion(1, periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
        consolidacion.AgregarAsignacion(1, 1, requisicion, detalleReq, cantidadNecesaria);

        var proveedor = new Proveedor(1, "Proveedor Uno");
        var pedido = new PedidoProveedor(1, consolidacion, proveedor, "PO-001", usuarioCreacionId: 10, Fecha);
        var detallePedido = pedido.AgregarDetalle(1, consolidacion.Detalles[0], cantidadPedida);
        // RN-065/D-18 (2026-09-17): distribución completa exigida antes de enviar.
        pedido.AgregarDistribucion(3, detallePedido, sedeOrigen, cantidadPedida);
        // D-04/RN-046: una entrega solo puede registrarse contra un pedido ENVIADO o
        // PARCIALMENTE_ENTREGADO.
        pedido.Enviar();

        return (pedido, detallePedido);
    }

    private static Entrega CrearEntrega(PedidoProveedor pedido) =>
        new(1, pedido, usuarioCreacionId: 10, Fecha, "REM-001");

    [Fact]
    public void Crea_una_entrega_valida_para_un_pedido()
    {
        var (pedido, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);

        var entrega = CrearEntrega(pedido);

        Assert.Equal(pedido, entrega.PedidoProveedor);
        Assert.Equal("REM-001", entrega.NumeroRemision);
        Assert.Empty(entrega.Detalles);
    }

    [Fact]
    public void No_permite_crear_una_entrega_sin_numero_de_remision()
    {
        var (pedido, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);

        Assert.Throws<ReglaDeNegocioException>(() =>
            new Entrega(1, pedido, usuarioCreacionId: 10, Fecha, numeroRemision: ""));
    }

    [Fact]
    public void Agregar_detalle_permite_una_entrega_parcial()
    {
        var (pedido, detallePedido) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido);

        var detalleEntrega = entrega.AgregarDetalle(1, detallePedido, cantidadEntregada: 60, cantidadYaEntregadaEnOtrasEntregas: 0);

        Assert.Equal(60, detalleEntrega.CantidadEntregada);
        Assert.Equal(detallePedido.Producto, detalleEntrega.Producto);
    }

    [Fact]
    public void No_permite_que_la_cantidad_entregada_acumulada_supere_la_cantidad_pedida()
    {
        var (pedido, detallePedido) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido);

        // Ya se entregaron 60 en otra entrega; esta entrega intenta agregar 50 más (110 > 100).
        Assert.Throws<ReglaDeNegocioException>(() =>
            entrega.AgregarDetalle(1, detallePedido, cantidadEntregada: 50, cantidadYaEntregadaEnOtrasEntregas: 60));
    }

    [Fact]
    public void Permite_completar_exactamente_la_cantidad_pedida_entre_varias_entregas()
    {
        var (pedido, detallePedido) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega2 = CrearEntrega(pedido);

        // Primera entrega ya registró 60; esta segunda entrega completa con 40 (60+40=100).
        var detalle = entrega2.AgregarDetalle(2, detallePedido, cantidadEntregada: 40, cantidadYaEntregadaEnOtrasEntregas: 60);

        Assert.Equal(40, detalle.CantidadEntregada);
    }

    [Fact]
    public void No_permite_agregar_un_detalle_de_pedido_que_no_pertenece_a_esta_entrega()
    {
        var (pedidoA, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var (pedidoB, detalleB) = CrearPedidoConUnDetalle(CrearProducto(2, "Jabón"), 20, 20);

        var entregaDePedidoA = CrearEntrega(pedidoA);

        Assert.Throws<ReglaDeNegocioException>(() =>
            entregaDePedidoA.AgregarDetalle(1, detalleB, cantidadEntregada: 10, cantidadYaEntregadaEnOtrasEntregas: 0));
    }

    [Fact]
    public void Distribuir_una_entrega_conserva_fotografia_historica_de_la_sede()
    {
        var (pedido, detallePedido) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido);
        var detalleEntrega = entrega.AgregarDetalle(1, detallePedido, 60, 0);

        var empresa = new Empresa(5, "Empresa Destino");
        var sede = new Sede(5, empresa, "Sede Bogotá", direccion: "Carrera 10 # 20-30", ciudad: "Bogotá", contacto: "Recepción");

        var distribucion = entrega.AgregarDistribucion(1, detalleEntrega, sede, 60);

        // Snapshot capturado en el momento de crear la distribución (RN-035/ADR-019).
        Assert.Equal("Carrera 10 # 20-30", distribucion.DireccionEntrega);
        Assert.Equal("Bogotá", distribucion.CiudadEntrega);
        Assert.Equal("Recepción", distribucion.ContactoEntrega);

        // Sede no expone hoy ningún método para modificar dirección/ciudad/contacto después de
        // construida (solo Activar/Desactivar) — la inmutabilidad del snapshot es estructural:
        // DistribucionEntrega no tiene setters para estos campos, así que no pueden cambiar.
    }

    [Fact]
    public void No_permite_agregar_una_distribucion_de_un_detalle_que_no_pertenece_a_esta_entrega()
    {
        var (pedido, detallePedido) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entregaA = CrearEntrega(pedido);
        var detalleDeEntregaA = entregaA.AgregarDetalle(1, detallePedido, 60, 0);

        var entregaB = CrearEntrega(pedido);
        var empresa = new Empresa(5, "Empresa Destino");
        var sede = new Sede(5, empresa, "Sede Bogotá");

        Assert.Throws<ReglaDeNegocioException>(() => entregaB.AgregarDistribucion(1, detalleDeEntregaA, sede, 10));
    }

    [Fact]
    public void Registrar_la_entrega_no_modifica_el_pedido_ni_la_consolidacion_original()
    {
        var (pedido, detallePedido) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido);

        entrega.AgregarDetalle(1, detallePedido, 60, 0);

        Assert.Equal(100, detallePedido.CantidadPedida);
        Assert.Single(pedido.Detalles);
        Assert.Equal(85, pedido.Consolidacion.Detalles[0].CantidadNecesaria);
    }

    // D-04/RN-046 (cierre documental 2026-09-11): estados de Entrega y su relación con el
    // estado del pedido.

    [Fact]
    public void No_permite_registrar_una_entrega_para_un_pedido_en_borrador()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var periodo = CrearPeriodo();
        var empresa = new Empresa(1, "Empresa 1");
        var requisicion = new Requisicion(1, empresa, periodo, 10, Fecha);
        var detalleReq = requisicion.AgregarDetalle(1, producto, 85);
        var sedeOrigen = new Sede(1, empresa, "Sede origen");
        requisicion.AgregarDistribucion(2, detalleReq, sedeOrigen, 85);
        requisicion.Enviar(1, Fecha);
        requisicion.IniciarRevision(1, Fecha);
        requisicion.Aprobar(1, Fecha);

        var consolidacion = new Consolidacion(1, periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
        consolidacion.AgregarAsignacion(1, 1, requisicion, detalleReq, 85);

        var proveedor = new Proveedor(1, "Proveedor Uno");
        // A diferencia de CrearPedidoConUnDetalle, este pedido se deja en BORRADOR (sin Enviar()).
        var pedidoEnBorrador = new PedidoProveedor(1, consolidacion, proveedor, "PO-001", usuarioCreacionId: 10, Fecha);
        pedidoEnBorrador.AgregarDetalle(1, consolidacion.Detalles[0], 100);

        Assert.Throws<ReglaDeNegocioException>(() => new Entrega(1, pedidoEnBorrador, usuarioCreacionId: 10, Fecha, "REM-001"));
    }

    [Fact]
    public void Una_entrega_se_crea_siempre_registrada()
    {
        var (pedido, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);

        var entrega = CrearEntrega(pedido);

        Assert.Equal(AuropaqPedidos.Domain.Enums.EntregaEstado.Registrada, entrega.Estado);
    }

    [Fact]
    public void Anular_pasa_de_registrada_a_anulada()
    {
        var (pedido, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido);

        entrega.Anular();

        Assert.Equal(AuropaqPedidos.Domain.Enums.EntregaEstado.Anulada, entrega.Estado);
    }

    [Fact]
    public void No_permite_anular_una_entrega_ya_anulada()
    {
        var (pedido, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido);
        entrega.Anular();

        Assert.Throws<ReglaDeNegocioException>(() => entrega.Anular());
    }

    // RN-054 (cierre 2026-09-11): un pedido CANCELADO queda fuera de operación — no admite
    // entregas nuevas ni modificaciones/anulaciones de las existentes.

    [Fact]
    public void No_permite_crear_una_entrega_para_un_pedido_cancelado()
    {
        var (pedido, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        pedido.Cancelar();

        Assert.Throws<ReglaDeNegocioException>(() => new Entrega(1, pedido, usuarioCreacionId: 10, Fecha, "REM-001"));
    }

    [Fact]
    public void No_permite_agregar_detalle_a_una_entrega_de_un_pedido_cancelado()
    {
        var (pedido, detallePedido) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido); // creada mientras el pedido todavía estaba ENVIADO.
        pedido.Cancelar();

        Assert.Throws<ReglaDeNegocioException>(() =>
            entrega.AgregarDetalle(1, detallePedido, cantidadEntregada: 60, cantidadYaEntregadaEnOtrasEntregas: 0));

        // La operación rechazada no modifica los datos existentes.
        Assert.Empty(entrega.Detalles);
    }

    [Fact]
    public void No_permite_agregar_distribucion_a_una_entrega_de_un_pedido_cancelado()
    {
        var (pedido, detallePedido) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido);
        var detalleEntrega = entrega.AgregarDetalle(1, detallePedido, cantidadEntregada: 60, cantidadYaEntregadaEnOtrasEntregas: 0);
        pedido.Cancelar();

        var empresa = new Empresa(5, "Empresa Destino");
        var sede = new Sede(5, empresa, "Sede Bogotá");

        Assert.Throws<ReglaDeNegocioException>(() => entrega.AgregarDistribucion(1, detalleEntrega, sede, 60));

        // La operación rechazada no modifica los datos existentes.
        Assert.Empty(detalleEntrega.Distribuciones);
    }

    [Fact]
    public void No_permite_anular_una_entrega_de_un_pedido_cancelado()
    {
        var (pedido, _) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido);
        pedido.Cancelar();

        Assert.Throws<ReglaDeNegocioException>(() => entrega.Anular());

        // La operación rechazada no modifica el estado existente de la entrega.
        Assert.Equal(AuropaqPedidos.Domain.Enums.EntregaEstado.Registrada, entrega.Estado);
    }

    [Fact]
    public void Un_pedido_operativo_sigue_permitiendo_las_operaciones_normales_de_entrega()
    {
        // Control: mismo flujo que las pruebas anteriores, pero SIN cancelar el pedido — debe
        // seguir funcionando exactamente igual que antes de RN-054.
        var (pedido, detallePedido) = CrearPedidoConUnDetalle(CrearProducto(1, "Papel higiénico"), 85, 100);
        var entrega = CrearEntrega(pedido);
        var detalleEntrega = entrega.AgregarDetalle(1, detallePedido, cantidadEntregada: 60, cantidadYaEntregadaEnOtrasEntregas: 0);

        var empresa = new Empresa(5, "Empresa Destino");
        var sede = new Sede(5, empresa, "Sede Bogotá");
        var distribucion = entrega.AgregarDistribucion(1, detalleEntrega, sede, 60);

        Assert.Equal(60, distribucion.Cantidad);

        entrega.Anular();
        Assert.Equal(AuropaqPedidos.Domain.Enums.EntregaEstado.Anulada, entrega.Estado);
    }
}
