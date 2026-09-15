using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK-007, 05-api.md §12.3: PUT /api/v1/sedes/{id} vive en su propia ruta (no anidada bajo
// empresas/{empresaId}), igual que el contrato documentado. El Controller solo traduce HTTP <->
// caso de uso (CLAUDE.md §35/§40). RN-059/060 (punto 8): ORGANIZACION_ADMINISTRAR.
[ApiController]
[Route("api/v1/sedes")]
public sealed class SedesController : ControllerBase
{
    private readonly ActualizarSedeUseCase _actualizarSede;

    public SedesController(ActualizarSedeUseCase actualizarSede)
    {
        _actualizarSede = actualizarSede;
    }

    [Authorize(Policy = "Permiso:ORGANIZACION_ADMINISTRAR")]
    [HttpPut("{id:int}")]
    public ActionResult<ApiResponse<SedeResponse>> Actualizar(int id, [FromBody] ActualizarSedeRequest request)
    {
        var resultado = _actualizarSede.Ejecutar(id, request);
        return Ok(ApiResponse<SedeResponse>.De(resultado));
    }
}
