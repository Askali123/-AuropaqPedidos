using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-001: las empresas son datos configurables, no deben hardcodearse en el sistema.
public sealed class Empresa
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Nit { get; private set; }
    public bool Activo { get; private set; }

    public Empresa(int id, string nombre, string? nit = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la empresa es obligatorio.");

        Id = id;
        Nombre = nombre;
        Nit = nit;
        Activo = true;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    // TASK-006: "Modificar empresa" (05-api.md §11.4). Activo se cambia con Activar()/Desactivar(),
    // no aquí, para mantener explícita la transición de estado (mismo criterio que el resto del
    // dominio: RN no mezcla "editar datos" con "cambiar estado").
    public void ActualizarDatos(string nombre, string? nit)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la empresa es obligatorio.");

        Nombre = nombre;
        Nit = nit;
    }
}
