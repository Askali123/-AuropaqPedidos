using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;

namespace Application.Tests;

public class GuardarBorradorRequisicionUseCaseTests
{
    [Fact]
    public void Guardar_borrador_devuelve_la_requisicion_sin_cambiar_su_estado()
    {
        var escenario = new EscenarioDePrueba();
        var requisicion = new Requisicion(escenario.Ids.Siguiente(), escenario.Empresa, escenario.Periodo, 10, new DateTime(2026, 9, 1));
        escenario.Requisiciones.Guardar(requisicion);

        var useCase = new GuardarBorradorRequisicionUseCase(escenario.Requisiciones);
        var respuesta = useCase.Ejecutar(requisicion.Id);

        Assert.Equal(RequisicionEstado.Borrador.ToString(), respuesta.Estado);
    }

    [Fact]
    public void Guardar_borrador_de_requisicion_inexistente_lanza_no_encontrado()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new GuardarBorradorRequisicionUseCase(escenario.Requisiciones);

        Assert.Throws<RecursoNoEncontradoException>(() => useCase.Ejecutar(999));
    }
}
