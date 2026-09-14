using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class DetalleRequisicionUseCasesTests
{
    private static (EscenarioDePrueba escenario, Requisicion requisicion) CrearEscenarioConRequisicion()
    {
        var escenario = new EscenarioDePrueba();
        var requisicion = new Requisicion(escenario.Ids.Siguiente(), escenario.Empresa, escenario.Periodo, 10, new DateTime(2026, 9, 1));
        escenario.Requisiciones.Guardar(requisicion);
        return (escenario, requisicion);
    }

    [Fact]
    public void Agregar_detalle_valido_lo_incluye_en_la_respuesta()
    {
        var (escenario, requisicion) = CrearEscenarioConRequisicion();
        var useCase = new AgregarDetalleRequisicionUseCase(escenario.Requisiciones, escenario.Productos, escenario.Ids);

        var respuesta = useCase.Ejecutar(requisicion.Id, new AgregarDetalleRequisicionRequest(escenario.Producto.Id, 100, "Urgente"));

        Assert.Single(respuesta.Detalles);
        Assert.Equal(100, respuesta.Detalles[0].CantidadSolicitada);
        Assert.Equal("Urgente", respuesta.Detalles[0].Observacion);
    }

    [Fact]
    public void Agregar_detalle_con_producto_inexistente_lanza_no_encontrado()
    {
        var (escenario, requisicion) = CrearEscenarioConRequisicion();
        var useCase = new AgregarDetalleRequisicionUseCase(escenario.Requisiciones, escenario.Productos, escenario.Ids);

        Assert.Throws<RecursoNoEncontradoException>(() =>
            useCase.Ejecutar(requisicion.Id, new AgregarDetalleRequisicionRequest(999, 100, null)));
    }

    [Fact]
    public void Agregar_detalle_con_producto_inactivo_lanza_regla_de_negocio()
    {
        var (escenario, requisicion) = CrearEscenarioConRequisicion();
        escenario.Producto.Desactivar();
        var useCase = new AgregarDetalleRequisicionUseCase(escenario.Requisiciones, escenario.Productos, escenario.Ids);

        Assert.Throws<ReglaDeNegocioException>(() =>
            useCase.Ejecutar(requisicion.Id, new AgregarDetalleRequisicionRequest(escenario.Producto.Id, 100, null)));
    }

    [Fact]
    public void Agregar_detalle_en_requisicion_inexistente_lanza_no_encontrado()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new AgregarDetalleRequisicionUseCase(escenario.Requisiciones, escenario.Productos, escenario.Ids);

        Assert.Throws<RecursoNoEncontradoException>(() =>
            useCase.Ejecutar(requisicionId: 999, new AgregarDetalleRequisicionRequest(escenario.Producto.Id, 100, null)));
    }

    [Fact]
    public void Actualizar_cantidad_y_observacion_del_detalle()
    {
        var (escenario, requisicion) = CrearEscenarioConRequisicion();
        var agregar = new AgregarDetalleRequisicionUseCase(escenario.Requisiciones, escenario.Productos, escenario.Ids);
        var respuestaInicial = agregar.Ejecutar(requisicion.Id, new AgregarDetalleRequisicionRequest(escenario.Producto.Id, 100, "Inicial"));
        var detalleId = respuestaInicial.Detalles[0].Id;

        var actualizar = new ActualizarDetalleRequisicionUseCase(escenario.Requisiciones);
        var respuesta = actualizar.Ejecutar(requisicion.Id, detalleId, new ActualizarDetalleRequisicionRequest(150, "Actualizada"));

        Assert.Equal(150, respuesta.Detalles[0].CantidadSolicitada);
        Assert.Equal("Actualizada", respuesta.Detalles[0].Observacion);
    }

    [Fact]
    public void Actualizar_detalle_inexistente_lanza_no_encontrado()
    {
        var (escenario, requisicion) = CrearEscenarioConRequisicion();
        var actualizar = new ActualizarDetalleRequisicionUseCase(escenario.Requisiciones);

        Assert.Throws<RecursoNoEncontradoException>(() =>
            actualizar.Ejecutar(requisicion.Id, detalleId: 999, new ActualizarDetalleRequisicionRequest(150, null)));
    }

    [Fact]
    public void Eliminar_detalle_lo_remueve_de_la_respuesta()
    {
        var (escenario, requisicion) = CrearEscenarioConRequisicion();
        var agregar = new AgregarDetalleRequisicionUseCase(escenario.Requisiciones, escenario.Productos, escenario.Ids);
        var respuestaInicial = agregar.Ejecutar(requisicion.Id, new AgregarDetalleRequisicionRequest(escenario.Producto.Id, 100, null));
        var detalleId = respuestaInicial.Detalles[0].Id;

        var eliminar = new EliminarDetalleRequisicionUseCase(escenario.Requisiciones);
        var respuesta = eliminar.Ejecutar(requisicion.Id, detalleId);

        Assert.Empty(respuesta.Detalles);
    }

    [Fact]
    public void Eliminar_detalle_inexistente_lanza_no_encontrado()
    {
        var (escenario, requisicion) = CrearEscenarioConRequisicion();
        var eliminar = new EliminarDetalleRequisicionUseCase(escenario.Requisiciones);

        Assert.Throws<RecursoNoEncontradoException>(() => eliminar.Ejecutar(requisicion.Id, detalleId: 999));
    }
}
