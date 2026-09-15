using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-017, 05-api.md §28.3. Solicitud/Producto inexistentes -> 404; solicitud no pendiente ->
// 422 (lanzado por SolicitudProductoCatalogo.Homologar, Domain). usuarioResolucionId NO se valida
// contra IUsuarioRepository (mismo criterio que SolicitarProductoNoCatalogadoUseCase).
public sealed class HomologarProductoUseCase
{
    private readonly ISolicitudProductoCatalogoRepository _solicitudes;
    private readonly IProductoRepository _productos;

    public HomologarProductoUseCase(ISolicitudProductoCatalogoRepository solicitudes, IProductoRepository productos)
    {
        _solicitudes = solicitudes;
        _productos = productos;
    }

    public SolicitudProductoCatalogoResponse Ejecutar(int id, HomologarProductoRequest request, int usuarioResolucionId, DateTime fecha)
    {
        var solicitud = _solicitudes.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La solicitud {id} no existe.");

        var producto = _productos.ObtenerPorId(request.ProductoId)
            ?? throw new RecursoNoEncontradoException($"El producto {request.ProductoId} no existe.");

        solicitud.Homologar(producto, usuarioResolucionId, fecha);

        _solicitudes.Guardar(solicitud);

        return SolicitudProductoCatalogoMapper.AResponse(solicitud);
    }
}
