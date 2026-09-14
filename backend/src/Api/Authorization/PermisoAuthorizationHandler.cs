using System.IdentityModel.Tokens.Jwt;
using AuropaqPedidos.Application.Organizacion;
using Microsoft.AspNetCore.Authorization;

namespace AuropaqPedidos.Api.Authorization;

// TASK-016. Lee el UsuarioId del claim "sub" del JWT (mismo claim emitido por JwtTokenGenerator,
// TASK-015) y delega en UsuarioTienePermisoUseCase (Application) — el Handler nunca consulta
// SQL/DbContext directamente (TASK-016 §17: Api -> Application + Infrastructure, no Api -> BD).
// Requiere `JwtBearerOptions.MapInboundClaims = false` (configurado en Program.cs) para que el
// claim llegue exactamente como "sub" y no remapeado a la URI larga que ASP.NET Core usa por
// defecto para claims JWT cortos.
public sealed class PermisoAuthorizationHandler : AuthorizationHandler<PermisoRequirement>
{
    private readonly UsuarioTienePermisoUseCase _usuarioTienePermiso;

    public PermisoAuthorizationHandler(UsuarioTienePermisoUseCase usuarioTienePermiso)
    {
        _usuarioTienePermiso = usuarioTienePermiso;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermisoRequirement requirement)
    {
        var claimUsuarioId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (claimUsuarioId is not null
            && int.TryParse(claimUsuarioId, out var usuarioId)
            && _usuarioTienePermiso.Ejecutar(usuarioId, requirement.Codigo))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
