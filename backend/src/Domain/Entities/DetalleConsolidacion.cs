using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-027, 02-dominio.md §19, 04-base-datos.md §24: cantidad total necesaria de un producto,
// derivada de las asignaciones que la componen. Solo se crea/muta a través de Consolidacion
// (constructor y mutadores internal), que es quien agrupa por producto (regla "no mezclar
// productos diferentes").
public sealed class DetalleConsolidacion
{
    public int Id { get; }
    public Producto Producto { get; }

    private readonly List<AsignacionConsolidacion> _asignaciones = new();
    public IReadOnlyList<AsignacionConsolidacion> Asignaciones => _asignaciones;

    // CantidadNecesaria es siempre la suma de las asignaciones: no se almacena por separado
    // para no poder quedar desincronizada (mismo criterio que DetalleRequisicion.CantidadDistribuida).
    public int CantidadNecesaria => _asignaciones.Sum(a => a.Cantidad);

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private DetalleConsolidacion()
    {
    }
#pragma warning restore CS8618

    internal DetalleConsolidacion(int id, Producto producto)
    {
        if (producto is null)
            throw new ReglaDeNegocioException("Un detalle de consolidación debe tener un producto.");

        Id = id;
        Producto = producto;
    }

    internal AsignacionConsolidacion AgregarAsignacion(int id, DetalleRequisicion detalleOrigen, int cantidad)
    {
        var asignacion = new AsignacionConsolidacion(id, detalleOrigen, cantidad);
        _asignaciones.Add(asignacion);
        return asignacion;
    }
}
