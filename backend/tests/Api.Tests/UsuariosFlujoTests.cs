using System.Net;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de UsuariosController (TASK-008, docs/04-base-datos.md §7), a través
// de la Api real (Controllers + Application + Infrastructure + SQL Server de pruebas).
public sealed class UsuariosFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public UsuariosFlujoTests(ApiWebApplicationFactory factory)
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

    [Fact]
    public async Task Crear_usuario_responde_201_y_queda_persistido()
    {
        var escenario = await NuevoEscenarioAsync(201);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/usuarios", new
        {
            empresaId = escenario.EmpresaId,
            nombre = "Martha",
            apellido = "Gómez",
            correo = "martha.201@auropaq.com",
            password = "password123",
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<UsuarioDto>>();
        Assert.Equal(escenario.EmpresaId, cuerpo!.Data.EmpresaId);
        Assert.Equal("Martha", cuerpo.Data.Nombre);
        Assert.True(cuerpo.Data.Activo);

        var listado = await _cliente.GetAsync("/api/v1/usuarios");
        var cuerpoListado = await listado.Content.ReadFromJsonAsync<Envoltorio<List<UsuarioDto>>>();
        Assert.Contains(cuerpoListado!.Data, u => u.Id == cuerpo.Data.Id);
    }

    [Fact]
    public async Task Listar_usuarios_responde_200()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/usuarios");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_usuario_con_correo_duplicado_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(202);
        var body = new
        {
            empresaId = escenario.EmpresaId,
            nombre = "Martha",
            correo = "duplicado.202@auropaq.com",
            password = "password123",
        };
        await _cliente.PostAsJsonAsync("/api/v1/usuarios", body);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/usuarios", body with { nombre = "Otra Martha" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Crear_usuario_con_password_corto_devuelve_422()
    {
        var escenario = await NuevoEscenarioAsync(203);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/usuarios", new
        {
            empresaId = escenario.EmpresaId,
            nombre = "Martha",
            correo = "martha.203@auropaq.com",
            password = "1234567",
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Crear_usuario_no_expone_password_ni_hash_en_la_respuesta()
    {
        var escenario = await NuevoEscenarioAsync(204);

        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/usuarios", new
        {
            empresaId = escenario.EmpresaId,
            nombre = "Martha",
            correo = "martha.204@auropaq.com",
            password = "password123",
        });

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password123", cuerpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", cuerpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Crear_usuario_con_empresa_inexistente_devuelve_404()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/usuarios", new
        {
            empresaId = 999999,
            nombre = "Martha",
            correo = "martha.inexistente@auropaq.com",
            password = "password123",
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record UsuarioDto(
        int Id, int EmpresaId, string Nombre, string? Apellido, string Correo, bool Activo,
        DateTime FechaCreacion, DateTime FechaActualizacion);
}
