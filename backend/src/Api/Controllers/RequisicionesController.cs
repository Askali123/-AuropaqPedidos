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
//
// RN-059/060 (punto 8, 2026-09-15): autorización real (permiso + alcance por empresa) en TODAS
// las rutas de este Controller — antes solo Enviar/Aprobar la tenían (TASK-049/050). Mapeo
// completo en 06-seguridad.md §52. `IdentidadTemporal`/`X-Usuario-Id`/`X-Empresa-Id` ya no se
// usan aquí: el actor de negocio y la empresa se derivan siempre del JWT (ver
// ObtenerUsuarioIdAutenticado y los casos de uso de creación/listado, que resuelven la empresa
// internamente a partir de Usuario.Empresa.Id).
[ApiController]
[Route("api/v1/requisiciones")]
public sealed class RequisicionesController : ControllerBase
{
    private readonly IniciarOContinuarRequisicionUseCase _iniciarOContinuar;
    private readonly ObtenerRequisicionUseCase _obtener;
    private readonly ListarRequisicionesUseCase _listar;
    private readonly ListarRequisicionesPendientesDeRevisionUseCase _listarPendientesDeRevision;
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
        ObtenerRequisicionUseCase obtener,
        ListarRequisicionesUseCase listar,
        ListarRequisicionesPendientesDeRevisionUseCase listarPendientesDeRevision,
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
        _obtener = obtener;
        _listar = listar;
        _listarPendientesDeRevision = listarPendientesDeRevision;
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

    // docs/05-api.md §17.1/§54.6 punto 1: "mis requisiciones" — empresa derivada del JWT.
    [Authorize(Policy = "Permiso:REQUISICION_VER")]
    [HttpGet]
    public ActionResult<ApiResponse<IReadOnlyList<RequisicionResponse>>> Listar()
    {
        var resultado = _listar.Ejecutar(ObtenerUsuarioIdAutenticado());
        return Ok(ApiResponse<IReadOnlyList<RequisicionResponse>>.De(resultado));
    }

    // docs/05-api.md §24.1/§54.6 punto 1: bandeja de revisión, misma empresa del JWT.
    [Authorize(Policy = "Permiso:REQUISICION_VER")]
    [HttpGet("pendientes-revision")]
    public ActionResult<ApiResponse<IReadOnlyList<RequisicionResponse>>> ListarPendientesDeRevision()
    {
        var resultado = _listarPendientesDeRevision.Ejecutar(ObtenerUsuarioIdAutenticado());
        return Ok(ApiResponse<IReadOnlyList<RequisicionResponse>>.De(resultado));
    }

    // docs/05-api.md §17.2/§54.6 punto 2. RN-060: corrige TASK-032 (se implementó sin alcance) —
    // ahora exige la misma empresa, igual que el resto de rutas de este Controller.
    [Authorize(Policy = "Permiso:REQUISICION_VER")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpGet("{id:int}")]
    public ActionResult<ApiResponse<RequisicionResponse>> Obtener(int id)
    {
        var resultado = _obtener.Ejecutar(id);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §18. "Crea o recupera": no se puede saber de antemano si el resultado es
    // un recurso nuevo o uno existente, así que se responde 200 en ambos casos (no 201). Sin
    // AlcanceRequisicion aquí: todavía no existe un recurso con {id} contra el que comprobar
    // alcance — la empresa se deriva de Usuario.Empresa.Id dentro del caso de uso.
    [Authorize(Policy = "Permiso:REQUISICION_CREAR")]
    [HttpPost]
    public ActionResult<ApiResponse<RequisicionResponse>> IniciarOContinuar([FromBody] CrearRequisicionRequest request)
    {
        var resultado = _iniciarOContinuar.Ejecutar(ObtenerUsuarioIdAutenticado(), request, DateTime.UtcNow);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §19.
    [Authorize(Policy = "Permiso:REQUISICION_CREAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpPost("{id:int}/detalles")]
    public ActionResult<ApiResponse<RequisicionResponse>> AgregarDetalle(int id, [FromBody] AgregarDetalleRequisicionRequest request)
    {
        var resultado = _agregarDetalle.Ejecutar(id, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §20.
    [Authorize(Policy = "Permiso:REQUISICION_MODIFICAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpPut("{id:int}/detalles/{detalleId:int}")]
    public ActionResult<ApiResponse<RequisicionResponse>> ActualizarDetalle(
        int id, int detalleId, [FromBody] ActualizarDetalleRequisicionRequest request)
    {
        var resultado = _actualizarDetalle.Ejecutar(id, detalleId, request);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §21.
    [Authorize(Policy = "Permiso:REQUISICION_MODIFICAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
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
    [Authorize(Policy = "Permiso:REQUISICION_CREAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpPost("{id:int}/detalles/{detalleId:int}/distribuciones")]
    public ActionResult<ApiResponse<RequisicionResponse>> AgregarDistribucion(
        int id, int detalleId, [FromBody] AgregarDistribucionRequest request)
    {
        var resultado = _agregarDistribucion.Ejecutar(id, detalleId, request.SedeId, request.Cantidad);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RequisicionResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:REQUISICION_MODIFICAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpPut("{id:int}/detalles/{detalleId:int}/distribuciones/{distribucionId:int}")]
    public ActionResult<ApiResponse<RequisicionResponse>> ModificarDistribucion(
        int id, int detalleId, int distribucionId, [FromBody] ModificarDistribucionRequest request)
    {
        var resultado = _modificarDistribucion.Ejecutar(id, detalleId, distribucionId, request.NuevaCantidad);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    [Authorize(Policy = "Permiso:REQUISICION_MODIFICAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpDelete("{id:int}/detalles/{detalleId:int}/distribuciones/{distribucionId:int}")]
    public ActionResult<ApiResponse<RequisicionResponse>> EliminarDistribucion(int id, int detalleId, int distribucionId)
    {
        var resultado = _eliminarDistribucion.Ejecutar(id, detalleId, distribucionId);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // TASK-028/RN-013: guardar no es enviar. No documentado como ruta propia en
    // docs/05-api.md (el documento no enumera un endpoint dedicado), se expone como acción
    // explícita siguiendo la misma convención que /enviar, /aprobar, /devolver (05-api.md §41).
    [Authorize(Policy = "Permiso:REQUISICION_CREAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpPost("{id:int}/guardar")]
    public ActionResult<ApiResponse<RequisicionResponse>> GuardarBorrador(int id)
    {
        var resultado = _guardarBorrador.Ejecutar(id);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // docs/05-api.md §23. TASK-016: primer endpoint protegido por permiso (06-seguridad.md §52) —
    // requiere autenticación (401 si falta/JWT inválido) + REQUISICION_ENVIAR (403 si no lo
    // tiene). TASK-050 agrega alcance por empresa (403 si la Requisicion no es de la Empresa del
    // usuario autenticado) y usa el UsuarioId del JWT como actor de negocio.
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
    // dispara). RN-060/06-seguridad.md §52: reutiliza REQUISICION_APROBAR — mismo actor que
    // aprueba/devuelve, sin permiso propio porque nadie le asigna un dueño distinto.
    [Authorize(Policy = "Permiso:REQUISICION_APROBAR")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpPost("{id:int}/iniciar-revision")]
    public ActionResult<ApiResponse<RequisicionResponse>> IniciarRevision(int id)
    {
        var resultado = _iniciarRevision.Ejecutar(id, ObtenerUsuarioIdAutenticado(), DateTime.UtcNow);
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
    [Authorize(Policy = "Permiso:REQUISICION_DEVOLVER")]
    [Authorize(Policy = "AlcanceRequisicion")]
    [HttpPost("{id:int}/devolver")]
    public ActionResult<ApiResponse<RequisicionResponse>> Devolver(int id, [FromBody] DevolverRequisicionRequest request)
    {
        var resultado = _devolver.Ejecutar(id, ObtenerUsuarioIdAutenticado(), DateTime.UtcNow, request);
        return Ok(ApiResponse<RequisicionResponse>.De(resultado));
    }

    // Solo se usa en endpoints con [Authorize]: para cuando esta línea se ejecuta, la
    // autorización (Permiso + AlcanceRequisicion cuando aplica) ya exigió con éxito un claim
    // "sub" válido — PermisoAuthorizationHandler/AlcanceRequisicionAuthorizationHandler ya lo
    // parsearon para llegar hasta aquí, así que el parseo no puede fallar en la práctica.
    private int ObtenerUsuarioIdAutenticado() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
}
