using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;

namespace AuropaqPedidos.Application.Requisiciones;

// TASK-032, 05-api.md §17.2/§54.6 punto 2. Sin alcance por empresa aquí (a diferencia de Listar):
// no hay una regla documentada que impida abrir una requisición por id desde otra empresa/rol de
// revisión (05-api.md §54.6 lo deja explícitamente abierto: "la futura bandeja de revisión de
// otro rol/empresa") — no se inventa una restricción que la documentación no exige.
public sealed class ObtenerRequisicionUseCase
{
    private readonly IRequisicionRepository _requisiciones;

    public ObtenerRequisicionUseCase(IRequisicionRepository requisiciones)
    {
        _requisiciones = requisiciones;
    }

    public RequisicionResponse Ejecutar(int id)
    {
        var requisicion = _requisiciones.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La requisición {id} no existe.");

        return RequisicionMapper.AResponse(requisicion);
    }
}
