using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
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

    public AuthController(LoginUseCase login)
    {
        _login = login;
    }

    [HttpPost("login")]
    public ActionResult<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var resultado = _login.Ejecutar(request, DateTime.UtcNow);
        return Ok(ApiResponse<LoginResponse>.De(resultado));
    }
}
