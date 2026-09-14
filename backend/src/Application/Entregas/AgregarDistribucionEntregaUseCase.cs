using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Entregas;

// TASK-044, RN-035: registra a qué sede se destinó una cantidad entregada. La sede puede
// pertenecer a cualquier empresa (mismo criterio que DistribucionPedido).
public sealed class AgregarDistribucionEntregaUseCase
{
    private readonly IEntregaRepository _entregas;
    private readonly ISedeRepository _sedes;
    private readonly IGeneradorDeIdentificadores _ids;

    public AgregarDistribucionEntregaUseCase(IEntregaRepository entregas, ISedeRepository sedes, IGeneradorDeIdentificadores ids)
    {
        _entregas = entregas;
        _sedes = sedes;
        _ids = ids;
    }

    public EntregaResponse Ejecutar(int entregaId, int detalleId, int sedeId, int cantidad)
    {
        var entrega = EntregaFinder.ObtenerOLanzar(_entregas, entregaId);
        var detalle = EntregaFinder.ObtenerDetalleOLanzar(entrega, detalleId);

        var sede = _sedes.ObtenerPorId(sedeId)
            ?? throw new RecursoNoEncontradoException("La sede indicada no existe.");

        // Mismo criterio que "sede activa" en Requisiciones/PedidoProveedor.
        if (!sede.Activo)
            throw new ReglaDeNegocioException("La sede no está activa.");

        entrega.AgregarDistribucion(_ids.Siguiente(), detalle, sede, cantidad);

        _entregas.Guardar(entrega);

        return EntregaMapper.AResponse(entrega);
    }
}
