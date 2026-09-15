using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuropaqPedidos.Api.Common;

// TASK-055/056 (Logging/Auditoría, punto 8.1 — 2026-09-15): PedidosProveedorController y
// EntregasController todavía no exigen JWT (autorización de Fase 6-9 sigue pendiente, decisión
// explícita del usuario 2026-09-15). Aun así, si el llamador incluye un Bearer token válido, el
// middleware de autenticación (UseAuthentication, registrado globalmente en Program.cs) ya lo
// valida y puebla HttpContext.User — con o sin [Authorize] en el endpoint. Este helper solo lee
// ese dato si está disponible, para que Auditoria/logging registren el actor real cuando exista,
// sin convertir la ausencia de JWT en un error 401 (eso requeriría [Authorize], fuera de alcance
// de esta tarea).
internal static class IdentidadOpcional
{
    public static int? ObtenerUsuarioIdSiAutenticado(ClaimsPrincipal usuario)
    {
        var claim = usuario.FindFirst(JwtRegisteredClaimNames.Sub);
        return claim is not null && int.TryParse(claim.Value, out var usuarioId) ? usuarioId : null;
    }
}
