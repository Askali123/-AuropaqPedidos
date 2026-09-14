using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-023 / docs/05-api.md §19.
public sealed class AgregarDetalleRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;
    private readonly IProductoRepository _productos;
    private readonly IGeneradorDeIdentificadores _ids;

    public AgregarDetalleRequisicionUseCase(
        IRequisicionRepository requisiciones, IProductoRepository productos, IGeneradorDeIdentificadores ids)
    {
        _requisiciones = requisiciones;
        _productos = productos;
        _ids = ids;
    }

    public RequisicionResponse Ejecutar(int requisicionId, AgregarDetalleRequisicionRequest request)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);

        var producto = _productos.ObtenerPorId(request.ProductoId)
            ?? throw new Excepciones.RecursoNoEncontradoException("El producto indicado no existe.");

        // docs/05-api.md §19 y docs/06-seguridad.md §41: "producto activo" no lo valida Domain
        // (DetalleRequisicion solo exige que el producto no sea null), por eso se valida aquí.
        if (!producto.Activo)
            throw new ReglaDeNegocioException("El producto no está activo y no puede agregarse a la requisición.");

        requisicion.AgregarDetalle(_ids.Siguiente(), producto, request.CantidadSolicitada, request.Observacion);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
