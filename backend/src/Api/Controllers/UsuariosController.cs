using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-008. Sin headers de identidad ni autenticación (fuera de alcance de esta tarea, Fase 10 /
// TASK-048). El Controller solo traduce HTTP <-> caso de uso (CLAUDE.md §35/§40), mismo patrón
// que PeriodosController.
[ApiController]
[Route("api/v1/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly CrearUsuarioUseCase _crearUsuario;
    private readonly ListarUsuariosUseCase _listarUsuarios;

    public UsuariosController(CrearUsuarioUseCase crearUsuario, ListarUsuariosUseCase listarUsuarios)
    {
        _crearUsuario = crearUsuario;
        _listarUsuarios = listarUsuarios;
    }

    [HttpPost]
    public ActionResult<ApiResponse<UsuarioResponse>> Crear([FromBody] CrearUsuarioRequest request)
    {
        var resultado = _crearUsuario.Ejecutar(request, DateTime.UtcNow);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UsuarioResponse>.De(resultado));
    }

    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<UsuarioResponse>>> Listar()
    {
        var resultado = _listarUsuarios.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<UsuarioResponse>>.De(resultado));
    }
}
