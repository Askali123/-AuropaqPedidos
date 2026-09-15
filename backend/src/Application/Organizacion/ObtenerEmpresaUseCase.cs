using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-006, 05-api.md §11.2.
public sealed class ObtenerEmpresaUseCase
{
    private readonly IEmpresaRepository _empresas;

    public ObtenerEmpresaUseCase(IEmpresaRepository empresas)
    {
        _empresas = empresas;
    }

    public EmpresaResponse Ejecutar(int id)
    {
        var empresa = _empresas.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La empresa {id} no existe.");

        return new EmpresaResponse(empresa.Id, empresa.Nombre, empresa.Activo);
    }
}
