using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-029, 02-dominio.md §20, 04-base-datos.md §25: mantiene la relación entre una cantidad
// consolidada y el detalle de requisición que la originó. Solo se crea a través de
// DetalleConsolidacion (constructor internal), nunca de forma independiente.
public sealed class AsignacionConsolidacion
{
    public int Id { get; }
    public DetalleRequisicion DetalleRequisicionOrigen { get; }
    public int Cantidad { get; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private AsignacionConsolidacion()
    {
    }
#pragma warning restore CS8618

    internal AsignacionConsolidacion(int id, DetalleRequisicion detalleRequisicionOrigen, int cantidad)
    {
        if (detalleRequisicionOrigen is null)
            throw new ReglaDeNegocioException("Una asignación de consolidación debe referenciar un detalle de requisición.");

        if (cantidad <= 0)
            throw new ReglaDeNegocioException("La cantidad asignada debe ser mayor que cero.");

        Id = id;
        DetalleRequisicionOrigen = detalleRequisicionOrigen;
        Cantidad = cantidad;
    }
}
