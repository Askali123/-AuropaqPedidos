using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests;

public class FlujoEstadosRequisicionUseCasesTests
{
    private static readonly DateTime DentroDeVentana = new(2026, 9, 2);

    private static (EscenarioDePrueba escenario, Requisicion requisicion) CrearRequisicionListaParaEnviar()
    {
        var escenario = new EscenarioDePrueba();
        var requisicion = new Requisicion(escenario.Ids.Siguiente(), escenario.Empresa, escenario.Periodo, 10, new DateTime(2026, 9, 1));
        escenario.Requisiciones.Guardar(requisicion);

        var agregarDetalle = new AgregarDetalleRequisicionUseCase(escenario.Requisiciones, escenario.Productos, escenario.Ids);
        var respuesta = agregarDetalle.Ejecutar(requisicion.Id, new AgregarDetalleRequisicionRequest(escenario.Producto.Id, 100, null));
        var detalleId = respuesta.Detalles[0].Id;

        var agregarDistribucion = new AgregarDistribucionRequisicionUseCase(escenario.Requisiciones, escenario.Sedes, escenario.Ids);
        agregarDistribucion.Ejecutar(requisicion.Id, detalleId, escenario.Sede.Id, 100);

        return (escenario, requisicion);
    }

    [Fact]
    public void Enviar_requisicion_completa_cambia_su_estado()
    {
        var (escenario, requisicion) = CrearRequisicionListaParaEnviar();
        var useCase = new EnviarRequisicionUseCase(escenario.Requisiciones, NullLogger<EnviarRequisicionUseCase>.Instance);

        var respuesta = useCase.Ejecutar(requisicion.Id, usuarioId: 1, DentroDeVentana);

        Assert.Equal(RequisicionEstado.Enviada.ToString(), respuesta.Estado);
        Assert.Single(respuesta.Historial);
    }

    [Fact]
    public void Enviar_requisicion_sin_distribucion_completa_propaga_error_de_dominio()
    {
        var escenario = new EscenarioDePrueba();
        var requisicion = new Requisicion(escenario.Ids.Siguiente(), escenario.Empresa, escenario.Periodo, 10, new DateTime(2026, 9, 1));
        escenario.Requisiciones.Guardar(requisicion);
        var agregarDetalle = new AgregarDetalleRequisicionUseCase(escenario.Requisiciones, escenario.Productos, escenario.Ids);
        agregarDetalle.Ejecutar(requisicion.Id, new AgregarDetalleRequisicionRequest(escenario.Producto.Id, 100, null));

        var useCase = new EnviarRequisicionUseCase(escenario.Requisiciones, NullLogger<EnviarRequisicionUseCase>.Instance);

        Assert.Throws<ReglaDeNegocioException>(() => useCase.Ejecutar(requisicion.Id, 1, DentroDeVentana));
    }

    [Fact]
    public void Enviar_requisicion_inexistente_lanza_no_encontrado()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new EnviarRequisicionUseCase(escenario.Requisiciones, NullLogger<EnviarRequisicionUseCase>.Instance);

        Assert.Throws<RecursoNoEncontradoException>(() => useCase.Ejecutar(999, 1, DentroDeVentana));
    }

    [Fact]
    public void Iniciar_revision_solo_es_valido_despues_de_enviar()
    {
        var (escenario, requisicion) = CrearRequisicionListaParaEnviar();
        new EnviarRequisicionUseCase(escenario.Requisiciones, NullLogger<EnviarRequisicionUseCase>.Instance).Ejecutar(requisicion.Id, 1, DentroDeVentana);

        var iniciarRevision = new IniciarRevisionRequisicionUseCase(escenario.Requisiciones);
        var respuesta = iniciarRevision.Ejecutar(requisicion.Id, usuarioId: 1, DentroDeVentana);

        Assert.Equal(RequisicionEstado.EnRevision.ToString(), respuesta.Estado);
    }

    [Fact]
    public void Aprobar_requisicion_en_revision_cambia_su_estado()
    {
        var (escenario, requisicion) = CrearRequisicionListaParaEnviar();
        new EnviarRequisicionUseCase(escenario.Requisiciones, NullLogger<EnviarRequisicionUseCase>.Instance).Ejecutar(requisicion.Id, 1, DentroDeVentana);
        new IniciarRevisionRequisicionUseCase(escenario.Requisiciones).Ejecutar(requisicion.Id, 1, DentroDeVentana);

        var aprobar = new AprobarRequisicionUseCase(escenario.Requisiciones, NullLogger<AprobarRequisicionUseCase>.Instance);
        var respuesta = aprobar.Ejecutar(requisicion.Id, usuarioId: 2, DentroDeVentana, new AprobarRequisicionRequest("Cumple los requisitos"));

        Assert.Equal(RequisicionEstado.Aprobada.ToString(), respuesta.Estado);
    }

    [Fact]
    public void Devolver_requisicion_en_revision_exige_motivo_y_cambia_su_estado()
    {
        var (escenario, requisicion) = CrearRequisicionListaParaEnviar();
        new EnviarRequisicionUseCase(escenario.Requisiciones, NullLogger<EnviarRequisicionUseCase>.Instance).Ejecutar(requisicion.Id, 1, DentroDeVentana);
        new IniciarRevisionRequisicionUseCase(escenario.Requisiciones).Ejecutar(requisicion.Id, 1, DentroDeVentana);

        var devolver = new DevolverRequisicionUseCase(escenario.Requisiciones, NullLogger<DevolverRequisicionUseCase>.Instance);
        var respuesta = devolver.Ejecutar(requisicion.Id, usuarioId: 2, DentroDeVentana, new DevolverRequisicionRequest("Cantidad incorrecta"));

        Assert.Equal(RequisicionEstado.Devuelta.ToString(), respuesta.Estado);
        Assert.Equal("Cantidad incorrecta", respuesta.Historial[^1].Comentario);
    }

    [Fact]
    public void Devolver_sin_motivo_propaga_error_de_dominio()
    {
        var (escenario, requisicion) = CrearRequisicionListaParaEnviar();
        new EnviarRequisicionUseCase(escenario.Requisiciones, NullLogger<EnviarRequisicionUseCase>.Instance).Ejecutar(requisicion.Id, 1, DentroDeVentana);
        new IniciarRevisionRequisicionUseCase(escenario.Requisiciones).Ejecutar(requisicion.Id, 1, DentroDeVentana);

        var devolver = new DevolverRequisicionUseCase(escenario.Requisiciones, NullLogger<DevolverRequisicionUseCase>.Instance);

        Assert.Throws<ReglaDeNegocioException>(() =>
            devolver.Ejecutar(requisicion.Id, 2, DentroDeVentana, new DevolverRequisicionRequest("")));
    }

    [Fact]
    public void Requisicion_devuelta_puede_corregirse_y_reenviarse()
    {
        var (escenario, requisicion) = CrearRequisicionListaParaEnviar();
        new EnviarRequisicionUseCase(escenario.Requisiciones, NullLogger<EnviarRequisicionUseCase>.Instance).Ejecutar(requisicion.Id, 1, DentroDeVentana);
        new IniciarRevisionRequisicionUseCase(escenario.Requisiciones).Ejecutar(requisicion.Id, 1, DentroDeVentana);
        var respuestaDevuelta = new DevolverRequisicionUseCase(escenario.Requisiciones, NullLogger<DevolverRequisicionUseCase>.Instance)
            .Ejecutar(requisicion.Id, 2, DentroDeVentana, new DevolverRequisicionRequest("Corregir cantidad"));

        var detalleId = respuestaDevuelta.Detalles[0].Id;

        // corrección: mientras está DEVUELTA puede modificarse y redistribuirse
        var actualizar = new ActualizarDetalleRequisicionUseCase(escenario.Requisiciones);
        actualizar.Ejecutar(requisicion.Id, detalleId, new ActualizarDetalleRequisicionRequest(120, null));

        var agregarDistribucion = new AgregarDistribucionRequisicionUseCase(escenario.Requisiciones, escenario.Sedes, escenario.Ids);
        agregarDistribucion.Ejecutar(requisicion.Id, detalleId, escenario.SedeAlterna.Id, 20);

        // reenvío
        var reenviar = new EnviarRequisicionUseCase(escenario.Requisiciones, NullLogger<EnviarRequisicionUseCase>.Instance);
        var respuesta = reenviar.Ejecutar(requisicion.Id, usuarioId: 1, DentroDeVentana.AddHours(1));

        Assert.Equal(RequisicionEstado.Enviada.ToString(), respuesta.Estado);
        // Borrador->Enviada, Enviada->EnRevision, EnRevision->Devuelta, Devuelta->Enviada (reenvío).
        Assert.Equal(4, respuesta.Historial.Count);
    }
}
