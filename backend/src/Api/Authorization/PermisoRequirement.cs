using Microsoft.AspNetCore.Authorization;

namespace AuropaqPedidos.Api.Authorization;

// TASK-016. Un único tipo de requirement, parametrizado por Permiso.Codigo (RN-055: identificador
// funcional global y único) — no una Policy estática por permiso.
public sealed class PermisoRequirement : IAuthorizationRequirement
{
    public string Codigo { get; }

    public PermisoRequirement(string codigo)
    {
        Codigo = codigo;
    }
}
