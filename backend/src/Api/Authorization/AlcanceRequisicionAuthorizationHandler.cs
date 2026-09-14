using System.IdentityModel.Tokens.Jwt;
using AuropaqPedidos.Application.Requisiciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AuropaqPedidos.Api.Authorization;

// TASK-050. Autorización basada en recurso (patrón estándar de ASP.NET Core con enrutamiento de
// endpoints): con routing de endpoints (el usado por este proyecto, ASP.NET Core 10 minimal
// hosting), el AuthorizeMiddleware pasa el propio HttpContext como "Resource" — no el
// AuthorizationFilterContext de la pipeline clásica de MVC filters (verificado empíricamente:
// GetType().FullName = "Microsoft.AspNetCore.Http.DefaultHttpContext"). De ahí se lee el "id" de
// la ruta (p. ej. /requisiciones/{id}/enviar) vía HttpContext.Request.RouteValues. Delega en
// UsuarioTieneAlcanceSobreRequisicionUseCase (Application) — el Handler nunca consulta
// SQL/DbContext directamente (mismo criterio que PermisoAuthorizationHandler, TASK-016 §17/§19).
public sealed class AlcanceRequisicionAuthorizationHandler : AuthorizationHandler<AlcanceRequisicionRequirement>
{
    private readonly UsuarioTieneAlcanceSobreRequisicionUseCase _alcance;

    public AlcanceRequisicionAuthorizationHandler(UsuarioTieneAlcanceSobreRequisicionUseCase alcance)
    {
        _alcance = alcance;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlcanceRequisicionRequirement requirement)
    {
        var claimUsuarioId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (claimUsuarioId is null || !int.TryParse(claimUsuarioId, out var usuarioId))
            return Task.CompletedTask;

        if (context.Resource is not HttpContext httpContext)
            return Task.CompletedTask;

        if (!httpContext.Request.RouteValues.TryGetValue("id", out var idValor)
            || !int.TryParse(idValor?.ToString(), out var requisicionId))
        {
            return Task.CompletedTask;
        }

        if (_alcance.Ejecutar(usuarioId, requisicionId))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
