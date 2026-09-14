using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using AuropaqPedidos.Domain.Enums;

namespace Application.Tests;

public class IniciarOContinuarRequisicionUseCaseTests
{
    [Fact]
    public void Crea_una_requisicion_nueva_en_borrador_si_no_existe()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new IniciarOContinuarRequisicionUseCase(
            escenario.Requisiciones, escenario.Empresas, escenario.Periodos, escenario.Ids);

        var respuesta = useCase.Ejecutar(
            escenario.Empresa.Id, usuarioId: 10, new CrearRequisicionRequest(escenario.Periodo.Id), new DateTime(2026, 9, 1));

        Assert.Equal(RequisicionEstado.Borrador.ToString(), respuesta.Estado);
        Assert.Equal(escenario.Empresa.Id, respuesta.EmpresaId);
        Assert.NotNull(escenario.Requisiciones.ObtenerPorId(respuesta.Id));
    }

    [Fact]
    public void Devuelve_la_requisicion_existente_en_lugar_de_crear_una_nueva()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new IniciarOContinuarRequisicionUseCase(
            escenario.Requisiciones, escenario.Empresas, escenario.Periodos, escenario.Ids);
        var request = new CrearRequisicionRequest(escenario.Periodo.Id);

        var primera = useCase.Ejecutar(escenario.Empresa.Id, 10, request, new DateTime(2026, 9, 1));
        var segunda = useCase.Ejecutar(escenario.Empresa.Id, 10, request, new DateTime(2026, 9, 1));

        Assert.Equal(primera.Id, segunda.Id);
    }

    [Fact]
    public void Lanza_no_encontrado_si_la_empresa_no_existe()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new IniciarOContinuarRequisicionUseCase(
            escenario.Requisiciones, escenario.Empresas, escenario.Periodos, escenario.Ids);

        Assert.Throws<RecursoNoEncontradoException>(() =>
            useCase.Ejecutar(empresaId: 999, usuarioId: 10, new CrearRequisicionRequest(escenario.Periodo.Id), new DateTime(2026, 9, 1)));
    }

    [Fact]
    public void Lanza_no_encontrado_si_el_periodo_no_existe()
    {
        var escenario = new EscenarioDePrueba();
        var useCase = new IniciarOContinuarRequisicionUseCase(
            escenario.Requisiciones, escenario.Empresas, escenario.Periodos, escenario.Ids);

        Assert.Throws<RecursoNoEncontradoException>(() =>
            useCase.Ejecutar(escenario.Empresa.Id, usuarioId: 10, new CrearRequisicionRequest(PeriodoId: 999), new DateTime(2026, 9, 1)));
    }
}
