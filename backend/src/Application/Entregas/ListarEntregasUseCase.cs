using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Entregas.Dtos;

namespace AuropaqPedidos.Application.Entregas;

// I1-3 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): permite al Frontend listar las
// entregas de un pedido (reutiliza IEntregaRepository.ObtenerPorPedido, ya existente para
// TASK-043/045) en vez de requerir un Id de Entrega a mano.
public sealed class ListarEntregasUseCase
{
    private readonly IEntregaRepository _entregas;

    public ListarEntregasUseCase(IEntregaRepository entregas)
    {
        _entregas = entregas;
    }

    public IReadOnlyList<EntregaResponse> Ejecutar(int pedidoProveedorId) =>
        _entregas.ObtenerPorPedido(pedidoProveedorId)
            .Select(EntregaMapper.AResponse)
            .ToList();
}
