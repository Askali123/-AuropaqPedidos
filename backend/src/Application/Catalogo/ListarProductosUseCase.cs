using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// Habilita el selector de producto del Frontend de Requisiciones (docs/05-api.md §54.6). Devuelve
// el catálogo completo sin filtrar por Activo ni por ninguna otra condición: la regla de que un
// producto inactivo no puede agregarse a una requisición ya existe en
// AgregarDetalleRequisicionUseCase (que valida Producto.Activo en el momento de agregar, no al
// listar) — instrucción explícita de no inventar aquí un filtro ni una regla de disponibilidad
// nueva.
public sealed class ListarProductosUseCase
{
    private readonly IProductoRepository _productos;

    public ListarProductosUseCase(IProductoRepository productos)
    {
        _productos = productos;
    }

    public IReadOnlyList<ProductoResponse> Ejecutar() =>
        _productos.ObtenerTodos()
            .Select(p => new ProductoResponse(
                p.Id, p.Nombre, p.CodigoInterno, p.Descripcion,
                p.Categoria.Id, p.Categoria.Nombre,
                p.UnidadMedida.Id, p.UnidadMedida.Codigo, p.UnidadMedida.Nombre, p.Activo))
            .ToList();
}
