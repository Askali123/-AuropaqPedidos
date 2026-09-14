using System.Net;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// TASK-015 (POST /api/v1/auth/login), a través de la Api real (Controllers + Application +
// Infrastructure + SQL Server de pruebas). Crea el usuario primero por HTTP real
// (POST /api/v1/usuarios), igual que lo haría el flujo real, en vez de insertarlo directo en BD.
public sealed class AuthFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public AuthFlujoTests(ApiWebApplicationFactory factory)
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

    private async Task<string> CrearUsuarioAsync(int empresaId, string correo, string password)
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/usuarios", new
        {
            empresaId,
            nombre = "Martha",
            correo,
            password,
        });
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        return correo;
    }

    [Fact]
    public async Task Login_valido_responde_200_con_token()
    {
        var escenario = await NuevoEscenarioAsync(301);
        var correo = await CrearUsuarioAsync(escenario.EmpresaId, "martha.301@auropaq.com", "password123");

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { correo, password = "password123" });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<LoginDto>>();
        Assert.False(string.IsNullOrWhiteSpace(cuerpo!.Data.Token));
        Assert.Equal(correo, cuerpo.Data.Usuario.Correo);
    }

    [Fact]
    public async Task Login_no_expone_password_ni_hash_en_la_respuesta()
    {
        var escenario = await NuevoEscenarioAsync(302);
        var correo = await CrearUsuarioAsync(escenario.EmpresaId, "martha.302@auropaq.com", "password123");

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { correo, password = "password123" });

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password123", cuerpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", cuerpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_con_password_incorrecta_devuelve_401()
    {
        var escenario = await NuevoEscenarioAsync(303);
        var correo = await CrearUsuarioAsync(escenario.EmpresaId, "martha.303@auropaq.com", "password123");

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { correo, password = "otro-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("CREDENCIALES_INVALIDAS", error!.Error.Code);
    }

    [Fact]
    public async Task Login_con_correo_inexistente_devuelve_401()
    {
        var respuesta = await _cliente.PostAsJsonAsync(
            "/api/v1/auth/login", new { correo = "no-existe@auropaq.com", password = "password123" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("CREDENCIALES_INVALIDAS", error!.Error.Code);
    }

    [Fact]
    public async Task Login_con_usuario_inactivo_devuelve_401_con_el_mismo_codigo()
    {
        var escenario = await NuevoEscenarioAsync(304);
        var correo = await CrearUsuarioAsync(escenario.EmpresaId, "martha.304@auropaq.com", "password123");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            var usuario = db.Usuarios.Single(u => u.Correo == correo);
            usuario.Desactivar(DateTime.UtcNow);
            await db.SaveChangesAsync();
        }

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { correo, password = "password123" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("CREDENCIALES_INVALIDAS", error!.Error.Code);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record LoginDto(string Token, DateTime FechaExpiracion, UsuarioDto Usuario);

    private sealed record UsuarioDto(
        int Id, int EmpresaId, string Nombre, string? Apellido, string Correo, bool Activo,
        DateTime FechaCreacion, DateTime FechaActualizacion);
}
