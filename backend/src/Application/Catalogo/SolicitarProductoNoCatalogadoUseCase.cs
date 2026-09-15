using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-017, 05-api.md §28.1. RN-059/060 (punto 8): empresaId se deriva de Usuario.Empresa.Id del
// usuario autenticado (JWT), no de X-Empresa-Id — mismo criterio que
// IniciarOContinuarRequisicionUseCase.
public sealed class SolicitarProductoNoCatalogadoUseCase
{
    private readonly ISolicitudProductoCatalogoRepository _solicitudes;
    private readonly IUsuarioRepository _usuarios;
    private readonly IGeneradorDeIdentificadores _ids;

    public SolicitarProductoNoCatalogadoUseCase(
        ISolicitudProductoCatalogoRepository solicitudes,
        IUsuarioRepository usuarios,
        IGeneradorDeIdentificadores ids)
    {
        _solicitudes = solicitudes;
        _usuarios = usuarios;
        _ids = ids;
    }

    public SolicitudProductoCatalogoResponse Ejecutar(int usuarioId, SolicitarProductoRequest request, DateTime fecha)
    {
        var usuario = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException("El usuario autenticado no existe.");

        var solicitud = new SolicitudProductoCatalogo(
            _ids.Siguiente(), usuario.Empresa, usuarioId, request.NombreSolicitado, fecha, request.Descripcion, request.Observacion);

        _solicitudes.Guardar(solicitud);

        return SolicitudProductoCatalogoMapper.AResponse(solicitud);
    }
}
