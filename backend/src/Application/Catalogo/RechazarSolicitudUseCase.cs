using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-017, 05-api.md §28.5. Motivo vacío -> 422 (lanzado por SolicitudProductoCatalogo.Rechazar,
// Domain, RN-026). usuarioResolucionId NO se valida contra IUsuarioRepository (mismo criterio que
// SolicitarProductoNoCatalogadoUseCase).
public sealed class RechazarSolicitudUseCase
{
    private readonly ISolicitudProductoCatalogoRepository _solicitudes;

    public RechazarSolicitudUseCase(ISolicitudProductoCatalogoRepository solicitudes)
    {
        _solicitudes = solicitudes;
    }

    public SolicitudProductoCatalogoResponse Ejecutar(int id, RechazarSolicitudRequest request, int usuarioResolucionId, DateTime fecha)
    {
        var solicitud = _solicitudes.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La solicitud {id} no existe.");

        solicitud.Rechazar(usuarioResolucionId, fecha, request.Motivo);

        _solicitudes.Guardar(solicitud);

        return SolicitudProductoCatalogoMapper.AResponse(solicitud);
    }
}
