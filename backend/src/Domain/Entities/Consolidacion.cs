using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-027/RN-028/RN-029, 02-dominio.md §18, 04-base-datos.md §23: agrupa necesidades de
// requisiciones APROBADAS por producto, para determinar cuánto se necesita comprar. No
// modifica las requisiciones originales (RN-028) y conserva trazabilidad hacia cada
// DetalleRequisicion que aportó cantidad (RN-029). Es el agregado raíz de
// DetalleConsolidacion/AsignacionConsolidacion, igual que Requisicion lo es de
// DetalleRequisicion/DistribucionRequisicion: la regla "no mezclar productos diferentes"
// cruza varios detalles a la vez y solo Consolidacion puede garantizarla de forma consistente.
public sealed class Consolidacion
{
    public int Id { get; }
    public Periodo Periodo { get; }

    // UsuarioCreacionId queda como identificador simple porque Usuario todavía no existe en
    // Domain (mismo criterio ya usado en Requisicion/SolicitudProductoCatalogo).
    public int UsuarioCreacionId { get; }

    // Texto libre, sin enum: 04-base-datos.md §23 documenta la columna Estado, pero no define
    // valores para Consolidacion (a diferencia de RequisicionEstado, que sí los enumera). Mismo
    // criterio ya usado para Periodo.Estado: no se inventan valores no documentados.
    public string Estado { get; }

    public DateTime FechaCreacion { get; }
    public string? Observacion { get; }

    private readonly List<DetalleConsolidacion> _detalles = new();
    public IReadOnlyList<DetalleConsolidacion> Detalles => _detalles;

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private Consolidacion()
    {
    }
#pragma warning restore CS8618

    public Consolidacion(int id, Periodo periodo, int usuarioCreacionId, string estado, DateTime fechaCreacion, string? observacion = null)
    {
        if (periodo is null)
            throw new ReglaDeNegocioException("Una consolidación debe pertenecer a un periodo.");

        if (string.IsNullOrWhiteSpace(estado))
            throw new ReglaDeNegocioException("El estado de la consolidación es obligatorio.");

        Id = id;
        Periodo = periodo;
        UsuarioCreacionId = usuarioCreacionId;
        Estado = estado;
        FechaCreacion = fechaCreacion;
        Observacion = observacion;
    }

    // RN-027: solo requisiciones APROBADAS pueden participar en la consolidación.
    // RN-028: requisicionOrigen/detalleOrigen nunca se modifican aquí, solo se referencian.
    // RN-029: la asignación conserva la trazabilidad hacia el detalle de requisición original.
    // "No mezclar productos diferentes": se agrupa por Producto, reutilizando el
    // DetalleConsolidacion existente para ese producto si ya existe uno en esta consolidación.
    public void AgregarAsignacion(
        int detalleConsolidacionId, int asignacionId, Requisicion requisicionOrigen, DetalleRequisicion detalleOrigen, int cantidad)
    {
        if (requisicionOrigen is null)
            throw new ReglaDeNegocioException("Debe indicarse la requisición de origen.");

        if (requisicionOrigen.Estado != RequisicionEstado.Aprobada)
            throw new ReglaDeNegocioException("Solo pueden consolidarse detalles de requisiciones aprobadas.");

        if (detalleOrigen is null || !requisicionOrigen.Detalles.Contains(detalleOrigen))
            throw new ReglaDeNegocioException("El detalle de requisición no pertenece a la requisición de origen indicada.");

        var detalle = _detalles.FirstOrDefault(d => d.Producto == detalleOrigen.Producto);
        if (detalle is null)
        {
            detalle = new DetalleConsolidacion(detalleConsolidacionId, detalleOrigen.Producto);
            _detalles.Add(detalle);
        }

        detalle.AgregarAsignacion(asignacionId, detalleOrigen, cantidad);
    }
}
