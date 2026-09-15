using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-015, 05-api.md §15.
public sealed class ObtenerUnidadMedidaUseCase
{
    private readonly IUnidadMedidaRepository _unidadesMedida;

    public ObtenerUnidadMedidaUseCase(IUnidadMedidaRepository unidadesMedida)
    {
        _unidadesMedida = unidadesMedida;
    }

    public UnidadMedidaResponse Ejecutar(int id)
    {
        var unidadMedida = _unidadesMedida.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La unidad de medida {id} no existe.");

        return new UnidadMedidaResponse(unidadMedida.Id, unidadMedida.Codigo, unidadMedida.Nombre, unidadMedida.Activo);
    }
}
