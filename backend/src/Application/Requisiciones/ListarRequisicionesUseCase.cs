using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-031/032, 05-api.md §17.1/§54.6 punto 1: "mis requisiciones" — alcance por empresa (mismo
// criterio ya establecido para enviar/aprobar, RN-058). Sin filtro de Estado/Periodo: no
// documentados como obligatorios para esta consulta (05-api.md §17.1 los deja como "filtros
// futuros posibles", no como requisito actual). RN-059/060 (punto 8): empresaId se deriva de
// Usuario.Empresa.Id del usuario autenticado, no de X-Empresa-Id.
public sealed class ListarRequisicionesUseCase
{
    private readonly IRequisicionRepository _requisiciones;
    private readonly IUsuarioRepository _usuarios;

    public ListarRequisicionesUseCase(IRequisicionRepository requisiciones, IUsuarioRepository usuarios)
    {
        _requisiciones = requisiciones;
        _usuarios = usuarios;
    }

    public IReadOnlyList<RequisicionResponse> Ejecutar(int usuarioId)
    {
        var usuario = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException("El usuario autenticado no existe.");

        return _requisiciones.ObtenerPorEmpresa(usuario.Empresa.Id)
            .Select(RequisicionMapper.AResponse)
            .ToList();
    }
}
