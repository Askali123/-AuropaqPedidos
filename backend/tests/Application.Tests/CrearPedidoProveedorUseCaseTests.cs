using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests;

public class CrearPedidoProveedorUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 2);

    private sealed class Escenario
    {
        public FakeRequisicionRepository Requisiciones { get; } = new();
        public FakeConsolidacionRepository Consolidaciones { get; } = new();
        public FakePedidoProveedorRepository Pedidos { get; } = new();
        public FakePeriodoRepository Periodos { get; } = new();
        public FakeProveedorRepository Proveedores { get; } = new();
        public FakeSedeRepository Sedes { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();
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

        public CrearPedidoProveedorUseCase CrearUseCase() =>
            new(Pedidos, Consolidaciones, Proveedores, Auditoria, Ids, NullLogger<CrearPedidoProveedorUseCase>.Instance);
        public AgregarDetallePedidoProveedorUseCase AgregarDetalleUseCase() => new(Pedidos, Ids);
        public AgregarDistribucionPedidoUseCase AgregarDistribucionUseCase() => new(Pedidos, Sedes, Ids);

        public Producto CrearProducto(string nombre) =>
            new(Ids.Siguiente(), nombre, new Categoria(Ids.Siguiente(), "Aseo"), new UnidadMedida(Ids.Siguiente(), "UNIDAD", "Unidad"));

        // Crea una Consolidacion con un único DetalleConsolidacion (cantidad) originado en una
        // requisición APROBADA, y la guarda en el repositorio.
        public (Consolidacion Consolidacion, DetalleConsolidacion Detalle) CrearConsolidacionConUnDetalle(Producto producto, int cantidad)
        {
            var empresa = new Empresa(Ids.Siguiente(), "Empresa 1");
            var requisicion = new Requisicion(Ids.Siguiente(), empresa, Periodo, 10, Fecha);
            var detalleReq = requisicion.AgregarDetalle(Ids.Siguiente(), producto, cantidad);
            var sede = new Sede(Ids.Siguiente(), empresa, "Sede 1");
            requisicion.AgregarDistribucion(Ids.Siguiente(), detalleReq, sede, cantidad);
            requisicion.Enviar(1, Fecha);
            requisicion.IniciarRevision(1, Fecha);
            requisicion.Aprobar(1, Fecha);
            Requisiciones.Guardar(requisicion);

            var consolidacion = new Consolidacion(Ids.Siguiente(), Periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
            consolidacion.AgregarAsignacion(Ids.Siguiente(), Ids.Siguiente(), requisicion, detalleReq, cantidad);
            Consolidaciones.Guardar(consolidacion);

            return (consolidacion, consolidacion.Detalles[0]);
        }
    }

    [Fact]
    public void Crea_un_pedido_para_una_consolidacion_y_proveedor_existentes()
    {
        var escenario = new Escenario();
        var (consolidacion, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);

        var respuesta = escenario.CrearUseCase().Ejecutar(
            Fecha,
            new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-001"));

        Assert.Equal(consolidacion.Id, respuesta.ConsolidacionId);
        Assert.Equal(escenario.Proveedor.Id, respuesta.ProveedorId);
        Assert.Equal("PO-001", respuesta.NumeroPedido);
        Assert.Empty(respuesta.Detalles);
    }

    // TASK-056: "creación de pedido" es uno de los 6 ejemplos documentados en
    // 04-base-datos.md §33. usuarioId es null aquí porque este endpoint todavía no exige JWT.
    [Fact]
    public void Crear_un_pedido_registra_un_evento_de_auditoria()
    {
        var escenario = new Escenario();
        var (consolidacion, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);

        var respuesta = escenario.CrearUseCase().Ejecutar(
            Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-001"));

        var registro = Assert.Single(escenario.Auditoria.Registros);
        Assert.Equal("PedidoProveedor", registro.Entidad);
        Assert.Equal(respuesta.Id, registro.EntidadId);
        Assert.Equal("CREAR", registro.Accion);
        Assert.Null(registro.UsuarioId);
    }

    [Fact]
    public void Consolidacion_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() =>
            escenario.CrearUseCase().Ejecutar(Fecha, new CrearPedidoProveedorRequest(999, escenario.Proveedor.Id, "PO-001")));
    }

    [Fact]
    public void Proveedor_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();
        var (consolidacion, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);

        Assert.Throws<RecursoNoEncontradoException>(() =>
            escenario.CrearUseCase().Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, 999, "PO-001")));
    }

    [Fact]
    public void Proveedor_inactivo_no_puede_recibir_un_pedido()
    {
        var escenario = new Escenario();
        var (consolidacion, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);
        escenario.Proveedor.Desactivar();

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.CrearUseCase().Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-001")));
    }

    [Fact]
    public void Agregar_detalle_conserva_cantidad_necesaria_y_permite_cantidad_pedida_diferente()
    {
        var escenario = new Escenario();
        var (consolidacion, detalleConsolidado) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);
        var pedido = escenario.CrearUseCase().Ejecutar(
            Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-001"));

        var respuesta = escenario.AgregarDetalleUseCase().Ejecutar(
            pedido.Id, new AgregarDetallePedidoProveedorRequest(detalleConsolidado.Id, CantidadPedida: 100, PrecioUnitario: 12.5m));

        var detalle = Assert.Single(respuesta.Detalles);
        Assert.Equal(85, detalle.CantidadNecesaria);
        Assert.Equal(100, detalle.CantidadPedida);
        Assert.Equal(12.5m, detalle.PrecioUnitario);
        Assert.Equal(detalleConsolidado.Id, detalle.DetalleConsolidacionId);
    }

    [Fact]
    public void Agregar_detalle_con_id_de_consolidacion_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();
        var (consolidacion, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);
        var pedido = escenario.CrearUseCase().Ejecutar(
            Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-001"));

        Assert.Throws<RecursoNoEncontradoException>(() =>
            escenario.AgregarDetalleUseCase().Ejecutar(pedido.Id, new AgregarDetallePedidoProveedorRequest(999, 100)));
    }

    [Fact]
    public void Agregar_distribucion_reparte_el_detalle_entre_sedes_de_distintas_empresas()
    {
        var escenario = new Escenario();
        var (consolidacion, detalleConsolidado) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);
        var pedido = escenario.CrearUseCase().Ejecutar(
            Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-001"));
        var conDetalle = escenario.AgregarDetalleUseCase().Ejecutar(
            pedido.Id, new AgregarDetallePedidoProveedorRequest(detalleConsolidado.Id, 100));
        var detalleId = conDetalle.Detalles[0].Id;

        var empresaA = new Empresa(escenario.Ids.Siguiente(), "Empresa A");
        var empresaB = new Empresa(escenario.Ids.Siguiente(), "Empresa B");
        var sedeA = new Sede(escenario.Ids.Siguiente(), empresaA, "Sede A");
        var sedeB = new Sede(escenario.Ids.Siguiente(), empresaB, "Sede B");
        escenario.Sedes.Agregar(sedeA);
        escenario.Sedes.Agregar(sedeB);

        escenario.AgregarDistribucionUseCase().Ejecutar(pedido.Id, detalleId, sedeA.Id, 60);
        var respuesta = escenario.AgregarDistribucionUseCase().Ejecutar(pedido.Id, detalleId, sedeB.Id, 40);

        Assert.Equal(2, respuesta.Detalles[0].Distribuciones.Count);
    }

    [Fact]
    public void Sede_inactiva_no_puede_recibir_distribucion_de_pedido()
    {
        var escenario = new Escenario();
        var (consolidacion, detalleConsolidado) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);
        var pedido = escenario.CrearUseCase().Ejecutar(
            Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-001"));
        var conDetalle = escenario.AgregarDetalleUseCase().Ejecutar(
            pedido.Id, new AgregarDetallePedidoProveedorRequest(detalleConsolidado.Id, 100));
        var detalleId = conDetalle.Detalles[0].Id;

        var empresa = new Empresa(escenario.Ids.Siguiente(), "Empresa 1");
        var sede = new Sede(escenario.Ids.Siguiente(), empresa, "Sede inactiva");
        sede.Desactivar();
        escenario.Sedes.Agregar(sede);

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.AgregarDistribucionUseCase().Ejecutar(pedido.Id, detalleId, sede.Id, 10));
    }

    [Fact]
    public void Crear_pedido_y_agregar_detalle_no_modifican_la_consolidacion_ni_la_requisicion_original()
    {
        var escenario = new Escenario();
        var (consolidacion, detalleConsolidado) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);
        var pedido = escenario.CrearUseCase().Ejecutar(
            Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-001"));

        escenario.AgregarDetalleUseCase().Ejecutar(pedido.Id, new AgregarDetallePedidoProveedorRequest(detalleConsolidado.Id, 100));

        var consolidacionRecuperada = escenario.Consolidaciones.ObtenerPorId(consolidacion.Id)!;
        Assert.Single(consolidacionRecuperada.Detalles);
        Assert.Equal(85, consolidacionRecuperada.Detalles[0].CantidadNecesaria);
    }

    // D-03/D-09 (cierre documental 2026-09-11): estado inicial e identificadores.

    [Fact]
    public void Un_pedido_creado_queda_siempre_en_borrador()
    {
        var escenario = new Escenario();
        var (consolidacion, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);

        var respuesta = escenario.CrearUseCase().Ejecutar(
            Fecha, new CrearPedidoProveedorRequest(consolidacion.Id, escenario.Proveedor.Id, "PO-001"));

        Assert.Equal("Borrador", respuesta.Estado);
    }

    [Fact]
    public void No_permite_dos_pedidos_con_el_mismo_numero_para_el_mismo_proveedor()
    {
        var escenario = new Escenario();
        var (consolidacionA, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);
        var (consolidacionB, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Jabón"), 20);
        escenario.CrearUseCase().Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacionA.Id, escenario.Proveedor.Id, "PO-001"));

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.CrearUseCase().Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacionB.Id, escenario.Proveedor.Id, "PO-001")));
    }

    [Fact]
    public void Permite_el_mismo_numero_de_pedido_para_proveedores_distintos()
    {
        var escenario = new Escenario();
        var otroProveedor = new Proveedor(999, "Otro proveedor");
        escenario.Proveedores.Agregar(otroProveedor);
        var (consolidacionA, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Papel higiénico"), 85);
        var (consolidacionB, _) = escenario.CrearConsolidacionConUnDetalle(escenario.CrearProducto("Jabón"), 20);
        escenario.CrearUseCase().Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacionA.Id, escenario.Proveedor.Id, "PO-001"));

        var respuesta = escenario.CrearUseCase().Ejecutar(Fecha, new CrearPedidoProveedorRequest(consolidacionB.Id, otroProveedor.Id, "PO-001"));

        Assert.Equal("PO-001", respuesta.NumeroPedido);
    }
}
