using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-011, endurecido por RN-055/ADR-056 (hardening previo a TASK-013). Permiso es GLOBAL: sin
// Empresa que validar (mismo criterio que CrearRolUseCase). Codigo duplicado -> 422 (mismo
// criterio ya usado para Usuario.Correo y Periodo Año+Mes) — la restricción UNIQUE de
// PermisoConfiguration es la segunda barrera, no la única.
public sealed class CrearPermisoUseCase
{
    private readonly IPermisoRepository _permisos;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearPermisoUseCase(IPermisoRepository permisos, IGeneradorDeIdentificadores ids)
    {
        _permisos = permisos;
        _ids = ids;
    }

    public PermisoResponse Ejecutar(CrearPermisoRequest request)
    {
        if (_permisos.ExisteParaCodigo(request.Codigo))
            throw new ReglaDeNegocioException("Ya existe un permiso con ese código.");

        var permiso = new Permiso(_ids.Siguiente(), request.Codigo, request.Nombre, request.Descripcion);

        _permisos.Guardar(permiso);

        return new PermisoResponse(permiso.Id, permiso.Codigo, permiso.Nombre, permiso.Descripcion);
    }
}
