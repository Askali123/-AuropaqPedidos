using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración del CRUD de Categoría (TASK-014, 05-api.md §14), Unidad de medida
// (TASK-015, 05-api.md §15) y Producto (TASK-016, 05-api.md §13). Listar() de Producto ya estaba
// cubierto por CatalogosFlujoTests; aquí solo se cubre lo nuevo de esta sesión.
// RN-059/060 (punto 8, 2026-09-15): PRODUCTO_VER/CREAR/EDITAR, sin alcance por empresa (catálogo
// global) — un único actor bootstrap (número fijo, idempotente vía AutorizacionHelper) alcanza
// para toda la clase.
public sealed class CatalogoCrudFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private const int NumeroBootstrap = 90_040;

    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public CatalogoCrudFlujoTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<string> TokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresaId = 700_000 + NumeroBootstrap;
        if (await db.Empresas.FindAsync(empresaId) is null)
        {
            db.Empresas.Add(new AuropaqPedidos.Domain.Entities.Empresa(empresaId, "Empresa bootstrap catálogo"));
            await db.SaveChangesAsync();
        }

        return await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, NumeroBootstrap, empresaId, "PRODUCTO_VER", "PRODUCTO_CREAR", "PRODUCTO_EDITAR");
    }

    private HttpRequestMessage ConToken(HttpMethod metodo, string url, string token)
    {
        var solicitud = new HttpRequestMessage(metodo, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return solicitud;
    }

    private async Task<CategoriaDto> CrearCategoriaAsync(string token, string nombre)
    {
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/categorias", token);
        solicitud.Content = JsonContent.Create(new { nombre });
        var respuesta = await _cliente.SendAsync(solicitud);
        return (await respuesta.Content.ReadFromJsonAsync<Envoltorio<CategoriaDto>>())!.Data;
    }

    private async Task<UnidadMedidaDto> CrearUnidadMedidaAsync(string token, string codigo, string nombre)
    {
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/unidades-medida", token);
        solicitud.Content = JsonContent.Create(new { codigo, nombre });
        var respuesta = await _cliente.SendAsync(solicitud);
        return (await respuesta.Content.ReadFromJsonAsync<Envoltorio<UnidadMedidaDto>>())!.Data;
    }

    [Fact]
    public async Task Crear_categoria_responde_201_y_aparece_en_el_listado()
    {
        var token = await TokenAsync();
        var categoria = await CrearCategoriaAsync(token, "Aseo 401");

        var listado = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/categorias", token));
        var cuerpo = await listado.Content.ReadFromJsonAsync<Envoltorio<List<CategoriaDto>>>();
        Assert.Contains(cuerpo!.Data, c => c.Id == categoria.Id && c.Activo);
    }

    [Fact]
    public async Task Obtener_categoria_devuelve_la_categoria_creada()
    {
        var token = await TokenAsync();
        var categoria = await CrearCategoriaAsync(token, "Cafetería 402");

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/categorias/{categoria.Id}", token));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task Obtener_categoria_inexistente_devuelve_404()
    {
        var token = await TokenAsync();

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/categorias/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_categoria_cambia_datos_y_puede_desactivarla()
    {
        var token = await TokenAsync();
        var categoria = await CrearCategoriaAsync(token, "Papelería 403");

        var solicitud = ConToken(HttpMethod.Put, $"/api/v1/categorias/{categoria.Id}", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "Papelería y oficina",
            descripcion = "Actualizada",
            activo = false,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<CategoriaDto>>();
        Assert.Equal("Papelería y oficina", cuerpo!.Data.Nombre);
        Assert.False(cuerpo.Data.Activo);
    }

    [Fact]
    public async Task Crear_unidad_de_medida_responde_201_y_aparece_en_el_listado()
    {
        var token = await TokenAsync();
        var unidad = await CrearUnidadMedidaAsync(token, "GALON404", "Galón");

        var listado = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/unidades-medida", token));
        var cuerpo = await listado.Content.ReadFromJsonAsync<Envoltorio<List<UnidadMedidaDto>>>();
        Assert.Contains(cuerpo!.Data, u => u.Id == unidad.Id && u.Activo);
    }

    [Fact]
    public async Task Actualizar_unidad_de_medida_cambia_datos_y_puede_desactivarla()
    {
        var token = await TokenAsync();
        var unidad = await CrearUnidadMedidaAsync(token, "CAJA405", "Caja");

        var solicitud = ConToken(HttpMethod.Put, $"/api/v1/unidades-medida/{unidad.Id}", token);
        solicitud.Content = JsonContent.Create(new
        {
            codigo = "CJA405",
            nombre = "Caja grande",
            activo = false,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<UnidadMedidaDto>>();
        Assert.Equal("CJA405", cuerpo!.Data.Codigo);
        Assert.False(cuerpo.Data.Activo);
    }

    [Fact]
    public async Task Obtener_unidad_de_medida_inexistente_devuelve_404()
    {
        var token = await TokenAsync();

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/unidades-medida/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_producto_responde_201_con_categoria_y_unidad_asociadas()
    {
        var token = await TokenAsync();
        var categoria = await CrearCategoriaAsync(token, "Aseo 406");
        var unidad = await CrearUnidadMedidaAsync(token, "UNIDAD406", "Unidad");

        var solicitud = ConToken(HttpMethod.Post, "/api/v1/productos", token);
        solicitud.Content = JsonContent.Create(new
        {
            codigoInterno = "1281-406",
            nombre = "Abrasivo Regular",
            descripcion = "Descripción",
            categoriaId = categoria.Id,
            unidadMedidaId = unidad.Id,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<ProductoDto>>();
        Assert.Equal("Abrasivo Regular", cuerpo!.Data.Nombre);
        Assert.Equal(categoria.Id, cuerpo.Data.CategoriaId);
        Assert.Equal(unidad.Id, cuerpo.Data.UnidadMedidaId);
        Assert.True(cuerpo.Data.Activo);

        var obtenido = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/productos/{cuerpo.Data.Id}", token));
        Assert.Equal(HttpStatusCode.OK, obtenido.StatusCode);
    }

    [Fact]
    public async Task Crear_producto_con_categoria_inexistente_devuelve_404()
    {
        var token = await TokenAsync();
        var unidad = await CrearUnidadMedidaAsync(token, "UNIDAD407", "Unidad");

        var solicitud = ConToken(HttpMethod.Post, "/api/v1/productos", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "Producto X",
            categoriaId = 999999,
            unidadMedidaId = unidad.Id,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("RECURSO_NO_ENCONTRADO", error!.Error.Code);
    }

    [Fact]
    public async Task Crear_producto_con_unidad_de_medida_inexistente_devuelve_404()
    {
        var token = await TokenAsync();
        var categoria = await CrearCategoriaAsync(token, "Aseo 408");

        var solicitud = ConToken(HttpMethod.Post, "/api/v1/productos", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "Producto X",
            categoriaId = categoria.Id,
            unidadMedidaId = 999999,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_producto_cambia_datos_y_puede_desactivarlo()
    {
        var token = await TokenAsync();
        var categoria = await CrearCategoriaAsync(token, "Aseo 409");
        var unidad = await CrearUnidadMedidaAsync(token, "UNIDAD409", "Unidad");
        var crear = ConToken(HttpMethod.Post, "/api/v1/productos", token);
        crear.Content = JsonContent.Create(new
        {
            nombre = "Producto original",
            categoriaId = categoria.Id,
            unidadMedidaId = unidad.Id,
        });
        var creado = await _cliente.SendAsync(crear);
        var producto = (await creado.Content.ReadFromJsonAsync<Envoltorio<ProductoDto>>())!.Data;

        var solicitud = ConToken(HttpMethod.Put, $"/api/v1/productos/{producto.Id}", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "Producto renombrado",
            codigoInterno = "PROD-409",
            descripcion = "Nueva descripción",
            categoriaId = categoria.Id,
            unidadMedidaId = unidad.Id,
            activo = false,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<ProductoDto>>();
        Assert.Equal("Producto renombrado", cuerpo!.Data.Nombre);
        Assert.False(cuerpo.Data.Activo);
    }

    [Fact]
    public async Task Actualizar_producto_inexistente_devuelve_404()
    {
        var token = await TokenAsync();
        var categoria = await CrearCategoriaAsync(token, "Aseo 410");
        var unidad = await CrearUnidadMedidaAsync(token, "UNIDAD410", "Unidad");

        var solicitud = ConToken(HttpMethod.Put, "/api/v1/productos/999999", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "X",
            categoriaId = categoria.Id,
            unidadMedidaId = unidad.Id,
            activo = true,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record CategoriaDto(int Id, string Nombre, string? Descripcion, bool Activo);

    private sealed record UnidadMedidaDto(int Id, string Codigo, string Nombre, bool Activo);

    private sealed record ProductoDto(
        int Id, string Nombre, string? CodigoInterno, string? Descripcion,
        int CategoriaId, string CategoriaNombre, int UnidadMedidaId, bool Activo);
}
