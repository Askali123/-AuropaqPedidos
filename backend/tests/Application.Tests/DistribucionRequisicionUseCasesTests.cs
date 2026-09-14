using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class DistribucionRequisicionUseCasesTests
{
    private static (EscenarioDePrueba escenario, Requisicion requisicion, int detalleId) CrearEscenarioConDetalle(int cantidad = 100)
    {
        var escenario = new EscenarioDePrueba();
        var requisicion = new Requisicion(escenario.Ids.Siguiente(), escenario.Empresa, escenario.Periodo, 10, new DateTime(2026, 9, 1));
        escenario.Requisiciones.Guardar(requisicion);

        var agregarDetalle = new AgregarDetalleRequisicionUseCase(escenario.Requisiciones, escenario.Productos, escenario.Ids);
        var respuesta = agregarDetalle.Ejecutar(requisicion.Id, new AgregarDetalleRequisicionRequest(escenario.Producto.Id, cantidad, null));

        return (escenario, requisicion, respuesta.Detalles[0].Id);
    }

    [Fact]
    public void Agregar_distribucion_valida_actualiza_la_cantidad_distribuida()
    {
        var (escenario, requisicion, detalleId) = CrearEscenarioConDetalle(100);
        var useCase = new AgregarDistribucionRequisicionUseCase(escenario.Requisiciones, escenario.Sedes, escenario.Ids);

        var respuesta = useCase.Ejecutar(requisicion.Id, detalleId, escenario.Sede.Id, 60);

        Assert.Equal(60, respuesta.Detalles[0].CantidadDistribuida);
    }

    [Fact]
    public void Agregar_distribucion_con_sede_inexistente_lanza_no_encontrado()
    {
        var (escenario, requisicion, detalleId) = CrearEscenarioConDetalle(100);
        var useCase = new AgregarDistribucionRequisicionUseCase(escenario.Requisiciones, escenario.Sedes, escenario.Ids);

        Assert.Throws<RecursoNoEncontradoException>(() => useCase.Ejecutar(requisicion.Id, detalleId, sedeId: 999, cantidad: 60));
    }

    [Fact]
    public void Agregar_distribucion_con_sede_inactiva_lanza_regla_de_negocio()
    {
        var (escenario, requisicion, detalleId) = CrearEscenarioConDetalle(100);
        escenario.Sede.Desactivar();
        var useCase = new AgregarDistribucionRequisicionUseCase(escenario.Requisiciones, escenario.Sedes, escenario.Ids);

        Assert.Throws<ReglaDeNegocioException>(() => useCase.Ejecutar(requisicion.Id, detalleId, escenario.Sede.Id, 60));
    }

    [Fact]
    public void Agregar_distribucion_que_supera_la_cantidad_solicitada_propaga_error_de_dominio()
    {
        var (escenario, requisicion, detalleId) = CrearEscenarioConDetalle(100);
        var useCase = new AgregarDistribucionRequisicionUseCase(escenario.Requisiciones, escenario.Sedes, escenario.Ids);

        Assert.Throws<ReglaDeNegocioException>(() => useCase.Ejecutar(requisicion.Id, detalleId, escenario.Sede.Id, 101));
    }

    [Fact]
    public void Modificar_distribucion_actualiza_la_cantidad()
    {
        var (escenario, requisicion, detalleId) = CrearEscenarioConDetalle(100);
        var agregar = new AgregarDistribucionRequisicionUseCase(escenario.Requisiciones, escenario.Sedes, escenario.Ids);
        var respuestaInicial = agregar.Ejecutar(requisicion.Id, detalleId, escenario.Sede.Id, 60);
        var distribucionId = respuestaInicial.Detalles[0].Distribuciones[0].Id;

        var modificar = new ModificarDistribucionRequisicionUseCase(escenario.Requisiciones);
        var respuesta = modificar.Ejecutar(requisicion.Id, detalleId, distribucionId, 80);

        Assert.Equal(80, respuesta.Detalles[0].Distribuciones[0].Cantidad);
    }

    [Fact]
    public void Modificar_distribucion_inexistente_lanza_no_encontrado()
    {
        var (escenario, requisicion, detalleId) = CrearEscenarioConDetalle(100);
        var modificar = new ModificarDistribucionRequisicionUseCase(escenario.Requisiciones);

        Assert.Throws<RecursoNoEncontradoException>(() =>
            modificar.Ejecutar(requisicion.Id, detalleId, distribucionId: 999, nuevaCantidad: 10));
    }

    [Fact]
    public void Eliminar_distribucion_la_remueve_de_la_respuesta()
    {
        var (escenario, requisicion, detalleId) = CrearEscenarioConDetalle(100);
        var agregar = new AgregarDistribucionRequisicionUseCase(escenario.Requisiciones, escenario.Sedes, escenario.Ids);
        var respuestaInicial = agregar.Ejecutar(requisicion.Id, detalleId, escenario.Sede.Id, 60);
        var distribucionId = respuestaInicial.Detalles[0].Distribuciones[0].Id;

        var eliminar = new EliminarDistribucionRequisicionUseCase(escenario.Requisiciones);
        var respuesta = eliminar.Ejecutar(requisicion.Id, detalleId, distribucionId);

        Assert.Empty(respuesta.Detalles[0].Distribuciones);
    }
}
