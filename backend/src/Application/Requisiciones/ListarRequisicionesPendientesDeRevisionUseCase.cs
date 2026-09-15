using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-031, 05-api.md §24.1/§54.6 punto 1: bandeja de revisión. Alcance por empresa (mismo
// criterio que Enviar/Aprobar, RN-058) — un revisor solo ve pendientes de su propia empresa.
// RN-059/060 (punto 8): empresaId se deriva de Usuario.Empresa.Id del usuario autenticado.
public sealed class ListarRequisicionesPendientesDeRevisionUseCase
{
    private readonly IRequisicionRepository _requisiciones;
    private readonly IUsuarioRepository _usuarios;

    public ListarRequisicionesPendientesDeRevisionUseCase(IRequisicionRepository requisiciones, IUsuarioRepository usuarios)
    {
        _requisiciones = requisiciones;
        _usuarios = usuarios;
    }

    public IReadOnlyList<RequisicionResponse> Ejecutar(int usuarioId)
    {
        var usuario = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException("El usuario autenticado no existe.");

        return _requisiciones.ObtenerEnRevisionPorEmpresa(usuario.Empresa.Id)
            .Select(RequisicionMapper.AResponse)
            .ToList();
    }
}
