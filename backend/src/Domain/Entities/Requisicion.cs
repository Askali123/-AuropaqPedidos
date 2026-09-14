using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-007..RN-020, RN-036/RN-037: ciclo de vida de la requisición mensual (Empresa + Periodo).
// Es el agregado raíz de Detalle/DistribucionRequisicion: ambos solo se crean y mutan a través
// de esta clase, porque las reglas de negocio (RN-011, editabilidad por estado) cruzan varios
// de esos objetos a la vez y solo Requisicion puede garantizarlas de forma consistente.
public sealed class Requisicion
{
    public int Id { get; }
    public Empresa Empresa { get; }
    public Periodo Periodo { get; }

    // UsuarioCreacionId queda como identificador simple porque Usuario todavía no existe en Domain.
    public int UsuarioCreacionId { get; }

    public RequisicionEstado Estado { get; private set; }
    public DateTime FechaCreacion { get; }
    public DateTime? FechaEnvio { get; private set; }

    private readonly List<DetalleRequisicion> _detalles = new();
    public IReadOnlyList<DetalleRequisicion> Detalles => _detalles;

    private readonly List<HistorialRequisicion> _historial = new();
    public IReadOnlyList<HistorialRequisicion> Historial => _historial;

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private Requisicion()
    {
    }
#pragma warning restore CS8618

    public Requisicion(int id, Empresa empresa, Periodo periodo, int usuarioCreacionId, DateTime fechaCreacion)
    {
        if (empresa is null)
            throw new ReglaDeNegocioException("Una requisición debe pertenecer a una empresa.");

        if (periodo is null)
            throw new ReglaDeNegocioException("Una requisición debe pertenecer a un periodo.");

        Id = id;
        Empresa = empresa;
        Periodo = periodo;
        UsuarioCreacionId = usuarioCreacionId;
        FechaCreacion = fechaCreacion;
        Estado = RequisicionEstado.Borrador;
    }

    // RN-012/RN-014/RN-020: BORRADOR y DEVUELTA son los únicos estados en los que puede editarse.
    public bool EsEditable => Estado is RequisicionEstado.Borrador or RequisicionEstado.Devuelta;

    // TASK-023/RN-009: agregar un producto mientras la requisición esté editable.
    public DetalleRequisicion AgregarDetalle(int id, Producto producto, int cantidadSolicitada, string? observacion = null)
    {
        AsegurarEditable();

        var detalle = new DetalleRequisicion(id, producto, cantidadSolicitada, observacion);
        _detalles.Add(detalle);
        return detalle;
    }

    // TASK-024
    public void ModificarCantidadDetalle(DetalleRequisicion detalle, int nuevaCantidad)
    {
        AsegurarEditable();
        AsegurarPerteneceADetalles(detalle);

        detalle.ModificarCantidad(nuevaCantidad);
    }

    // TASK-024
    public void ModificarObservacionDetalle(DetalleRequisicion detalle, string? observacion)
    {
        AsegurarEditable();
        AsegurarPerteneceADetalles(detalle);

        detalle.ModificarObservacion(observacion);
    }

    // TASK-025: solo se permite mientras la requisición está editable (nunca sobre un proceso cerrado/histórico).
    public void EliminarDetalle(DetalleRequisicion detalle)
    {
        AsegurarEditable();
        AsegurarPerteneceADetalles(detalle);

        _detalles.Remove(detalle);
    }

    // TASK-026, 02-dominio.md §15: la sede utilizada debe pertenecer a la empresa de la requisición.
    public DistribucionRequisicion AgregarDistribucion(int id, DetalleRequisicion detalle, Sede sede, int cantidad)
    {
        AsegurarEditable();
        AsegurarPerteneceADetalles(detalle);
        AsegurarSedeDeLaEmpresa(sede);

        return detalle.AgregarDistribucion(id, sede, cantidad);
    }

    public void ModificarDistribucion(DetalleRequisicion detalle, DistribucionRequisicion distribucion, int nuevaCantidad)
    {
        AsegurarEditable();
        AsegurarPerteneceADetalles(detalle);

        detalle.ModificarDistribucion(distribucion, nuevaCantidad);
    }

    public void EliminarDistribucion(DetalleRequisicion detalle, DistribucionRequisicion distribucion)
    {
        AsegurarEditable();
        AsegurarPerteneceADetalles(detalle);

        detalle.EliminarDistribucion(distribucion);
    }

    // TASK-029/TASK-035, RN-012..RN-015/RN-020, 04-base-datos.md §21.
    // El envío inicial (BORRADOR) y el reenvío tras corrección (DEVUELTA) exigen exactamente
    // las mismas validaciones documentadas, por lo que comparten esta única implementación.
    public void Enviar(int usuarioId, DateTime fechaEnvio)
    {
        AsegurarEditable();

        if (_detalles.Count == 0)
            throw new ReglaDeNegocioException("La requisición debe tener al menos un detalle para enviarse.");

        if (!Empresa.Activo)
            throw new ReglaDeNegocioException("La empresa de la requisición no está activa.");

        if (!Periodo.EstaDentroDeVentanaDeSolicitud(fechaEnvio))
            throw new ReglaDeNegocioException("La requisición solo puede enviarse dentro de la ventana de solicitud del periodo.");

        if (_detalles.Any(d => !d.DistribucionCompleta))
            throw new ReglaDeNegocioException("Todos los detalles deben tener su cantidad completamente distribuida entre sedes antes de enviar.");

        var estadoAnterior = Estado;
        Estado = RequisicionEstado.Enviada;
        FechaEnvio = fechaEnvio;

        RegistrarHistorial(estadoAnterior, Estado, usuarioId, fechaEnvio, comentario: null);
    }

    // Transición dibujada en 02-dominio.md §17 y 04-base-datos.md §18 (ENVIADA -> EN_REVISION).
    // Ningún RN-XXX ni TASK-XXX define la acción/rol que la dispara; ver ambigüedad reportada.
    public void IniciarRevision(int usuarioId, DateTime fecha)
    {
        if (Estado != RequisicionEstado.Enviada)
            throw new ReglaDeNegocioException("Solo una requisición enviada puede pasar a revisión.");

        var estadoAnterior = Estado;
        Estado = RequisicionEstado.EnRevision;

        RegistrarHistorial(estadoAnterior, Estado, usuarioId, fecha, comentario: null);
    }

    // TASK-033/RN-017
    public void Aprobar(int usuarioId, DateTime fecha, string? comentario = null)
    {
        if (Estado != RequisicionEstado.EnRevision)
            throw new ReglaDeNegocioException("Solo una requisición en revisión puede aprobarse.");

        var estadoAnterior = Estado;
        Estado = RequisicionEstado.Aprobada;

        RegistrarHistorial(estadoAnterior, Estado, usuarioId, fecha, comentario);
    }

    // TASK-034/RN-018: la devolución debe registrar como mínimo el motivo.
    public void Devolver(int usuarioId, DateTime fecha, string motivo)
    {
        if (Estado != RequisicionEstado.EnRevision)
            throw new ReglaDeNegocioException("Solo una requisición en revisión puede devolverse.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ReglaDeNegocioException("Devolver una requisición requiere un motivo.");

        var estadoAnterior = Estado;
        Estado = RequisicionEstado.Devuelta;

        RegistrarHistorial(estadoAnterior, Estado, usuarioId, fecha, motivo);
    }

    private void AsegurarEditable()
    {
        if (!EsEditable)
            throw new ReglaDeNegocioException("La requisición no puede modificarse en su estado actual.");
    }

    private void AsegurarPerteneceADetalles(DetalleRequisicion detalle)
    {
        if (detalle is null || !_detalles.Contains(detalle))
            throw new ReglaDeNegocioException("El detalle no pertenece a esta requisición.");
    }

    private void AsegurarSedeDeLaEmpresa(Sede sede)
    {
        if (sede is null || sede.Empresa != Empresa)
            throw new ReglaDeNegocioException("La sede debe pertenecer a la misma empresa de la requisición.");
    }

    private void RegistrarHistorial(RequisicionEstado anterior, RequisicionEstado nuevo, int usuarioId, DateTime fecha, string? comentario)
    {
        _historial.Add(new HistorialRequisicion(anterior, nuevo, usuarioId, fecha, comentario));
    }
}
