using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-010. Rol es GLOBAL: sin Empresa que validar (a diferencia de CrearUsuarioUseCase). Sin
// verificación de unicidad de Nombre: 04-base-datos.md §9.1 no documenta esa regla (mismo
// criterio ya aplicado a Empresa.Nit).
public sealed class CrearRolUseCase
{
    private readonly IRolRepository _roles;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearRolUseCase(IRolRepository roles, IGeneradorDeIdentificadores ids)
    {
        _roles = roles;
        _ids = ids;
    }

    public RolResponse Ejecutar(CrearRolRequest request)
    {
        var rol = new Rol(_ids.Siguiente(), request.Nombre, request.Descripcion);

        _roles.Guardar(rol);

        return new RolResponse(rol.Id, rol.Nombre, rol.Descripcion, rol.Activo);
    }
}
