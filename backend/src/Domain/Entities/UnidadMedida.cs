using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// Unidad utilizada para expresar cantidades (UnidadMedida 1 --- N Producto).
public sealed class UnidadMedida
{
    public int Id { get; private set; }
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public bool Activo { get; private set; }

    public UnidadMedida(int id, string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ReglaDeNegocioException("El código de la unidad de medida es obligatorio.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la unidad de medida es obligatorio.");

        Id = id;
        Codigo = codigo;
        Nombre = nombre;
        Activo = true;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    // TASK-015, 05-api.md §15. Activo se cambia con Activar()/Desactivar(), no aquí.
    public void ActualizarDatos(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ReglaDeNegocioException("El código de la unidad de medida es obligatorio.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la unidad de medida es obligatorio.");

        Codigo = codigo;
        Nombre = nombre;
    }
}
