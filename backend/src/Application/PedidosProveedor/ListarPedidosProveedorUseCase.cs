using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Dtos;

namespace AuropaqPedidos.Application.PedidosProveedor;

// I1-2 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): permite al Frontend listar
// pedidos (opcionalmente filtrados por consolidación) en vez de requerir un Id a mano. Sin
// alcance por empresa (CLAUDE.md §27): PedidoProveedor no pertenece a una única empresa.
public sealed class ListarPedidosProveedorUseCase
{
    private readonly IPedidoProveedorRepository _pedidos;

    public ListarPedidosProveedorUseCase(IPedidoProveedorRepository pedidos)
    {
        _pedidos = pedidos;
    }

    public IReadOnlyList<PedidoProveedorResponse> Ejecutar(int? consolidacionId) =>
        _pedidos.Listar(consolidacionId)
            .Select(PedidoProveedorMapper.AResponse)
            .ToList();
}
