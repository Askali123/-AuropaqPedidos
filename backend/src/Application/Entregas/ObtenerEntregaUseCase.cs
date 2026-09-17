using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.Excepciones;

namespace AuropaqPedidos.Application.Entregas;

// I1-3 (docs/incremento-fase-6-9-frontend-2026-09-17-1028.md): consultar el detalle de una
// entrega (detalles, distribuciones, estado).
public sealed class ObtenerEntregaUseCase
{
    private readonly IEntregaRepository _entregas;

    public ObtenerEntregaUseCase(IEntregaRepository entregas)
    {
        _entregas = entregas;
    }

    public EntregaResponse Ejecutar(int id)
    {
        var entrega = _entregas.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La entrega {id} no existe.");

        return EntregaMapper.AResponse(entrega);
    }
}
