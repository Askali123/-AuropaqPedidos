using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using AuropaqPedidos.Domain.Enums;

namespace Application.Tests;

// RN-059/060 (punto 8): empresaId ya no se pasa explícito, se deriva de Usuario.Empresa.Id.
public class IniciarOContinuarRequisicionUseCaseTests
{
    [Fact]
    public void Crea_una_requisicion_nueva_en_borrador_si_no_existe()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new IniciarOContinuarRequisicionUseCase(
            escenario.Requisiciones, escenario.Usuarios, escenario.Periodos, escenario.Ids);

        var respuesta = useCase.Ejecutar(
            escenario.Usuario.Id, new CrearRequisicionRequest(escenario.Periodo.Id), new DateTime(2026, 9, 1));

        Assert.Equal(RequisicionEstado.Borrador.ToString(), respuesta.Estado);
        Assert.Equal(escenario.Empresa.Id, respuesta.EmpresaId);
        Assert.NotNull(escenario.Requisiciones.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Devuelve_la_requisicion_existente_en_lugar_de_crear_una_nueva()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new IniciarOContinuarRequisicionUseCase(
            escenario.Requisiciones, escenario.Usuarios, escenario.Periodos, escenario.Ids);
        var request = new CrearRequisicionRequest(escenario.Periodo.Id);

        var primera = useCase.Ejecutar(escenario.Usuario.Id, request, new DateTime(2026, 9, 1));
        var segunda = useCase.Ejecutar(escenario.Usuario.Id, request, new DateTime(2026, 9, 1));

        Assert.Equal(primera.Id, segunda.Id);
    }

    [Fact]
    public void Lanza_no_encontrado_si_el_usuario_no_existe()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new IniciarOContinuarRequisicionUseCase(
            escenario.Requisiciones, escenario.Usuarios, escenario.Periodos, escenario.Ids);

        Assert.Throws<RecursoNoEncontradoException>(() =>
            useCase.Ejecutar(usuarioId: 999, new CrearRequisicionRequest(escenario.Periodo.Id), new DateTime(2026, 9, 1)));
    }

    [Fact]
    public void Lanza_no_encontrado_si_el_periodo_no_existe()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new IniciarOContinuarRequisicionUseCase(
            escenario.Requisiciones, escenario.Usuarios, escenario.Periodos, escenario.Ids);

        Assert.Throws<RecursoNoEncontradoException>(() =>
            useCase.Ejecutar(escenario.Usuario.Id, new CrearRequisicionRequest(PeriodoId: 999), new DateTime(2026, 9, 1)));
    }
}
