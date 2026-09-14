using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IRolPermisoRepository
{
    // TASK-013, 04-base-datos.md §9.4: "no deben existir registros duplicados" (mismo criterio
    // ya usado para UsuarioRol/UsuarioSede) — se comprueba en Application antes de construir la
    // relación.
    bool Existe(int rolId, int permisoId);

    IReadOnlyList<RolPermiso> ObtenerPorRol(int rolId);

    void Guardar(RolPermiso rolPermiso);
}
