using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IUsuarioSedeRepository
{
    // TASK-009, 04-base-datos.md §8: "No deben existir registros duplicados UsuarioId+SedeId" —
    // se comprueba en Application antes de construir la relación (mismo criterio ya usado para
    // Correo de Usuario y Año+Mes de Periodo).
    bool Existe(int usuarioId, int sedeId);

    // Satisface el criterio de aceptación de TASK-009: "el sistema debe poder determinar las
    // sedes autorizadas de un usuario".
    IReadOnlyList<UsuarioSede> ObtenerPorUsuario(int usuarioId);

    void Guardar(UsuarioSede usuarioSede);
}
