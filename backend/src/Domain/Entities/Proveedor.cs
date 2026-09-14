using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-018 (04-base-datos.md §14, 02-dominio.md §10). Implementación mínima: solo los campos
// documentados, necesarios como prerrequisito estructural de PedidoProveedor (TASK-007). El
// resto del alcance de TASK-018 (casos de uso/API de gestión de proveedores) sigue pendiente.
public sealed class Proveedor
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Nit { get; private set; }
    public string? Contacto { get; private set; }
    public string? Telefono { get; private set; }
    public string? Correo { get; private set; }
    public bool Activo { get; private set; }

    public Proveedor(int id, string nombre, string? nit = null, string? contacto = null, string? telefono = null, string? correo = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre del proveedor es obligatorio.");

        Id = id;
        Nombre = nombre;
        Nit = nit;
        Contacto = contacto;
        Telefono = telefono;
        Correo = correo;
        Activo = true;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
