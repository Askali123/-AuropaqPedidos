using System.IdentityModel.Tokens.Jwt;
using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Api.Controllers.Dtos;
using AuropaqPedidos.Application.Requisiciones;
using AuropaqPedidos.Application.Requisiciones.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuropaqPedidos.Api.Controllers;

// docs/05-api.md §47 (API del MVP) + tarea actual: flujo de Requisición (crear/continuar,
// detalles, distribución, guardar, enviar, iniciar-revision, aprobar, devolver).
// El Controller solo traduce HTTP <-> caso de uso (CLAUDE.md §35/§40, 03-arquitectura.md §20):
// no contiene reglas de negocio, todas viven en Application/Domain.
[ApiController]
[Route("api/v1/requisiciones")]
public sealed class RequisicionesController : ControllerBase
{
    private readonly IniciarOContinuarRequisicionUseCase _iniciarOContinuar;
    private readonly AgregarDetalleRequisicionUseCase _agregarDetalle;
    private readonly ActualizarDetalleRequisicionUseCase _actualizarDetalle;
    private readonly EliminarDetalleRequisicionUseCase _eliminarDetalle;
    private readonly AgregarDistribucionRequisicionUseCase _agregarDistribucion;
    private readonly ModificarDistribucionRequisicionUseCase _modificarDistribucion;
    private readonly EliminarDistribucionRequisicionUseCase _eliminarDistribucion;
    private readonly GuardarBorradorRequisicionUseCase _guardarBorrador;
    private readonly EnviarRequisicionUseCase _enviar;
    private readonly IniciarRevisionRequisicionUseCase _iniciarRevision;
    private readonly AprobarRequisicionUseCase _aprobar;
    private readonly DevolverRequisicionUseCase _devolver;

    public RequisicionesController(
        IniciarOContinuarRequisicionUseCase iniciarOContinuar,
        AgregarDetalleRequisicionUseCase agregarDetalle,
        ActualizarDetalleRequisicionUseCase actualizarDetalle,
        EliminarDetalleRequisicionUseCase eliminarDetalle,
        AgregarDistribucionRequisicionUseCase agregarDistribucion,
        ModificarDistribucionRequisicionUseCase modificarDistribucion,
        EliminarDistribucionRequisicionUseCase eliminarDistribucion,
        GuardarBorradorRequisicionUseCase guardarBorrador,
        EnviarRequisicionUseCase enviar,
        IniciarRevisionRequisicionUseCase iniciarRevision,
        AprobarRequisicionUseCase aprobar,
        DevolverRequisicionUseCase devolver)
    {
        _iniciarOContinuar = iniciarOContinuar;
        _agregarDetalle = agregarDetalle;
        _actualizarDetalle = actualizarDetalle;
        _eliminarDetalle = eliminarDetalle;
        _agregarDistribucion = agregarDistribucion;
        _modificarDistribucion = modificarDistribucion;
        _eliminarDistribucion = eliminarDistribucion;
        _guardarBorrador = guardarBorrador;
        _enviar = enviar;
        _iniciarRevision = iniciarRevision;
        _aprobar = aprobar;
        _devolver = devolver;
    }

    // docs/05-api.md §18. "Crea o recupera": no se puede saber de antemano si el resultado es
    // un recurso nuevo o uno existente, así que se responde 200 en ambos casos (no 201).
    [HttpPost]
    public ActionResult<ApiResponse<RequisicionResponse>> IniciarOContinuar([FromBody] CrearRequisicionRequest request)
    {
        if (!TryObtenerIdentidad(out var usuarioId, out var empresaId, out var error))
            return error;

        var resultado = _iniciarOContinuar.Ejecutar(empresaId, usuarioId, request, DateTime.UtcNow);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §19.
    [HttpPost("{id:int}/detalles")]
    public ActionResult<ApiResponse<RequisicionResponse>> AgregarDetalle(int id, [FromBody] AgregarDetalleRequisicionRequest request)
    {
        var resultado = _agregarDetalle.Ejecutar(id, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §20.
    [HttpPut("{id:int}/detalles/{detalleId:int}")]
    public ActionResult<ApiResponse<RequisicionResponse>> ActualizarDetalle(
        int id, int detalleId, [FromBody] ActualizarDetalleRequisicionRequest request)
    {
        var resultado = _actualizarDetalle.Ejecutar(id, detalleId, request);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §21.
    [HttpDelete("{id:int}/detalles/{detalleId:int}")]
    public ActionResult<ApiResponse<RequisicionResponse>> EliminarDetalle(int id, int detalleId)
    {
        var resultado = _eliminarDetalle.Ejecutar(id, detalleId);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // Ver ambigüedad ya reportada (progreso.md, sesión Application): docs/05-api.md §22
    // documenta un único PUT que reemplaza toda la distribución de un detalle; Application
    // solo implementa operaciones granulares (agregar/modificar/eliminar una distribución a
    // la vez), así que se exponen como sub-recurso "distribuciones" en lugar de reutilizar
    // esa misma ruta con un contrato distinto al documentado. No se resuelve la contradicción
    // en este bloque (instrucción explícita de la tarea actual).
    [HttpPost("{id:int}/detalles/{detalleId:int}/distribuciones")]
    public ActionResult<ApiResponse<RequisicionResponse>> AgregarDistribucion(
        int id, int detalleId, [FromBody] AgregarDistribucionRequest request)
    {
        var resultado = _agregarDistribucion.Ejecutar(id, detalleId, request.SedeId, request.Cantidad);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RequisicionResponse>.De(resultado));
    }

    [HttpPut("{id:int}/detalles/{detalleId:int}/distribuciones/{distribucionId:int}")]
    public ActionResult<ApiResponse<RequisicionResponse>> ModificarDistribucion(
        int id, int detalleId, int distribucionId, [FromBody] ModificarDistribucionRequest request)
    {
        var resultado = _modificarDistribucion.Ejecutar(id, detalleId, distribucionId, request.NuevaCantidad);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    [HttpDelete("{id:int}/detalles/{detalleId:int}/distribuciones/{distribucionId:int}")]
    public ActionResult<ApiResponse<RequisicionResponse>> EliminarDistribucion(int id, int detalleId, int distribucionId)
    {
        var resultado = _eliminarDistribucion.Ejecutar(id, detalleId, distribucionId);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // TASK-028/RN-013: guardar no es enviar. No documentado como ruta propia en
    // docs/05-api.md (el documento no enumera un endpoint dedicado), se expone como acción
    // explícita siguiendo la misma convención que /enviar, /aprobar, /devolver (05-api.md §41).
    [HttpPost("{id:int}/guardar")]
    public ActionResult<ApiResponse<RequisicionResponse>> GuardarBorrador(int id)
    {
        var resultado = _guardarBorrador.Ejecutar(id);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §23. TASK-016: primer endpoint protegido por permiso (06-seguridad.md §52,
    // el único par endpoint/permiso con algún respaldo documental para una ruta que existe
    // realmente) — requiere autenticación (401 si falta/JWT inválido) + REQUISICION_ENVIAR
    // (403 si no lo tiene). TASK-050 agrega alcance por empresa (403 si la Requisicion no es de
    // la Empresa del usuario autenticado) y reemplaza X-Usuario-Id por el UsuarioId del JWT como
    // actor de negocio (decisión explícita del usuario; el propio EnviarRequisicionUseCase ya
    // decía desde TASK-029 que "usuarioId debe provenir de la identidad autenticada, no del
    // cliente" — ver ObtenerUsuarioIdAutenticado).
    [Authorize(Policy = "Permiso:REQUISICION_ENVIAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpPost("{id:int}/enviar")]
    public ActionResult<ApiResponse<RequisicionResponse>> Enviar(int id)
    {
        var resultado = _enviar.Ejecutar(id, ObtenerUsuarioIdAutenticado(), DateTime.UtcNow);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // Transición ENVIADA -> EN_REVISION (02-dominio.md §17, 04-base-datos.md §18). Sin ruta
    // documentada en docs/05-api.md (ambigüedad ya reportada: ningún RN-XXX define qué la
    // dispara); se expone como acción explícita porque la tarea actual la pide expresamente.
    [HttpPost("{id:int}/iniciar-revision")]
    public ActionResult<ApiResponse<RequisicionResponse>> IniciarRevision(int id)
    {
        if (!IdentidadTemporal.TryObtenerUsuarioId(Request, out var usuarioId))
            return HeaderUsuarioIdFaltante();

        var resultado = _iniciarRevision.Ejecutar(id, usuarioId, DateTime.UtcNow);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §24.2. TASK-016: segundo endpoint protegido por permiso; TASK-050: alcance
    // por empresa + actor desde JWT — ver nota junto a Enviar.
    [Authorize(Policy = "Permiso:REQUISICION_APROBAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpPost("{id:int}/aprobar")]
    public ActionResult<ApiResponse<RequisicionResponse>> Aprobar(int id, [FromBody] AprobarRequisicionRequest request)
    {
        var resultado = _aprobar.Ejecutar(id, ObtenerUsuarioIdAutenticado(), DateTime.UtcNow, request);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §25.
    [HttpPost("{id:int}/devolver")]
    public ActionResult<ApiResponse<RequisicionResponse>> Devolver(int id, [FromBody] DevolverRequisicionRequest request)
    {
        if (!IdentidadTemporal.TryObtenerUsuarioId(Request, out var usuarioId))
            return HeaderUsuarioIdFaltante();

        var resultado = _devolver.Ejecutar(id, usuarioId, DateTime.UtcNow, request);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    private bool TryObtenerIdentidad(out int usuarioId, out int empresaId, out ActionResult error)
    {
        if (!IdentidadTemporal.TryObtenerUsuarioId(Request, out usuarioId))
        {
            empresaId = 0;
            error = HeaderFaltante("X-Usuario-Id");
            return false;
        }

        if (!IdentidadTemporal.TryObtenerEmpresaId(Request, out empresaId))
        {
            error = HeaderFaltante("X-Empresa-Id");
            return false;
        }

        error = null!;
        return true;
    }

    // TASK-050. Solo se usa en endpoints con [Authorize]: para cuando esta línea se ejecuta, la
    // autorización (Permiso + AlcanceRequisicion) ya exigió con éxito un claim "sub" válido —
    // PermisoAuthorizationHandler/AlcanceRequisicionAuthorizationHandler ya lo parsearon para
    // llegar hasta aquí, así que el parseo no puede fallar en la práctica.
    private int ObtenerUsuarioIdAutenticado() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    private static ActionResult HeaderUsuarioIdFaltante() => HeaderFaltante("X-Usuario-Id");

    private static ActionResult HeaderFaltante(string nombreHeader) => new BadRequestObjectResult(
        new ErrorResponse(new ErrorDetail(
            "SOLICITUD_INVALIDA",
            $"El header '{nombreHeader}' es obligatorio mientras no exista autenticación real (placeholder temporal).",
            Array.Empty<string>())));
}
