using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.Organizacion.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// TASK: habilitar las consultas necesarias para el Frontend de Requisiciones (docs/05-api.md
// §54.6). Solo lectura: el Frontend consulta datos ya existentes, no los administra desde aquí
// (no hay CRUD de Empresa/Sede en esta tarea). El Controller solo traduce HTTP <-> caso de uso
// (CLAUDE.md §35/§40).
[ApiController]
[Route("api/v1/empresas")]
public sealed class EmpresasController : ControllerBase
{
    private readonly ListarEmpresasUseCase _listarEmpresas;
    private readonly ListarSedesPorEmpresaUseCase _listarSedes;

    public EmpresasController(ListarEmpresasUseCase listarEmpresas, ListarSedesPorEmpresaUseCase listarSedes)
    {
        _listarEmpresas = listarEmpresas;
        _listarSedes = listarSedes;
    }

    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<EmpresaResponse>>> Listar()
    {
        var resultado = _listarEmpresas.Ejecutar();
        return Ok(ApiResponse<IReadOnlyList<EmpresaResponse>>.De(resultado));
    }

    [HttpGet("{empresaId:int}/sedes")]
    public ActionResult<ApiResponse<IReadOnlyList<SedeResponse>>> ListarSedes(int empresaId)
    {
        var resultado = _listarSedes.Ejecutar(empresaId);
        return Ok(ApiResponse<IReadOnlyList<SedeResponse>>.De(resultado));
    }
}
