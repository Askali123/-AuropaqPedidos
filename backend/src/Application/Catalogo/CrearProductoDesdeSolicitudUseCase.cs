using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-017, 05-api.md §28.4: "el backend debe registrar qué usuario resolvió la solicitud" — ya
// garantizado por SolicitudProductoCatalogo.Crear (Domain), que exige usuarioResolucionId (sin
// validarlo contra IUsuarioRepository, mismo criterio que SolicitarProductoNoCatalogadoUseCase).
// Nombre/Descripcion del Producto nuevo se toman de la solicitud (ver CrearProductoDesdeSolicitudRequest).
public sealed class CrearProductoDesdeSolicitudUseCase
{
    private readonly ISolicitudProductoCatalogoRepository _solicitudes;
    private readonly IProductoRepository _productos;
    private readonly ICategoriaRepository _categorias;
    private readonly IUnidadMedidaRepository _unidadesMedida;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearProductoDesdeSolicitudUseCase(
        ISolicitudProductoCatalogoRepository solicitudes,
        IProductoRepository productos,
        ICategoriaRepository categorias,
        IUnidadMedidaRepository unidadesMedida,
        IGeneradorDeIdentificadores ids)
    {
        _solicitudes = solicitudes;
        _productos = productos;
        _categorias = categorias;
        _unidadesMedida = unidadesMedida;
        _ids = ids;
    }

    public SolicitudProductoCatalogoResponse Ejecutar(
        int id, CrearProductoDesdeSolicitudRequest request, int usuarioResolucionId, DateTime fecha)
    {
        var solicitud = _solicitudes.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La solicitud {id} no existe.");

        var categoria = _categorias.ObtenerPorId(request.CategoriaId)
            ?? throw new RecursoNoEncontradoException($"La categoría {request.CategoriaId} no existe.");

        var unidadMedida = _unidadesMedida.ObtenerPorId(request.UnidadMedidaId)
            ?? throw new RecursoNoEncontradoException($"La unidad de medida {request.UnidadMedidaId} no existe.");

        var producto = new Producto(
            _ids.Siguiente(), solicitud.NombreSolicitado, categoria, unidadMedida,
            request.CodigoInterno, solicitud.Descripcion);
        _productos.Guardar(producto);

        solicitud.Crear(producto, usuarioResolucionId, fecha);
        _solicitudes.Guardar(solicitud);

        return SolicitudProductoCatalogoMapper.AResponse(solicitud);
    }
}
