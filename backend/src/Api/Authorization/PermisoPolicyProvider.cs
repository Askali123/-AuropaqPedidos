using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AuropaqPedidos.Api.Authorization;

// TASK-016 §7: evita registrar una Policy estática por cada Permiso.Codigo (cientos de policies
// duplicando el catálogo). Mecanismo estándar de ASP.NET Core (IAuthorizationPolicyProvider
// personalizado) — no un framework de autorización propio. Convención: cualquier nombre de
// policy con el prefijo "Permiso:" se resuelve dinámicamente como PermisoRequirement(código);
// cualquier otro nombre de policy se delega al proveedor por defecto.
public sealed class PermisoPolicyProvider : IAuthorizationPolicyProvider
{
    public const string Prefijo = "Permiso:";

    private readonly DefaultAuthorizationPolicyProvider _proveedorPorDefecto;

    public PermisoPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _proveedorPorDefecto = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _proveedorPorDefecto.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _proveedorPorDefecto.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Prefijo, StringComparison.Ordinal))
        {
            var codigo = policyName[Prefijo.Length..];

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermisoRequirement(codigo))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _proveedorPorDefecto.GetPolicyAsync(policyName);
    }
}
