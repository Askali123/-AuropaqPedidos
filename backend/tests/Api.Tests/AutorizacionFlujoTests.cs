using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// TASK-016/TASK-050. Comportamiento HTTP de autorización (401/403/alcance) contra los dos
// endpoints reales protegidos (RequisicionesController.Enviar/Aprobar). Usa una Requisición real
// llevada hasta el punto de poder enviarse/aprobarse (mismo fixture que RequisicionesFlujoTests).
// Desde TASK-050 los tokens de AutorizacionHelper deben pertenecer a la MISMA Empresa que la
// Requisición bajo prueba (alcance) — cada llamada pasa "escenario.EmpresaId" explícitamente.
public sealed class AutorizacionFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string UsuarioIdNegocio = "5";

    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public AutorizacionFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<(Escenario escenario, int requisicionId)> RequisicionListaParaEnviarAsync(int numero)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await Escenario.CrearAsync(db, numero);

        var crear = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requisiciones");
        crear.Headers.Add("X-Usuario-Id", UsuarioIdNegocio);
        crear.Headers.Add("X-Empresa-Id", escenario.EmpresaId.ToString());
        crear.Content = JsonContent.Create(new { periodoId = escenario.PeriodoId });
        var respuestaCrear = await _cliente.SendAsync(crear);
        var requisicionId = (await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>())!.Data.Id;

        var respuestaDetalle = await _cliente.PostAsJsonAsync(
            $"/api/v1/requisiciones/{requisicionId}/detalles",
            new { productoId = escenario.ProductoId, cantidadSolicitada = 10, observacion = (string?)null });
        var detalleId = (await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>())!.Data.Detalles[0].Id;

        await _cliente.PostAsJsonAsync(
            $"/api/v1/requisiciones/{requisicionId}/detalles/{detalleId}/distribuciones",
            new { sedeId = escenario.SedeId, cantidad = 10 });

        return (escenario, requisicionId);
    }

    private static HttpRequestMessage EnviarRequest(int requisicionId, string usuarioIdNegocio = UsuarioIdNegocio)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/enviar");
        request.Headers.Add("X-Usuario-Id", usuarioIdNegocio);
        return request;
    }

    [Fact]
    public async Task Enviar_sin_jwt_devuelve_401()
    {
        var (_, requisicionId) = await RequisicionListaParaEnviarAsync(401);

        var respuesta = await _cliente.SendAsync(EnviarRequest(requisicionId));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Enviar_con_jwt_invalido_devuelve_401()
    {
        var (_, requisicionId) = await RequisicionListaParaEnviarAsync(402);

        var solicitud = EnviarRequest(requisicionId);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "esto-no-es-un-jwt-valido");
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Enviar_con_jwt_valido_sin_permiso_devuelve_403()
    {
        var (escenario, requisicionId) = await RequisicionListaParaEnviarAsync(403);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        // Token válido, misma empresa (alcance OK), pero sin ningún Permiso asignado (0 códigos).
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(_factory, db, 403, escenario.EmpresaId);

        var solicitud = EnviarRequest(requisicionId);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Enviar_con_jwt_valido_y_permiso_ejecuta_el_endpoint()
    {
        var (escenario, requisicionId) = await RequisicionListaParaEnviarAsync(404);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 404, escenario.EmpresaId, "REQUISICION_ENVIAR");

        var solicitud = EnviarRequest(requisicionId);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();
        Assert.Equal("Enviada", cuerpo!.Data.Estado);
    }

    [Fact]
    public async Task Enviar_con_jwt_valido_permiso_pero_de_otra_empresa_devuelve_403()
    {
        var (escenario, requisicionId) = await RequisicionListaParaEnviarAsync(406);

        Escenario otraEmpresa;
        using (var scopeOtraEmpresa = _factory.Services.CreateScope())
        {
            var dbOtraEmpresa = scopeOtraEmpresa.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            otraEmpresa = await Escenario.CrearAsync(dbOtraEmpresa, 407);
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        // Permiso correcto, pero el token pertenece a otraEmpresa.EmpresaId, no a
        // escenario.EmpresaId (dueña real de la Requisición) — debe rechazarse por alcance.
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 406, otraEmpresa.EmpresaId, "REQUISICION_ENVIAR");

        var solicitud = EnviarRequest(requisicionId);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    // Auditoría post-TASK-050: cubría el caso solo a nivel unitario
    // (UsuarioTienePermisoUseCaseTests.Usuario_inactivo_no_esta_autorizado...); faltaba la
    // confirmación end-to-end de que un usuario desactivado DESPUÉS de emitido el JWT (token aún
    // válido criptográficamente) recibe 403 — no 200 — al intentar usarlo. RN-057.
    [Fact]
    public async Task Enviar_con_jwt_valido_permiso_y_alcance_pero_usuario_inactivo_devuelve_403()
    {
        var (escenario, requisicionId) = await RequisicionListaParaEnviarAsync(408);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 408, escenario.EmpresaId, "REQUISICION_ENVIAR");

        var usuario = await db.Usuarios.SingleAsync(u => u.Correo == "autorizacion.tests.408@auropaq.com");
        usuario.Desactivar(DateTime.UtcNow);
        await db.SaveChangesAsync();

        var solicitud = EnviarRequest(requisicionId);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Aprobar_con_jwt_valido_sin_permiso_devuelve_403()
    {
        var (escenario, requisicionId) = await RequisicionListaParaEnviarAsync(405);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var tokenEnviar = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, 405, escenario.EmpresaId, "REQUISICION_ENVIAR");

        var enviar = EnviarRequest(requisicionId);
        enviar.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenEnviar);
        await _cliente.SendAsync(enviar);

        var iniciarRevision = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/iniciar-revision");
        iniciarRevision.Headers.Add("X-Usuario-Id", UsuarioIdNegocio);
        await _cliente.SendAsync(iniciarRevision);

        // Mismo token que sí tenía REQUISICION_ENVIAR, pero no REQUISICION_APROBAR: confirma que
        // la autorización es específica por permiso, no "todo o nada" para el usuario.
        var aprobar = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/aprobar");
        aprobar.Headers.Add("X-Usuario-Id", UsuarioIdNegocio);
        aprobar.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenEnviar);
        aprobar.Content = JsonContent.Create(new { observacion = (string?)null });
        var respuesta = await _cliente.SendAsync(aprobar);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Login_sigue_siendo_publico_sin_jwt()
    {
        var respuesta = await _cliente.PostAsJsonAsync(
            "/api/v1/auth/login", new { correo = "no-existe@auropaq.com", password = "password123" });

        // 401 por CREDENCIALES_INVALIDAS (negocio), no por falta de autenticación previa al
        // propio login — confirma que el endpoint es alcanzable sin JWT.
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("CREDENCIALES_INVALIDAS", error!.Error.Code);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record RequisicionDto(int Id, string Estado, IReadOnlyList<DetalleDto> Detalles);

    private sealed record DetalleDto(int Id, int ProductoId, int CantidadSolicitada, bool DistribucionCompleta);
}
