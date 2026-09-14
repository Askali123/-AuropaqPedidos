using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-010, 04-base-datos.md §9.1: representa una responsabilidad funcional dentro del sistema.
// GLOBAL (sin EmpresaId) — el documento lista únicamente Id/Nombre/Descripcion/Activo, a
// diferencia de Usuario/Sede que sí declaran EmpresaId explícitamente; no se introduce esa
// relación por analogía. Solo campos escalares en el constructor (como Empresa): EF Core puede
// materializarlo directamente sin necesitar un constructor privado adicional.
public sealed class Rol
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; }

    public Rol(int id, string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre del rol es obligatorio.");

        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;
        Activo = true;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
