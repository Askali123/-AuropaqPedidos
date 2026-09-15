using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-006, 05-api.md §11.4. Activo se aplica con Activar()/Desactivar() (transición de estado
// explícita), no dentro de ActualizarDatos (mismo criterio que el resto del dominio).
public sealed class ActualizarEmpresaUseCase
{
    private readonly IEmpresaRepository _empresas;

    public ActualizarEmpresaUseCase(IEmpresaRepository empresas)
    {
        _empresas = empresas;
    }

    public EmpresaResponse Ejecutar(int id, ActualizarEmpresaRequest request)
    {
        var empresa = _empresas.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La empresa {id} no existe.");

        empresa.ActualizarDatos(request.Nombre, request.Nit);

        if (request.Activo)
            empresa.Activar();
        else
            empresa.Desactivar();

        _empresas.Guardar(empresa);

        return new EmpresaResponse(empresa.Id, empresa.Nombre, empresa.Activo);
    }
}
