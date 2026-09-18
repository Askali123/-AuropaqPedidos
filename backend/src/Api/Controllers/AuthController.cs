using System.IdentityModel.Tokens.Jwt;
using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-015. Ruta acordada explícitamente con el usuario (no documentada previamente en
// 05-api.md): POST /api/v1/auth/login, separada de /usuarios (recurso de dominio) porque
// representa una acción de sesión, no un recurso CRUD. El Controller solo traduce HTTP <->
// caso de uso (CLAUDE.md §35/§40), mismo patrón que el resto de los controllers.
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginUseCase _login;
    private readonly ObtenerMisPermisosUseCase _misPermisos;

    public AuthController(LoginUseCase login, ObtenerMisPermisosUseCase misPermisos)
    {
        _login = login;
        _misPermisos = misPermisos;
    }

    // [AllowAnonymous] agregado 2026-09-17 (P1-5, docs/2026-09-17-tareas.md): necesario desde que
    // Program.cs define un FallbackPolicy que exige autenticación por defecto en cualquier
    // endpoint sin [Authorize]/[AllowAnonymous] explícito — sin esto, nadie podría autenticarse.
    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var resultado = _login.Ejecutar(request, DateTime.UtcNow);
        return Ok(ApiResponse<LoginResponse>.De(resultado));
    }

    // TASK-101 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, Decisión A). Autoconsulta:
    // solo exige estar autenticado (sin política de permiso específico) — es la única forma de
    // que el propio usuario sepa qué puede hacer sin depender de SEGURIDAD_VER (que un
    // Solicitante no tiene). Vive en AuthController, no en UsuariosController, porque representa
    // "mi sesión", igual que /login — no un recurso administrado sobre otro usuario.
    [Authorize]
    [HttpGet("mis-permisos")]
    public ActionResult<ApiResponse<IReadOnlyList<PermisoResponse>>> MisPermisos()
    {
        var resultado = _misPermisos.Ejecutar(ObtenerUsuarioIdAutenticado());
        return Ok(ApiResponse<IReadOnlyList<PermisoResponse>>.De(resultado));
    }

    private int ObtenerUsuarioIdAutenticado() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
