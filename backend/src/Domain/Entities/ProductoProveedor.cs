using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-019, 02-dominio.md §11, 04-base-datos.md §15: resuelve la homologación entre el código
// interno de un Producto y el código que usa cada Proveedor para el mismo producto. Producto N
// ─── N Proveedor se implementa mediante esta relación (con datos propios, no una simple tabla
// puente): por eso tiene Id propio, a diferencia de UsuarioSede/UsuarioRol/RolPermiso.
// UnidadesPorEmpaque/CantidadMinimaCompra NO se modelan aquí: 01-reglas-negocio.md §15
// (Pendiente 8) descarta esos campos por ahora, sin necesidad de negocio confirmada.
public sealed class ProductoProveedor
{
    public int Id { get; }
    public Producto Producto { get; }
    public Proveedor Proveedor { get; }
    public string CodigoProveedor { get; private set; }
    public string? DescripcionProveedor { get; private set; }
    public string? CategoriaProveedor { get; private set; }
    public string? UnidadProveedor { get; private set; }
    public bool Activo { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private ProductoProveedor()
    {
    }
#pragma warning restore CS8618

    public ProductoProveedor(
        int id, Producto producto, Proveedor proveedor, string codigoProveedor,
        string? descripcionProveedor = null, string? categoriaProveedor = null, string? unidadProveedor = null)
    {
        if (producto is null)
            throw new ReglaDeNegocioException("Una relación producto-proveedor debe tener un producto.");

        if (proveedor is null)
            throw new ReglaDeNegocioException("Una relación producto-proveedor debe tener un proveedor.");

        if (string.IsNullOrWhiteSpace(codigoProveedor))
            throw new ReglaDeNegocioException("El código del proveedor es obligatorio.");

        Id = id;
        Producto = producto;
        Proveedor = proveedor;
        CodigoProveedor = codigoProveedor;
        DescripcionProveedor = descripcionProveedor;
        CategoriaProveedor = categoriaProveedor;
        UnidadProveedor = unidadProveedor;
        Activo = true;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    // 05-api.md §30. Activo se cambia con Activar()/Desactivar(), no aquí (mismo criterio que
    // Producto/Proveedor/Empresa). Producto/Proveedor no se reasignan desde aquí: identifican la
    // relación misma (mismo criterio que Empresa en Sede.ActualizarDatos).
    public void ActualizarDatos(
        string codigoProveedor, string? descripcionProveedor = null, string? categoriaProveedor = null, string? unidadProveedor = null)
    {
        if (string.IsNullOrWhiteSpace(codigoProveedor))
            throw new ReglaDeNegocioException("El código del proveedor es obligatorio.");

        CodigoProveedor = codigoProveedor;
        DescripcionProveedor = descripcionProveedor;
        CategoriaProveedor = categoriaProveedor;
        UnidadProveedor = unidadProveedor;
    }
}
