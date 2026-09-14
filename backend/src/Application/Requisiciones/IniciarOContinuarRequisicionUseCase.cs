using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-022 / docs/05-api.md §18: "crear o recuperar" es una sola operación de negocio.
// Si ya existe una requisición para Empresa+Periodo (sin importar su estado), se devuelve
// esa; nunca se crea una segunda (RN-007/ADR-011). La unicidad se resuelve aquí, mediante
// el repositorio, y no dentro de Domain (instrucción explícita del usuario).
public sealed class IniciarOContinuarRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;
    private readonly IEmpresaRepository _empresas;
    private readonly IPeriodoRepository _periodos;
    private readonly IGeneradorDeIdentificadores _ids;

    public IniciarOContinuarRequisicionUseCase(
        IRequisicionRepository requisiciones,
        IEmpresaRepository empresas,
        IPeriodoRepository periodos,
        IGeneradorDeIdentificadores ids)
    {
        _requisiciones = requisiciones;
        _empresas = empresas;
        _periodos = periodos;
        _ids = ids;
    }

    // empresaId y usuarioId deben provenir de la identidad autenticada (docs/05-api.md §18,
    // docs/06-seguridad.md §5/§6), nunca de un valor enviado libremente por el cliente.
    public RequisicionResponse Ejecutar(int empresaId, int usuarioId, CrearRequisicionRequest request, DateTime fechaActual)
    {
        var existente = _requisiciones.ObtenerPorEmpresaYPeriodo(empresaId, request.PeriodoId);
        if (existente is not null)
            return RequisicionMapper.AResponse(existente);

        var empresa = _empresas.ObtenerPorId(empresaId)
            ?? throw new RecursoNoEncontradoException("La empresa indicada no existe.");

        var periodo = _periodos.ObtenerPorId(request.PeriodoId)
            ?? throw new RecursoNoEncontradoException("El periodo indicado no existe.");

        var requisicion = new Requisicion(
            id: _ids.Siguiente(),
            empresa: empresa,
            periodo: periodo,
            usuarioCreacionId: usuarioId,
            fechaCreacion: fechaActual);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
