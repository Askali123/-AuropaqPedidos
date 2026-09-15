using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-015, 05-api.md §15. Sin validación de unicidad de Codigo: 04-base-datos.md §11 no la
// documenta (mismo criterio ya aplicado a Categoria.Nombre).
public sealed class CrearUnidadMedidaUseCase
{
    private readonly IUnidadMedidaRepository _unidadesMedida;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearUnidadMedidaUseCase(IUnidadMedidaRepository unidadesMedida, IGeneradorDeIdentificadores ids)
    {
        _unidadesMedida = unidadesMedida;
        _ids = ids;
    }

    public UnidadMedidaResponse Ejecutar(CrearUnidadMedidaRequest request)
    {
        var unidadMedida = new UnidadMedida(_ids.Siguiente(), request.Codigo, request.Nombre);

        _unidadesMedida.Guardar(unidadMedida);

        return new UnidadMedidaResponse(unidadMedida.Id, unidadMedida.Codigo, unidadMedida.Nombre, unidadMedida.Activo);
    }
}
