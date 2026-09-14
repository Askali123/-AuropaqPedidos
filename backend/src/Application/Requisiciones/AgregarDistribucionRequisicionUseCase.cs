using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-026, 02-dominio.md §15. La forma final de "distribuir por sede" definida en
// docs/05-api.md §22 es un PUT que reemplaza toda la distribución de un detalle en un solo
// llamado ("DistribuirDetalleRequest"); aquí se implementa la versión granular (agregar,
// modificar, eliminar una distribución a la vez) porque así se solicitó explícitamente para
// este bloque. Ver ambigüedad reportada sobre cómo reconciliar ambas formas en el bloque de Api.
public sealed class AgregarDistribucionRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;
    private readonly ISedeRepository _sedes;
    private readonly IGeneradorDeIdentificadores _ids;

    public AgregarDistribucionRequisicionUseCase(
        IRequisicionRepository requisiciones, ISedeRepository sedes, IGeneradorDeIdentificadores ids)
    {
        _requisiciones = requisiciones;
        _sedes = sedes;
        _ids = ids;
    }

    public Dtos.RequisicionResponse Ejecutar(int requisicionId, int detalleId, int sedeId, int cantidad)
    {
        var requisicion = RequisicionFinder.ObtenerOLanzar(_requisiciones, requisicionId);
        var detalle = RequisicionFinder.ObtenerDetalleOLanzar(requisicion, detalleId);

        var sede = _sedes.ObtenerPorId(sedeId)
            ?? throw new Excepciones.RecursoNoEncontradoException("La sede indicada no existe.");

        // docs/06-seguridad.md §42: "sede activa" no lo valida Domain (Requisicion solo exige
        // que la sede pertenezca a la empresa), por eso se valida aquí.
        if (!sede.Activo)
            throw new ReglaDeNegocioException("La sede no está activa.");

        requisicion.AgregarDistribucion(_ids.Siguiente(), detalle, sede, cantidad);

        _requisiciones.Guardar(requisicion);

        return RequisicionMapper.AResponse(requisicion);
    }
}
