using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.PedidosProveedor;

// TASK-041, RN-032: define qué cantidad del detalle del pedido corresponde a cada sede
// (de cualquier empresa — un PedidoProveedor no pertenece a una sola empresa).
//
// RN-065/D-18 (2026-09-17): la regla "suma = cantidad pedida" (equivalente a RN-011 para
// Requisición) ya está decidida e implementada — pero como guarda en
// PedidoProveedor.Enviar() (Domain), no aquí: agregar una distribución parcial sigue siendo
// válido en cualquier momento, solo Enviar() exige que la suma esté completa.
public sealed class AgregarDistribucionPedidoUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly ISedeRepository _sedes;
    private readonly IGeneradorDeIdentificadores _ids;

    public AgregarDistribucionPedidoUseCase(IPedidoProveedorRepository pedidos, ISedeRepository sedes, IGeneradorDeIdentificadores ids)
    {
        _pedidos = pedidos;
        _sedes = sedes;
        _ids = ids;
    }

    public PedidoProveedorResponse Ejecutar(int pedidoId, int detalleId, int sedeId, int cantidad)
    {
        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, pedidoId);
        var detalle = PedidoProveedorFinder.ObtenerDetalleOLanzar(pedido, detalleId);

        var sede = _sedes.ObtenerPorId(sedeId)
            ?? throw new RecursoNoEncontradoException("La sede indicada no existe.");

        // Mismo criterio que "sede activa" en Requisiciones (06-seguridad.md §42): Domain solo
        // exige que la sede no sea null, "activa" se valida aquí.
        if (!sede.Activo)
            throw new ReglaDeNegocioException("La sede no está activa.");

        pedido.AgregarDistribucion(_ids.Siguiente(), detalle, sede, cantidad);

        _pedidos.Guardar(pedido);

        return PedidoProveedorMapper.AResponse(pedido);
    }
}
