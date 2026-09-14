using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-008, 04-base-datos.md §7: representa una persona que utiliza el sistema. Un Usuario
// pertenece a exactamente una Empresa (Empresa 1 --- N Usuario), mismo patrón que Sede.
// TASK-015: agrega PasswordHash (autenticación). Domain nunca ve la contraseña en texto plano ni
// conoce el algoritmo de hash — ese es un detalle de Infrastructure (IPasswordHasher);
// aquí solo se garantiza que exista un hash no vacío, mismo criterio que Correo/Nombre.
public sealed class Usuario
{
    public int Id { get; private set; }
    public Empresa Empresa { get; private set; }
    public string Nombre { get; private set; }
    public string? Apellido { get; private set; }
    public string Correo { get; private set; }
    public string PasswordHash { get; private set; }
    public bool Activo { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaActualizacion { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private Usuario()
    {
    }
#pragma warning restore CS8618

    public Usuario(
        int id,
        Empresa empresa,
        string nombre,
        string correo,
        string passwordHash,
        DateTime fechaCreacion,
        string? apellido = null)
    {
        if (empresa is null)
            throw new ReglaDeNegocioException("Un usuario debe pertenecer a una empresa.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre del usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(correo))
            throw new ReglaDeNegocioException("El correo del usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ReglaDeNegocioException("El usuario debe tener credenciales.");

        Id = id;
        Empresa = empresa;
        Nombre = nombre;
        Apellido = apellido;
        Correo = correo;
        PasswordHash = passwordHash;
        Activo = true;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    // Mismo criterio que Requisicion (Domain no lee el reloj): la fecha la decide quien llama
    // (Application), para mantener el dominio puro y comprobable.
    public void Activar(DateTime fecha)
    {
        Activo = true;
        FechaActualizacion = fecha;
    }

    public void Desactivar(DateTime fecha)
    {
        Activo = false;
        FechaActualizacion = fecha;
    }
}
