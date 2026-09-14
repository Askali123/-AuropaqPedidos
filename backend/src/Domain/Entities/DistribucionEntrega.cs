using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-035, ADR-019, 02-dominio.md §26, 04-base-datos.md §31: cuánto de una entrega fue
// destinado a una sede. La dirección/ciudad/contacto se congelan (snapshot) en el momento de
// construir la distribución, a partir de la Sede vigente en ese instante — nunca se vuelven a
// leer de Sede después, para que una entrega histórica no cambie si la sede cambia de
// dirección más adelante. Solo se crea a través de DetalleEntrega (constructor internal).
public sealed class DistribucionEntrega
{
    public int Id { get; }
    public Sede Sede { get; }
    public int Cantidad { get; }
    public string? DireccionEntrega { get; }
    public string? CiudadEntrega { get; }
    public string? ContactoEntrega { get; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private DistribucionEntrega()
    {
    }
#pragma warning restore CS8618

    internal DistribucionEntrega(int id, Sede sede, int cantidad)
    {
        if (sede is null)
            throw new ReglaDeNegocioException("Una distribución de entrega debe tener una sede.");

        if (cantidad <= 0)
            throw new ReglaDeNegocioException("La cantidad distribuida debe ser mayor que cero.");

        Id = id;
        Sede = sede;
        Cantidad = cantidad;

        // Snapshot histórico (RN-035/ADR-019): se copia el valor vigente de la sede en este
        // instante, no una referencia que cambie si la sede se actualiza después.
        DireccionEntrega = sede.Direccion;
        CiudadEntrega = sede.Ciudad;
        ContactoEntrega = sede.Contacto;
    }
}
