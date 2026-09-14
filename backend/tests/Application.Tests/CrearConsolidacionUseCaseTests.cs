using Application.Tests.Fakes;
using AuropaqPedidos.Application.Consolidaciones;
using AuropaqPedidos.Application.Consolidaciones.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class CrearConsolidacionUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 2);

    private sealed class Escenario
    {
        public FakeRequisicionRepository Requisiciones { get; } = new();
        public FakeConsolidacionRepository Consolidaciones { get; } = new();
        public FakePeriodoRepository Periodos { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();

        public Periodo Periodo { get; } = new(
            1, 2026, 9,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
            "ABIERTO");

        public Escenario() => Periodos.Agregar(Periodo);

        public CrearConsolidacionUseCase CrearUseCase() =>
            new(Consolidaciones, Requisiciones, Periodos, Ids);

        // Crea y guarda una requisición APROBADA de una empresa, con un único detalle del
        // producto/cantidad indicados, siguiendo el flujo real de Domain.
        public (Requisicion Requisicion, DetalleRequisicion Detalle) AgregarRequisicionAprobada(
            int empresaId, Producto producto, int cantidad)
        {
            var empresa = new Empresa(empresaId, $"Empresa {empresaId}");
            var requisicion = new Requisicion(Ids.Siguiente(), empresa, Periodo, 10, Fecha);
            var detalle = requisicion.AgregarDetalle(Ids.Siguiente(), producto, cantidad);
            var sede = new Sede(Ids.Siguiente(), empresa, $"Sede {empresaId}");
            requisicion.AgregarDistribucion(Ids.Siguiente(), detalle, sede, cantidad);

            requisicion.Enviar(1, Fecha);
            requisicion.IniciarRevision(1, Fecha);
            requisicion.Aprobar(1, Fecha);

            Requisiciones.Guardar(requisicion);
            return (requisicion, detalle);
        }

        // Crea y guarda una requisición que se queda en BORRADOR (nunca se envía/aprueba).
        public (Requisicion Requisicion, DetalleRequisicion Detalle) AgregarRequisicionEnBorrador(
            int empresaId, Producto producto, int cantidad)
        {
            var empresa = new Empresa(empresaId, $"Empresa {empresaId}");
            var requisicion = new Requisicion(Ids.Siguiente(), empresa, Periodo, 10, Fecha);
            var detalle = requisicion.AgregarDetalle(Ids.Siguiente(), producto, cantidad);

            Requisiciones.Guardar(requisicion);
            return (requisicion, detalle);
        }

        public Producto CrearProducto(string nombre) =>
            new(Ids.Siguiente(), nombre, new Categoria(Ids.Siguiente(), "Aseo"), new UnidadMedida(Ids.Siguiente(), "UNIDAD", "Unidad"));
    }

    [Fact]
    public void Consolida_los_detalles_de_requisiciones_aprobadas_del_periodo_agrupando_por_producto()
    {
        var escenario = new Escenario();
        var producto = escenario.CrearProducto("Papel higiénico");
        escenario.AgregarRequisicionAprobada(empresaId: 1, producto, cantidad: 30);
        escenario.AgregarRequisicionAprobada(empresaId: 2, producto, cantidad: 20);

        var respuesta = escenario.CrearUseCase().Ejecutar(
            usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        Assert.Equal("GENERADA", respuesta.Estado);
        Assert.Single(respuesta.Detalles);
        Assert.Equal(50, respuesta.Detalles[0].CantidadNecesaria);
        Assert.Equal(2, respuesta.Detalles[0].Asignaciones.Count);
    }

    [Fact]
    public void No_incluye_detalles_de_requisiciones_que_no_estan_aprobadas()
    {
        var escenario = new Escenario();
        var producto = escenario.CrearProducto("Papel higiénico");
        escenario.AgregarRequisicionAprobada(empresaId: 1, producto, cantidad: 30);
        escenario.AgregarRequisicionEnBorrador(empresaId: 2, producto, cantidad: 999);

        var respuesta = escenario.CrearUseCase().Ejecutar(
            usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        Assert.Single(respuesta.Detalles);
        Assert.Equal(30, respuesta.Detalles[0].CantidadNecesaria);
    }

    [Fact]
    public void Productos_diferentes_generan_detalles_de_consolidacion_separados()
    {
        var escenario = new Escenario();
        var productoA = escenario.CrearProducto("Papel higiénico");
        var productoB = escenario.CrearProducto("Jabón");
        escenario.AgregarRequisicionAprobada(empresaId: 1, productoA, cantidad: 30);
        escenario.AgregarRequisicionAprobada(empresaId: 2, productoB, cantidad: 20);

        var respuesta = escenario.CrearUseCase().Ejecutar(
            usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        Assert.Equal(2, respuesta.Detalles.Count);
        Assert.Contains(respuesta.Detalles, d => d.ProductoId == productoA.Id && d.CantidadNecesaria == 30);
        Assert.Contains(respuesta.Detalles, d => d.ProductoId == productoB.Id && d.CantidadNecesaria == 20);
    }

    [Fact]
    public void Las_requisiciones_originales_permanecen_intactas_despues_de_consolidar()
    {
        var escenario = new Escenario();
        var producto = escenario.CrearProducto("Papel higiénico");
        var (requisicion, detalle) = escenario.AgregarRequisicionAprobada(empresaId: 1, producto, cantidad: 30);

        escenario.CrearUseCase().Ejecutar(usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        var recuperada = escenario.Requisiciones.ObtenerPorId(requisicion.Id)!;
        Assert.Equal(30, recuperada.Detalles[0].CantidadSolicitada);
        Assert.Equal(detalle.Id, recuperada.Detalles[0].Id);
    }

    [Fact]
    public void La_asignacion_conserva_el_id_del_detalle_de_requisicion_de_origen()
    {
        var escenario = new Escenario();
        var producto = escenario.CrearProducto("Papel higiénico");
        var (_, detalle) = escenario.AgregarRequisicionAprobada(empresaId: 1, producto, cantidad: 30);

        var respuesta = escenario.CrearUseCase().Ejecutar(
            usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        var asignacion = Assert.Single(respuesta.Detalles[0].Asignaciones);
        Assert.Equal(detalle.Id, asignacion.DetalleRequisicionId);
        Assert.Equal(30, asignacion.Cantidad);
    }

    [Fact]
    public void Periodo_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() =>
            escenario.CrearUseCase().Ejecutar(usuarioId: 1, Fecha, new CrearConsolidacionRequest(999, "GENERADA")));
    }

    // A2 (cierre de negocio 2026-09-11): un periodo puede tener múltiples Consolidacion, pero
    // una Requisicion aprobada participa en, a lo sumo, una.

    [Fact]
    public void Primera_consolidacion_toma_las_requisiciones_aprobadas_del_periodo()
    {
        var escenario = new Escenario();
        var producto = escenario.CrearProducto("Papel higiénico");
        escenario.AgregarRequisicionAprobada(empresaId: 1, producto, cantidad: 30);
        escenario.AgregarRequisicionAprobada(empresaId: 2, producto, cantidad: 20);

        var primera = escenario.CrearUseCase().Ejecutar(
            usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        Assert.Single(primera.Detalles);
        Assert.Equal(50, primera.Detalles[0].CantidadNecesaria);
    }

    [Fact]
    public void Segunda_consolidacion_del_mismo_periodo_no_vuelve_a_tomar_las_mismas_requisiciones()
    {
        var escenario = new Escenario();
        var producto = escenario.CrearProducto("Papel higiénico");
        escenario.AgregarRequisicionAprobada(empresaId: 1, producto, cantidad: 30);
        escenario.AgregarRequisicionAprobada(empresaId: 2, producto, cantidad: 20);
        escenario.CrearUseCase().Ejecutar(usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        var segunda = escenario.CrearUseCase().Ejecutar(
            usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        Assert.Empty(segunda.Detalles);
    }

    [Fact]
    public void Una_nueva_requisicion_aprobada_despues_si_puede_entrar_en_una_segunda_consolidacion()
    {
        var escenario = new Escenario();
        var producto = escenario.CrearProducto("Papel higiénico");
        escenario.AgregarRequisicionAprobada(empresaId: 1, producto, cantidad: 30);
        escenario.CrearUseCase().Ejecutar(usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        // Requisición aprobada DESPUÉS de la primera consolidación.
        escenario.AgregarRequisicionAprobada(empresaId: 2, producto, cantidad: 15);

        var segunda = escenario.CrearUseCase().Ejecutar(
            usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        Assert.Single(segunda.Detalles);
        Assert.Equal(15, segunda.Detalles[0].CantidadNecesaria);
    }

    [Fact]
    public void No_se_duplica_la_necesidad_consolidada_entre_dos_consolidaciones_del_mismo_periodo()
    {
        var escenario = new Escenario();
        var producto = escenario.CrearProducto("Papel higiénico");
        escenario.AgregarRequisicionAprobada(empresaId: 1, producto, cantidad: 30);
        var primera = escenario.CrearUseCase().Ejecutar(usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        escenario.AgregarRequisicionAprobada(empresaId: 2, producto, cantidad: 15);
        var segunda = escenario.CrearUseCase().Ejecutar(usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        // La necesidad total real (30 + 15 = 45) queda repartida entre las dos consolidaciones,
        // nunca duplicada dentro de una sola ni repetida entre ambas.
        Assert.Equal(30, primera.Detalles[0].CantidadNecesaria);
        Assert.Equal(15, segunda.Detalles[0].CantidadNecesaria);
    }

    [Fact]
    public void Una_requisicion_con_varios_detalles_se_excluye_por_completo_si_ya_participo()
    {
        var escenario = new Escenario();
        var productoA = escenario.CrearProducto("Papel higiénico");
        var productoB = escenario.CrearProducto("Jabón");
        var empresa = new Empresa(escenario.Ids.Siguiente(), "Empresa 1");
        var requisicion = new Requisicion(escenario.Ids.Siguiente(), empresa, escenario.Periodo, 10, Fecha);
        var detalleA = requisicion.AgregarDetalle(escenario.Ids.Siguiente(), productoA, 10);
        var detalleB = requisicion.AgregarDetalle(escenario.Ids.Siguiente(), productoB, 5);
        var sede = new Sede(escenario.Ids.Siguiente(), empresa, "Sede 1");
        requisicion.AgregarDistribucion(escenario.Ids.Siguiente(), detalleA, sede, 10);
        requisicion.AgregarDistribucion(escenario.Ids.Siguiente(), detalleB, sede, 5);
        requisicion.Enviar(1, Fecha);
        requisicion.IniciarRevision(1, Fecha);
        requisicion.Aprobar(1, Fecha);
        escenario.Requisiciones.Guardar(requisicion);

        escenario.CrearUseCase().Ejecutar(usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));
        var segunda = escenario.CrearUseCase().Ejecutar(usuarioId: 1, Fecha, new CrearConsolidacionRequest(escenario.Periodo.Id, "GENERADA"));

        // Ningún detalle de la misma requisición vuelve a aparecer, aunque tenga productos
        // distintos: se excluye la requisición completa, no detalle por detalle.
        Assert.Empty(segunda.Detalles);
    }
}
