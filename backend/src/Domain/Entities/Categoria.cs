using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// Clasifica productos dentro del catálogo (Categoria 1 --- N Producto).
public sealed class Categoria
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; }

    public Categoria(int id, string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la categoría es obligatorio.");

        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;
        Activo = true;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    // TASK-014, 05-api.md §14. Activo se cambia con Activar()/Desactivar(), no aquí (mismo
    // criterio ya usado en Empresa/Sede.ActualizarDatos).
    public void ActualizarDatos(string nombre, string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la categoría es obligatorio.");

        Nombre = nombre;
        Descripcion = descripcion;
    }
}
