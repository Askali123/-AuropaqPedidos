using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IUsuarioRolRepository
{
    // TASK-010, 04-base-datos.md §9.3: "no se permiten relaciones duplicadas" (mismo criterio
    // ya usado para UsuarioSede) — se comprueba en Application antes de construir la relación.
    bool Existe(int usuarioId, int rolId);

    // Satisface el criterio de aceptación de TASK-012 (adelantado a esta tarea por instrucción
    // explícita del usuario): "un usuario puede tener uno o varios roles".
    IReadOnlyList<UsuarioRol> ObtenerPorUsuario(int usuarioId);

    void Guardar(UsuarioRol usuarioRol);
}
