using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración del flujo de Requisición a través de la Api real (Controllers +
// Application + Infrastructure + SQL Server), según 03-arquitectura.md §42. Cubren el flujo
// principal (docs/05-api.md §48) y el mapeo de errores de negocio/no encontrado a HTTP.
//
// TASK-016/TASK-050: "/enviar" y "/aprobar" ahora exigen JWT + permiso + alcance por empresa
// (REQUISICION_ENVIAR/APROBAR, AutorizacionFlujoTests.cs cubre el contrato 401/403/alcance en
// detalle) — cada llamada a esas dos rutas en este archivo obtiene primero un token autorizado
// para la MISMA Empresa de la Requisición bajo prueba (AutorizacionHelper) y lo agrega como
// header "Authorization: Bearer ...". Desde TASK-050 esos dos endpoints ya NO leen
// "X-Usuario-Id" (el actor de negocio viene del JWT) — el header solo sigue siendo necesario en
// las demás rutas (crear, detalles, distribuir, iniciar-revision, devolver), que no están
// protegidas todavía y siguen sin cambios.
public sealed class RequisicionesFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private const string UsuarioId = "5";

    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public RequisicionesFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<Escenario> NuevoEscenarioAsync(int numero)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        return await Escenario.CrearAsync(db, numero);
    }

    private async Task<string> TokenConPermisosDeRequisicionAsync(int numero, int empresaId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        return await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, numero, empresaId, "REQUISICION_ENVIAR", "REQUISICION_APROBAR");
    }

    [Fact]
    public async Task Flujo_completo_hasta_aprobar()
    {
        var escenario = await NuevoEscenarioAsync(1);
        var token = await TokenConPermisosDeRequisicionAsync(1, escenario.EmpresaId);

        // BORRADOR
        var crear = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requisiciones");
        crear.Headers.Add("X-Usuario-Id", UsuarioId);
        crear.Headers.Add("X-Empresa-Id", escenario.EmpresaId.ToString());
        crear.Content = JsonContent.Create(new { periodoId = escenario.PeriodoId });

        var respuestaCrear = await _cliente.SendAsync(crear);
        Assert.Equal(HttpStatusCode.OK, respuestaCrear.StatusCode);
        var requisicion = await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();
        Assert.Equal("Borrador", requisicion!.Data.Estado);
        var requisicionId = requisicion.Data.Id;

        // Agregar detalle
        var respuestaDetalle = await _cliente.PostAsJsonAsync(
            $"/api/v1/requisiciones/{requisicionId}/detalles",
            new { productoId = escenario.ProductoId, cantidadSolicitada = 10, observacion = (string?)null });
        Assert.Equal(HttpStatusCode.Created, respuestaDetalle.StatusCode);
        var conDetalle = await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();
        var detalleId = conDetalle!.Data.Detalles[0].Id;

        // Distribuir toda la cantidad en una sola sede
        var respuestaDistribucion = await _cliente.PostAsJsonAsync(
            $"/api/v1/requisiciones/{requisicionId}/detalles/{detalleId}/distribuciones",
            new { sedeId = escenario.SedeId, cantidad = 10 });
        Assert.Equal(HttpStatusCode.Created, respuestaDistribucion.StatusCode);
        var conDistribucion = await respuestaDistribucion.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();
        Assert.True(conDistribucion!.Data.Detalles[0].DistribucionCompleta);

        // Enviar -> ENVIADA
        var enviar = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/enviar");
        enviar.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuestaEnviar = await _cliente.SendAsync(enviar);
        Assert.Equal(HttpStatusCode.OK, respuestaEnviar.StatusCode);
        var enviada = await respuestaEnviar.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();
        Assert.Equal("Enviada", enviada!.Data.Estado);

        // Iniciar revisión -> EN_REVISION
        var iniciarRevision = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/iniciar-revision");
        iniciarRevision.Headers.Add("X-Usuario-Id", UsuarioId);
        var respuestaRevision = await _cliente.SendAsync(iniciarRevision);
        Assert.Equal(HttpStatusCode.OK, respuestaRevision.StatusCode);

        // Aprobar -> APROBADA
        var aprobar = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/aprobar");
        aprobar.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        aprobar.Content = JsonContent.Create(new { observacion = "Requisición aprobada" });
        var respuestaAprobar = await _cliente.SendAsync(aprobar);
        Assert.Equal(HttpStatusCode.OK, respuestaAprobar.StatusCode);
        var aprobada = await respuestaAprobar.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();

        Assert.Equal("Aprobada", aprobada!.Data.Estado);
        Assert.Equal(3, aprobada.Data.Historial.Count);
    }

    [Fact]
    public async Task Flujo_de_devolucion_y_reenvio()
    {
        var escenario = await NuevoEscenarioAsync(2);
        var token = await TokenConPermisosDeRequisicionAsync(2, escenario.EmpresaId);

        var crear = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requisiciones");
        crear.Headers.Add("X-Usuario-Id", UsuarioId);
        crear.Headers.Add("X-Empresa-Id", escenario.EmpresaId.ToString());
        crear.Content = JsonContent.Create(new { periodoId = escenario.PeriodoId });
        var respuestaCrear = await _cliente.SendAsync(crear);
        var requisicionId = (await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>())!.Data.Id;

        var respuestaDetalle = await _cliente.PostAsJsonAsync(
            $"/api/v1/requisiciones/{requisicionId}/detalles",
            new { productoId = escenario.ProductoId, cantidadSolicitada = 5, observacion = (string?)null });
        var detalleId = (await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>())!.Data.Detalles[0].Id;

        await _cliente.PostAsJsonAsync(
            $"/api/v1/requisiciones/{requisicionId}/detalles/{detalleId}/distribuciones",
            new { sedeId = escenario.SedeId, cantidad = 5 });

        var enviar1 = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/enviar");
        enviar1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _cliente.SendAsync(enviar1);

        var iniciarRevision = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/iniciar-revision");
        iniciarRevision.Headers.Add("X-Usuario-Id", UsuarioId);
        await _cliente.SendAsync(iniciarRevision);

        var devolver = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/devolver");
        devolver.Headers.Add("X-Usuario-Id", UsuarioId);
        devolver.Content = JsonContent.Create(new { motivo = "Corregir cantidad" });
        var respuestaDevolver = await _cliente.SendAsync(devolver);
        Assert.Equal(HttpStatusCode.OK, respuestaDevolver.StatusCode);
        var devuelta = await respuestaDevolver.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();
        Assert.Equal("Devuelta", devuelta!.Data.Estado);

        // Corregir (mientras DEVUELTA es editable) y reenviar
        await _cliente.PutAsJsonAsync(
            $"/api/v1/requisiciones/{requisicionId}/detalles/{detalleId}",
            new { cantidadSolicitada = (int?)null, observacion = "Cantidad revisada" });

        var enviar2 = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/enviar");
        enviar2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuestaReenvio = await _cliente.SendAsync(enviar2);
        Assert.Equal(HttpStatusCode.OK, respuestaReenvio.StatusCode);
        var reenviada = await respuestaReenvio.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();

        Assert.Equal("Enviada", reenviada!.Data.Estado);
        Assert.Equal(4, reenviada.Data.Historial.Count);
    }

    [Fact]
    public async Task Enviar_requisicion_inexistente_devuelve_404_con_envoltorio_de_error()
    {
        // Empresa arbitraria: el alcance no bloquea aquí porque la requisición 999999 no existe
        // (UsuarioTieneAlcanceSobreRequisicionUseCase deja pasar para que el 404 real lo reporte
        // el caso de uso, no importa a qué empresa pertenezca este token).
        var escenario = await NuevoEscenarioAsync(900);
        var token = await TokenConPermisosDeRequisicionAsync(900, escenario.EmpresaId);
        var enviar = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requisiciones/999999/enviar");
        enviar.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var respuesta = await _cliente.SendAsync(enviar);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Enviar_sin_distribucion_completa_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(3);
        var token = await TokenConPermisosDeRequisicionAsync(3, escenario.EmpresaId);

        var crear = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requisiciones");
        crear.Headers.Add("X-Usuario-Id", UsuarioId);
        crear.Headers.Add("X-Empresa-Id", escenario.EmpresaId.ToString());
        crear.Content = JsonContent.Create(new { periodoId = escenario.PeriodoId });
        var respuestaCrear = await _cliente.SendAsync(crear);
        var requisicionId = (await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>())!.Data.Id;

        await _cliente.PostAsJsonAsync(
            $"/api/v1/requisiciones/{requisicionId}/detalles",
            new { productoId = escenario.ProductoId, cantidadSolicitada = 10, observacion = (string?)null });

        // Sin distribuir: DistribucionCompleta queda en false.
        var enviar = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/enviar");
        enviar.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuesta = await _cliente.SendAsync(enviar);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    // TASK-050: reemplaza al test anterior ("Enviar_sin_header_de_usuario_devuelve_400"), cuyo
    // comportamiento ya no existe — Enviar dejó de leer "X-Usuario-Id" (el actor de negocio
    // ahora viene del JWT). Este test confirma explícitamente el nuevo contrato: el header ya
    // NO es necesario para este endpoint.
    [Fact]
    public async Task Enviar_sin_header_X_Usuario_Id_funciona_igual_usando_el_actor_del_JWT()
    {
        // 909, no 901: Escenario.CrearAsync deriva Mes de "numero % 12 + 1" — 901 colisionaría
        // con el Periodo (2026, Mes=2) que ya usa el escenario "numero=1" de esta misma clase.
        var escenario = await NuevoEscenarioAsync(909);
        var token = await TokenConPermisosDeRequisicionAsync(909, escenario.EmpresaId);

        var crear = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requisiciones");
        crear.Headers.Add("X-Usuario-Id", UsuarioId);
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

        // Deliberadamente SIN "X-Usuario-Id": solo el JWT.
        var solicitud = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requisiciones/{requisicionId}/enviar");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<RequisicionDto>>();
        Assert.Equal("Enviada", cuerpo!.Data.Estado);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record RequisicionDto(
        int Id,
        int EmpresaId,
        int PeriodoId,
        string Estado,
        IReadOnlyList<DetalleDto> Detalles,
        IReadOnlyList<object> Historial);

    private sealed record DetalleDto(int Id, int ProductoId, int CantidadSolicitada, bool DistribucionCompleta);
}
