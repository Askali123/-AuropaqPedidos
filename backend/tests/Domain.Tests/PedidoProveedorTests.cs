using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class PedidoProveedorTests
{
    private static readonly DateTime Fecha = new(2026, 9, 2);

    private static Periodo CrearPeriodo() => new(
        1, 2026, 9,
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
        "ABIERTO");

    private static Producto CrearProducto(int id, string nombre) =>
        new(id, nombre, new Categoria(id, "Aseo"), new UnidadMedida(id, "UNIDAD", "Unidad"));

    // Mismo helper que ConsolidacionTests: requisición APROBADA con un único detalle.
    private static (Requisicion Requisicion, DetalleRequisicion Detalle) CrearRequisicionAprobada(
        int requisicionId, int empresaId, Producto producto, int cantidad, Periodo periodo)
    {
        var empresa = new Empresa(empresaId, $"Empresa {empresaId}");
        var requisicion = new Requisicion(requisicionId, empresa, periodo, 10, Fecha);
        var detalle = requisicion.AgregarDetalle(requisicionId * 100 + 1, producto, cantidad);
        var sede = new Sede(requisicionId, empresa, $"Sede {requisicionId}");
        requisicion.AgregarDistribucion(requisicionId * 100 + 2, detalle, sede, cantidad);

        requisicion.Enviar(1, Fecha);
        requisicion.IniciarRevision(1, Fecha);
        requisicion.Aprobar(1, Fecha);

        return (requisicion, detalle);
    }

    // Consolidación con un único DetalleConsolidacion (85 unidades) originado en una requisición aprobada.
    private static (Consolidacion Consolidacion, DetalleConsolidacion Detalle, Requisicion RequisicionOrigen, DetalleRequisicion DetalleOrigen) CrearConsolidacionConUnDetalle(
        Producto producto, int cantidad)
    {
        var periodo = CrearPeriodo();
        var (requisicion, detalleReq) = CrearRequisicionAprobada(1, empresaId: 1, producto, cantidad, periodo);

        var consolidacion = new Consolidacion(1, periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
        consolidacion.AgregarAsignacion(1, 1, requisicion, detalleReq, cantidad);

        return (consolidacion, consolidacion.Detalles[0], requisicion, detalleReq);
    }

    private static Proveedor CrearProveedor() => new(1, "Proveedor Uno");

    private static PedidoProveedor CrearPedido(Consolidacion consolidacion, Proveedor? proveedor = null) =>
        new(1, consolidacion, proveedor ?? CrearProveedor(), "PO-001", usuarioCreacionId: 10, Fecha);

    [Fact]
    public void Crea_un_pedido_valido_con_proveedor_y_numero()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var proveedor = CrearProveedor();

        var pedido = CrearPedido(consolidacion, proveedor);

        Assert.Equal(proveedor, pedido.Proveedor);
        Assert.Equal(consolidacion, pedido.Consolidacion);
        Assert.Equal("PO-001", pedido.NumeroPedido);
        Assert.Empty(pedido.Detalles);
    }

    [Fact]
    public void No_permite_crear_un_pedido_sin_proveedor()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);

        Assert.Throws<ReglaDeNegocioException>(() =>
            new PedidoProveedor(1, consolidacion, proveedor: null!, "PO-001", usuarioCreacionId: 10, Fecha));
    }

    [Fact]
    public void No_permite_crear_un_pedido_sin_numero_de_pedido()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);

        Assert.Throws<ReglaDeNegocioException>(() =>
            new PedidoProveedor(1, consolidacion, CrearProveedor(), numeroPedido: "", usuarioCreacionId: 10, Fecha));
    }

    [Fact]
    public void Agregar_detalle_captura_cantidad_necesaria_y_permite_cantidad_pedida_diferente()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(producto, 85);
        var pedido = CrearPedido(consolidacion);

        var detallePedido = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100);

        Assert.Equal(85, detallePedido.CantidadNecesaria);
        Assert.Equal(100, detallePedido.CantidadPedida);
        Assert.Equal(producto, detallePedido.Producto);
    }

    [Fact]
    public void Permite_cantidad_pedida_menor_que_la_cantidad_necesaria()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(producto, 85);
        var pedido = CrearPedido(consolidacion);

        var detallePedido = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 50);

        Assert.Equal(85, detallePedido.CantidadNecesaria);
        Assert.Equal(50, detallePedido.CantidadPedida);
    }

    [Fact]
    public void No_permite_agregar_un_detalle_de_consolidacion_que_no_pertenece_a_este_pedido()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacionA, detalleA, _, _) = CrearConsolidacionConUnDetalle(producto, 85);
        var (consolidacionB, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(2, "Jabón"), 20);

        var pedido = CrearPedido(consolidacionB);

        Assert.Throws<ReglaDeNegocioException>(() => pedido.AgregarDetalle(1, detalleA, 85));
    }

    [Fact]
    public void No_permite_cantidad_pedida_menor_o_igual_a_cero()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(producto, 85);
        var pedido = CrearPedido(consolidacion);

        Assert.Throws<ReglaDeNegocioException>(() => pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 0));
    }

    // TASK-105 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, hallazgo D1/F4).
    [Fact]
    public void Agregar_detalle_captura_el_codigo_del_proveedor_cuando_existe_la_relacion()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(producto, 85);
        var proveedor = CrearProveedor();
        var pedido = CrearPedido(consolidacion, proveedor);
        var productoProveedor = new ProductoProveedor(1, producto, proveedor, "COD-PROV-001");

        var detallePedido = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100, productoProveedor: productoProveedor);

        Assert.Equal("COD-PROV-001", detallePedido.CodigoProveedorUtilizado);
    }

    [Fact]
    public void Agregar_detalle_sin_relacion_producto_proveedor_deja_el_codigo_en_null()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(producto, 85);
        var pedido = CrearPedido(consolidacion);

        var detallePedido = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100);

        Assert.Null(detallePedido.CodigoProveedorUtilizado);
    }

    [Fact]
    public void No_permite_usar_una_relacion_producto_proveedor_de_otro_proveedor()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(producto, 85);
        var pedido = CrearPedido(consolidacion, CrearProveedor());
        var otroProveedor = new Proveedor(2, "Proveedor Dos");
        var productoProveedorDeOtroProveedor = new ProductoProveedor(1, producto, otroProveedor, "COD-OTRO");

        Assert.Throws<ReglaDeNegocioException>(() =>
            pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100, productoProveedor: productoProveedorDeOtroProveedor));
    }

    [Fact]
    public void Distribuye_el_detalle_del_pedido_entre_sedes_de_distintas_empresas()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(producto, 85);
        var pedido = CrearPedido(consolidacion);
        var detallePedido = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100);

        var empresaA = new Empresa(10, "Empresa A");
        var empresaB = new Empresa(20, "Empresa B");
        var sedeA = new Sede(1, empresaA, "Sede A");
        var sedeB = new Sede(2, empresaB, "Sede B");

        pedido.AgregarDistribucion(1, detallePedido, sedeA, 60);
        pedido.AgregarDistribucion(2, detallePedido, sedeB, 40);

        Assert.Equal(2, detallePedido.Distribuciones.Count);
        Assert.Equal(100, detallePedido.CantidadDistribuida);
    }

    [Fact]
    public void La_trazabilidad_hacia_la_necesidad_consolidada_y_la_requisicion_original_es_recuperable()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacion, detalleConsolidado, requisicionOrigen, detalleOrigen) = CrearConsolidacionConUnDetalle(producto, 85);
        var pedido = CrearPedido(consolidacion);
        var detallePedido = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 85);

        // 04-base-datos.md §27 no incluye DetalleConsolidacionId: la trazabilidad se resuelve
        // por (Producto + Consolidacion del pedido), válido porque Consolidacion nunca mezcla
        // dos DetalleConsolidacion del mismo producto.
        var detalleConsolidacionEncontrado = pedido.Consolidacion.Detalles.Single(d => d.Producto == detallePedido.Producto);
        Assert.Same(detalleConsolidado, detalleConsolidacionEncontrado);

        var asignacion = Assert.Single(detalleConsolidacionEncontrado.Asignaciones);
        Assert.Same(detalleOrigen, asignacion.DetalleRequisicionOrigen);
        Assert.Equal(requisicionOrigen.Id, requisicionOrigen.Id);
    }

    [Fact]
    public void Crear_el_pedido_y_sus_detalles_no_modifica_la_consolidacion_ni_la_requisicion_original()
    {
        var producto = CrearProducto(1, "Papel higiénico");
        var (consolidacion, detalleConsolidado, requisicionOrigen, detalleOrigen) = CrearConsolidacionConUnDetalle(producto, 85);
        var pedido = CrearPedido(consolidacion);

        pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100);

        Assert.Equal(85, detalleConsolidado.CantidadNecesaria);
        Assert.Single(consolidacion.Detalles);
        Assert.Equal(85, detalleOrigen.CantidadSolicitada);
        Assert.Equal(AuropaqPedidos.Domain.Enums.RequisicionEstado.Aprobada, requisicionOrigen.Estado);
    }

    // D-03/RN-043 (cierre documental 2026-09-11): ciclo de estados de PedidoProveedor.

    [Fact]
    public void Un_pedido_se_crea_siempre_en_borrador()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);

        var pedido = CrearPedido(consolidacion);

        Assert.Equal(PedidoProveedorEstado.Borrador, pedido.Estado);
    }

    [Fact]
    public void Enviar_pasa_de_borrador_a_enviado()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);

        pedido.Enviar();

        Assert.Equal(PedidoProveedorEstado.Enviado, pedido.Estado);
    }

    [Fact]
    public void No_permite_enviar_un_pedido_que_no_esta_en_borrador()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        pedido.Enviar();

        Assert.Throws<ReglaDeNegocioException>(() => pedido.Enviar());
    }

    // RN-065/D-18 (2026-09-17, incremento "Fase 6-9 en Frontend"): la distribución de cada
    // detalle debe estar completa antes de enviar — mismo criterio que Requisicion.Enviar()
    // (RN-011).
    [Fact]
    public void No_permite_enviar_un_pedido_con_distribucion_incompleta()
    {
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        var detalle = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100);
        pedido.AgregarDistribucion(1, detalle, new Sede(1, new Empresa(999, "Empresa"), "Sede"), 60); // solo 60 de 100

        Assert.Throws<ReglaDeNegocioException>(() => pedido.Enviar());
    }

    [Fact]
    public void Permite_enviar_un_pedido_con_distribucion_completa()
    {
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        var detalle = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100);
        pedido.AgregarDistribucion(1, detalle, new Sede(1, new Empresa(999, "Empresa"), "Sede"), 100);

        pedido.Enviar();

        Assert.Equal(PedidoProveedorEstado.Enviado, pedido.Estado);
    }

    [Fact]
    public void Cancelar_es_valido_desde_borrador()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);

        pedido.Cancelar();

        Assert.Equal(PedidoProveedorEstado.Cancelado, pedido.Estado);
    }

    [Fact]
    public void Cancelar_es_valido_desde_enviado()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        pedido.Enviar();

        pedido.Cancelar();

        Assert.Equal(PedidoProveedorEstado.Cancelado, pedido.Estado);
    }

    [Fact]
    public void No_permite_cancelar_un_pedido_ya_cancelado()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        pedido.Cancelar();

        Assert.Throws<ReglaDeNegocioException>(() => pedido.Cancelar());
    }

    [Fact]
    public void No_permite_cerrar_un_pedido_que_no_esta_entregado()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        pedido.Enviar();

        Assert.Throws<ReglaDeNegocioException>(() => pedido.Cerrar());
    }

    [Fact]
    public void Cerrar_pasa_de_entregado_a_cerrado()
    {
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        var detalle = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100);
        // RN-065/D-18 (2026-09-17): distribución completa exigida antes de enviar.
        pedido.AgregarDistribucion(1, detalle, new Sede(1, new Empresa(999, "Empresa"), "Sede"), 100);
        pedido.Enviar();
        pedido.ActualizarEstadoPorEntregas(hayAlgunaCantidadEntregada: true, quedaCantidadPendiente: false);

        pedido.Cerrar();

        Assert.Equal(PedidoProveedorEstado.Cerrado, pedido.Estado);
    }

    [Fact]
    public void No_permite_cerrar_un_pedido_ya_cerrado()
    {
        var (consolidacion, detalleConsolidado, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        var detalle = pedido.AgregarDetalle(1, detalleConsolidado, cantidadPedida: 100);
        // RN-065/D-18 (2026-09-17): distribución completa exigida antes de enviar.
        pedido.AgregarDistribucion(1, detalle, new Sede(1, new Empresa(999, "Empresa"), "Sede"), 100);
        pedido.Enviar();
        pedido.ActualizarEstadoPorEntregas(hayAlgunaCantidadEntregada: true, quedaCantidadPendiente: false);
        pedido.Cerrar();

        Assert.Throws<ReglaDeNegocioException>(() => pedido.Cerrar());
    }

    [Fact]
    public void ActualizarEstadoPorEntregas_deja_parcialmente_entregado_si_queda_cantidad_pendiente()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        pedido.Enviar();

        pedido.ActualizarEstadoPorEntregas(hayAlgunaCantidadEntregada: true, quedaCantidadPendiente: true);

        Assert.Equal(PedidoProveedorEstado.ParcialmenteEntregado, pedido.Estado);
    }

    // B1 (cierre técnico 2026-09-11): si ya no queda ninguna cantidad entregada válida (p. ej.
    // se anularon todas las entregas), el pedido vuelve a ENVIADO — RN-043 define
    // PARCIALMENTE_ENTREGADO como "existe al menos una entrega".
    [Fact]
    public void ActualizarEstadoPorEntregas_vuelve_a_enviado_si_no_queda_ninguna_cantidad_entregada()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);
        pedido.Enviar();
        pedido.ActualizarEstadoPorEntregas(hayAlgunaCantidadEntregada: true, quedaCantidadPendiente: true);

        pedido.ActualizarEstadoPorEntregas(hayAlgunaCantidadEntregada: false, quedaCantidadPendiente: true);

        Assert.Equal(PedidoProveedorEstado.Enviado, pedido.Estado);
    }

    [Fact]
    public void ActualizarEstadoPorEntregas_no_permite_actuar_sobre_un_pedido_en_borrador()
    {
        var (consolidacion, _, _, _) = CrearConsolidacionConUnDetalle(CrearProducto(1, "Papel higiénico"), 85);
        var pedido = CrearPedido(consolidacion);

        Assert.Throws<ReglaDeNegocioException>(() =>
            pedido.ActualizarEstadoPorEntregas(hayAlgunaCantidadEntregada: true, quedaCantidadPendiente: true));
    }
}
