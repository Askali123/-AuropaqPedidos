using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-015, 05-api.md §15. Activo se aplica con Activar()/Desactivar().
public sealed class ActualizarUnidadMedidaUseCase
{
    private readonly IUnidadMedidaRepository _unidadesMedida;

    public ActualizarUnidadMedidaUseCase(IUnidadMedidaRepository unidadesMedida)
    {
        _unidadesMedida = unidadesMedida;
    }

    public UnidadMedidaResponse Ejecutar(int id, ActualizarUnidadMedidaRequest request)
    {
        var unidadMedida = _unidadesMedida.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La unidad de medida {id} no existe.");

        unidadMedida.ActualizarDatos(request.Codigo, request.Nombre);

        if (request.Activo)
            unidadMedida.Activar();
        else
            unidadMedida.Desactivar();

        _unidadesMedida.Guardar(unidadMedida);

        return new UnidadMedidaResponse(unidadMedida.Id, unidadMedida.Codigo, unidadMedida.Nombre, unidadMedida.Activo);
    }
}
