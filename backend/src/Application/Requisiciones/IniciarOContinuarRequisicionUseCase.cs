using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-022 / docs/05-api.md §18: "crear o recuperar" es una sola operación de negocio.
// Si ya existe una requisición para Empresa+Periodo (sin importar su estado), se devuelve
// esa; nunca se crea una segunda (RN-007/ADR-011). La unicidad se resuelve aquí, mediante
// el repositorio, y no dentro de Domain (instrucción explícita del usuario).
// RN-059/060 (punto 8): empresaId ya no se recibe de X-Empresa-Id — se deriva de
// Usuario.Empresa.Id del usuario autenticado (JWT sub), igual que el resto del sistema no confía
// en un valor enviado por el cliente para determinar la empresa (docs/05-api.md §18,
// docs/06-seguridad.md §5/§6).
public sealed class IniciarOContinuarRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;
    private readonly IUsuarioRepository _usuarios;
    private readonly IPeriodoRepository _periodos;
    private readonly IGeneradorDeIdentificadores _ids;

    public IniciarOContinuarRequisicionUseCase(
        IRequisicionRepository requisiciones,
        IUsuarioRepository usuarios,
        IPeriodoRepository periodos,
        IGeneradorDeIdentificadores ids)
    {
        _requisiciones = requisiciones;
        _usuarios = usuarios;
        _periodos = periodos;
        _ids = ids;
    }

    public RequisicionResponse Ejecutar(int usuarioId, CrearRequisicionRequest request, DateTime fechaActual)
    {
        var usuario = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException("El usuario autenticado no existe.");

        var empresaId = usuario.Empresa.Id;

        var existente = _requisiciones.ObtenerPorEmpresaYPeriodo(empresaId, request.PeriodoId);
        if (existente is not null)
            return RequisicionMapper.AResponse(existente);

        var periodo = _periodos.ObtenerPorId(request.PeriodoId)
            ?? throw new RecursoNoEncontradoException("El periodo indicado no existe.");

        var requisicion = new Requisicion(
            id: _ids.Siguiente(),
            empresa: usuario.Empresa,
            periodo: periodo,
            usuarioCreacionId: usuarioId,
            fechaCreacion: fechaActual);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
