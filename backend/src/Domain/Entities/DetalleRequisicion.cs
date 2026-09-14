using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-009: un producto específico solicitado dentro de una requisición.
// Solo se crea/modifica a través de Requisicion (constructor y mutadores internal),
// que es quien controla el estado (editable/no editable) de todo el agregado.
public sealed class DetalleRequisicion
{
    public int Id { get; }
    public Producto Producto { get; }
    public int CantidadSolicitada { get; private set; }
    public string? Observacion { get; private set; }

    private readonly List<DistribucionRequisicion> _distribuciones = new();
    public IReadOnlyList<DistribucionRequisicion> Distribuciones => _distribuciones;

    public int CantidadDistribuida => _distribuciones.Sum(d => d.Cantidad);

    // RN-011: SUM(distribuciones) debe coincidir con la cantidad solicitada antes de enviar.
    public bool DistribucionCompleta => CantidadDistribuida == CantidadSolicitada;

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private DetalleRequisicion()
    {
    }
#pragma warning restore CS8618

    internal DetalleRequisicion(int id, Producto producto, int cantidadSolicitada, string? observacion)
    {
        if (producto is null)
            throw new ReglaDeNegocioException("Un detalle de requisición debe tener un producto.");

        if (cantidadSolicitada <= 0)
            throw new ReglaDeNegocioException("La cantidad solicitada debe ser mayor que cero.");

        Id = id;
        Producto = producto;
        CantidadSolicitada = cantidadSolicitada;
        Observacion = observacion;
    }

    internal void ModificarCantidad(int nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
            throw new ReglaDeNegocioException("La cantidad solicitada debe ser mayor que cero.");

        if (nuevaCantidad < CantidadDistribuida)
            throw new ReglaDeNegocioException("La cantidad no puede ser menor que lo ya distribuido entre sedes.");

        CantidadSolicitada = nuevaCantidad;
    }

    internal void ModificarObservacion(string? observacion) => Observacion = observacion;

    // RN-011: también es inválido distribuir más de la cantidad solicitada.
    internal DistribucionRequisicion AgregarDistribucion(int id, Sede sede, int cantidad)
    {
        if (CantidadDistribuida + cantidad > CantidadSolicitada)
            throw new ReglaDeNegocioException("La suma distribuida no puede superar la cantidad solicitada.");

        var distribucion = new DistribucionRequisicion(id, sede, cantidad);
        _distribuciones.Add(distribucion);
        return distribucion;
    }

    internal void ModificarDistribucion(DistribucionRequisicion distribucion, int nuevaCantidad)
    {
        if (!_distribuciones.Contains(distribucion))
            throw new ReglaDeNegocioException("La distribución no pertenece a este detalle.");

        var sumaSinEsta = CantidadDistribuida - distribucion.Cantidad;
        if (sumaSinEsta + nuevaCantidad > CantidadSolicitada)
            throw new ReglaDeNegocioException("La suma distribuida no puede superar la cantidad solicitada.");

        distribucion.ModificarCantidad(nuevaCantidad);
    }

    internal void EliminarDistribucion(DistribucionRequisicion distribucion)
    {
        _distribuciones.Remove(distribucion);
    }
}
