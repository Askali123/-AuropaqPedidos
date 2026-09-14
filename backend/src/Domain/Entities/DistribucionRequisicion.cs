using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-010/RN-011: cómo se distribuye la cantidad de un detalle entre las sedes de la empresa.
// Solo se crea/modifica a través de DetalleRequisicion (constructor y mutadores internal),
// que es quien conoce y protege el límite de la cantidad solicitada.
public sealed class DistribucionRequisicion
{
    public int Id { get; }
    public Sede Sede { get; }
    public int Cantidad { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private DistribucionRequisicion()
    {
    }
#pragma warning restore CS8618

    internal DistribucionRequisicion(int id, Sede sede, int cantidad)
    {
        if (sede is null)
            throw new ReglaDeNegocioException("Una distribución debe tener una sede.");

        if (cantidad <= 0)
            throw new ReglaDeNegocioException("La cantidad distribuida debe ser mayor que cero.");

        Id = id;
        Sede = sede;
        Cantidad = cantidad;
    }

    internal void ModificarCantidad(int nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
            throw new ReglaDeNegocioException("La cantidad distribuida debe ser mayor que cero.");

        Cantidad = nuevaCantidad;
    }
}
