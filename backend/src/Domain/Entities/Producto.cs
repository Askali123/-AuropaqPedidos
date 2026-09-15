using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-021: el catálogo interno es la referencia oficial de productos.
public sealed class Producto
{
    public int Id { get; private set; }
    public string? CodigoInterno { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public Categoria Categoria { get; private set; }
    public UnidadMedida UnidadMedida { get; private set; }
    public bool Activo { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private Producto()
    {
    }
#pragma warning restore CS8618

    public Producto(
        int id,
        string nombre,
        Categoria categoria,
        UnidadMedida unidadMedida,
        string? codigoInterno = null,
        string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre del producto es obligatorio.");

        if (categoria is null)
            throw new ReglaDeNegocioException("Un producto debe pertenecer a una categoría.");

        if (unidadMedida is null)
            throw new ReglaDeNegocioException("Un producto debe tener una unidad de medida.");

        Id = id;
        Nombre = nombre;
        Categoria = categoria;
        UnidadMedida = unidadMedida;
        CodigoInterno = codigoInterno;
        Descripcion = descripcion;
        Activo = true;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    // TASK-016, 05-api.md §13.4. Activo se cambia con Activar()/Desactivar(), no aquí. Sin
    // unicidad de CodigoInterno: 04-base-datos.md §12 la deja pendiente de confirmación del
    // negocio (mismo criterio ya usado para Empresa.Nit; no se inventa aquí).
    public void ActualizarDatos(
        string nombre,
        Categoria categoria,
        UnidadMedida unidadMedida,
        string? codigoInterno = null,
        string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre del producto es obligatorio.");

        if (categoria is null)
            throw new ReglaDeNegocioException("Un producto debe pertenecer a una categoría.");

        if (unidadMedida is null)
            throw new ReglaDeNegocioException("Un producto debe tener una unidad de medida.");

        Nombre = nombre;
        Categoria = categoria;
        UnidadMedida = unidadMedida;
        CodigoInterno = codigoInterno;
        Descripcion = descripcion;
    }
}
