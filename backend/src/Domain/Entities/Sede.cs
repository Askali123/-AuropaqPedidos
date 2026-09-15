using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-002: cada sede pertenece a exactamente una empresa (Empresa 1 --- N Sede).
public sealed class Sede
{
    public int Id { get; private set; }
    public Empresa Empresa { get; private set; }
    public string Nombre { get; private set; }
    public string? Direccion { get; private set; }
    public string? Ciudad { get; private set; }
    public string? Departamento { get; private set; }
    public string? Telefono { get; private set; }
    public string? Contacto { get; private set; }
    public bool Activo { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private Sede()
    {
    }
#pragma warning restore CS8618

    public Sede(
        int id,
        Empresa empresa,
        string nombre,
        string? direccion = null,
        string? ciudad = null,
        string? departamento = null,
        string? telefono = null,
        string? contacto = null)
    {
        if (empresa is null)
            throw new ReglaDeNegocioException("Una sede debe pertenecer a una empresa.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la sede es obligatorio.");

        Id = id;
        Empresa = empresa;
        Nombre = nombre;
        Direccion = direccion;
        Ciudad = ciudad;
        Departamento = departamento;
        Telefono = telefono;
        Contacto = contacto;
        Activo = true;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    // TASK-007: "Actualizar sede" (05-api.md §12.3). Activo se cambia con Activar()/Desactivar(),
    // no aquí (mismo criterio que Empresa.ActualizarDatos). Empresa no se puede reasignar: una sede
    // no puede pasar de una empresa a otra sin que exista una regla de negocio que lo autorice
    // (RN-002 — "cada sede pertenece a una única empresa").
    public void ActualizarDatos(
        string nombre,
        string? direccion = null,
        string? ciudad = null,
        string? departamento = null,
        string? telefono = null,
        string? contacto = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la sede es obligatorio.");

        Nombre = nombre;
        Direccion = direccion;
        Ciudad = ciudad;
        Departamento = departamento;
        Telefono = telefono;
        Contacto = contacto;
    }
}
