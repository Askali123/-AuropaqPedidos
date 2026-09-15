using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-007, 05-api.md §12.3. Empresa no se puede reasignar desde este endpoint (RN-002); el
// request no tiene EmpresaId. Activo se aplica con Activar()/Desactivar() (mismo criterio que
// ActualizarEmpresaUseCase).
public sealed class ActualizarSedeUseCase
{
    private readonly ISedeRepository _sedes;

    public ActualizarSedeUseCase(ISedeRepository sedes)
    {
        _sedes = sedes;
    }

    public SedeResponse Ejecutar(int id, ActualizarSedeRequest request)
    {
        var sede = _sedes.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"La sede {id} no existe.");

        sede.ActualizarDatos(request.Nombre, request.Direccion, request.Ciudad, request.Departamento, request.Telefono, request.Contacto);

        if (request.Activo)
            sede.Activar();
        else
            sede.Desactivar();

        _sedes.Guardar(sede);

        return new SedeResponse(sede.Id, sede.Nombre, sede.Direccion, sede.Ciudad, sede.Departamento, sede.Telefono, sede.Contacto, sede.Activo);
    }
}
