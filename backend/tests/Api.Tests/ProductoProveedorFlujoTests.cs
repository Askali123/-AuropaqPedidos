using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de la relación Producto-Proveedor (TASK-019, 02-dominio.md §11,
// 04-base-datos.md §15, 05-api.md §30), a través de la Api real (Controllers + Application +
// Infrastructure + SQL Server de pruebas).
// RN-059/060: PRODUCTO_VER/CREAR/EDITAR para las rutas anidadas bajo /productos (mismo permiso
// que el resto del catálogo de Producto — sin ciclo de vida propio, ver RN-059 punto 1);
// PROVEEDOR_VER para la ruta anidada bajo /proveedores. Sin alcance por empresa (catálogo global).
public sealed class ProductoProveedorFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private const int NumeroBootstrap = 90_070;

    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public ProductoProveedorFlujoTests(ApiWebApplicationFactory factory)
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
            db.Empresas.Add(new AuropaqPedidos.Domain.Entities.Empresa(empresaId, "Empresa bootstrap producto-proveedor"));
            await db.SaveChangesAsync();
        }

        return await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, NumeroBootstrap, empresaId,
            "PRODUCTO_VER", "PRODUCTO_CREAR", "PRODUCTO_EDITAR", "PROVEEDOR_VER", "PROVEEDOR_CREAR");
    }

    private static HttpRequestMessage ConToken(HttpMethod metodo, string url, string token)
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

    private async Task<ProductoDto> CrearProductoAsync(string token, string nombre)
    {
        var categoria = await CrearCategoriaAsync(token, $"Categoría {nombre}");
        var unidadMedida = await CrearUnidadMedidaAsync(token, $"UM-{nombre}", $"Unidad {nombre}");
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/productos", token);
        solicitud.Content = JsonContent.Create(new { nombre, categoriaId = categoria.Id, unidadMedidaId = unidadMedida.Id });
        var respuesta = await _cliente.SendAsync(solicitud);
        return (await respuesta.Content.ReadFromJsonAsync<Envoltorio<ProductoDto>>())!.Data;
    }

    private async Task<ProveedorDto> CrearProveedorAsync(string token, string nombre)
    {
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/proveedores", token);
        solicitud.Content = JsonContent.Create(new { nombre });
        var respuesta = await _cliente.SendAsync(solicitud);
        return (await respuesta.Content.ReadFromJsonAsync<Envoltorio<ProveedorDto>>())!.Data;
    }

    [Fact]
    public async Task Asociar_proveedor_a_producto_responde_201_y_aparece_en_ambos_listados()
    {
        var token = await TokenAsync();
        var producto = await CrearProductoAsync(token, "Papel higiénico 601");
        var proveedor = await CrearProveedorAsync(token, "Distribuidora ABC 601");

        var asociar = ConToken(HttpMethod.Post, $"/api/v1/productos/{producto.Id}/proveedores", token);
        asociar.Content = JsonContent.Create(new { proveedorId = proveedor.Id, codigoProveedor = "ABC-001" });
        var respuesta = await _cliente.SendAsync(asociar);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<ProductoProveedorDto>>();
        Assert.Equal("ABC-001", cuerpo!.Data.CodigoProveedor);
        Assert.True(cuerpo.Data.Activo);

        var porProducto = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/productos/{producto.Id}/proveedores", token));
        var listaPorProducto = await porProducto.Content.ReadFromJsonAsync<Envoltorio<List<ProductoProveedorDto>>>();
        Assert.Contains(listaPorProducto!.Data, r => r.Id == cuerpo.Data.Id);

        var porProveedor = await _cliente.SendAsync(ConToken(HttpMethod.Get, $"/api/v1/proveedores/{proveedor.Id}/productos", token));
        var listaPorProveedor = await porProveedor.Content.ReadFromJsonAsync<Envoltorio<List<ProductoProveedorDto>>>();
        Assert.Contains(listaPorProveedor!.Data, r => r.Id == cuerpo.Data.Id);
    }

    [Fact]
    public async Task Asociar_a_producto_inexistente_devuelve_404()
    {
        var token = await TokenAsync();
        var proveedor = await CrearProveedorAsync(token, "Distribuidora ABC 602");

        var asociar = ConToken(HttpMethod.Post, "/api/v1/productos/999999/proveedores", token);
        asociar.Content = JsonContent.Create(new { proveedorId = proveedor.Id, codigoProveedor = "ABC-001" });
        var respuesta = await _cliente.SendAsync(asociar);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Asociar_el_mismo_proveedor_dos_veces_al_mismo_producto_devuelve_422()
    {
        var token = await TokenAsync();
        var producto = await CrearProductoAsync(token, "Papel higiénico 603");
        var proveedor = await CrearProveedorAsync(token, "Distribuidora ABC 603");

        var primera = ConToken(HttpMethod.Post, $"/api/v1/productos/{producto.Id}/proveedores", token);
        primera.Content = JsonContent.Create(new { proveedorId = proveedor.Id, codigoProveedor = "ABC-001" });
        await _cliente.SendAsync(primera);

        var segunda = ConToken(HttpMethod.Post, $"/api/v1/productos/{producto.Id}/proveedores", token);
        segunda.Content = JsonContent.Create(new { proveedorId = proveedor.Id, codigoProveedor = "ABC-002" });
        var respuesta = await _cliente.SendAsync(segunda);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorEnvoltorio>();
        Assert.Equal("REGLA_DE_NEGOCIO_VIOLADA", error!.Error.Code);
    }

    [Fact]
    public async Task Actualizar_relacion_cambia_datos_y_puede_desactivarla()
    {
        var token = await TokenAsync();
        var producto = await CrearProductoAsync(token, "Papel higiénico 604");
        var proveedor = await CrearProveedorAsync(token, "Distribuidora ABC 604");

        var crear = ConToken(HttpMethod.Post, $"/api/v1/productos/{producto.Id}/proveedores", token);
        crear.Content = JsonContent.Create(new { proveedorId = proveedor.Id, codigoProveedor = "ABC-001" });
        var creada = (await (await _cliente.SendAsync(crear)).Content.ReadFromJsonAsync<Envoltorio<ProductoProveedorDto>>())!.Data;

        var actualizar = ConToken(HttpMethod.Put, $"/api/v1/productos/{producto.Id}/proveedores/{creada.Id}", token);
        actualizar.Content = JsonContent.Create(new
        {
            codigoProveedor = "ABC-002",
            descripcionProveedor = "Actualizada",
            categoriaProveedor = (string?)null,
            unidadProveedor = (string?)null,
            activo = false,
        });
        var respuesta = await _cliente.SendAsync(actualizar);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<ProductoProveedorDto>>();
        Assert.Equal("ABC-002", cuerpo!.Data.CodigoProveedor);
        Assert.False(cuerpo.Data.Activo);
    }

    // TASK-051: el productoId de la ruta debe corresponder realmente a la relación.
    [Fact]
    public async Task Actualizar_con_productoId_que_no_corresponde_a_la_relacion_devuelve_404()
    {
        var token = await TokenAsync();
        var producto = await CrearProductoAsync(token, "Papel higiénico 605");
        var otroProducto = await CrearProductoAsync(token, "Jabón 605");
        var proveedor = await CrearProveedorAsync(token, "Distribuidora ABC 605");

        var crear = ConToken(HttpMethod.Post, $"/api/v1/productos/{producto.Id}/proveedores", token);
        crear.Content = JsonContent.Create(new { proveedorId = proveedor.Id, codigoProveedor = "ABC-001" });
        var creada = (await (await _cliente.SendAsync(crear)).Content.ReadFromJsonAsync<Envoltorio<ProductoProveedorDto>>())!.Data;

        var actualizar = ConToken(HttpMethod.Put, $"/api/v1/productos/{otroProducto.Id}/proveedores/{creada.Id}", token);
        actualizar.Content = JsonContent.Create(new
        {
            codigoProveedor = "ABC-002",
            descripcionProveedor = (string?)null,
            categoriaProveedor = (string?)null,
            unidadProveedor = (string?)null,
            activo = true,
        });
        var respuesta = await _cliente.SendAsync(actualizar);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_proveedores_de_producto_inexistente_devuelve_404()
    {
        var token = await TokenAsync();

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/productos/999999/proveedores", token));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_productos_de_proveedor_inexistente_devuelve_404()
    {
        var token = await TokenAsync();

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/proveedores/999999/productos", token));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Asociar_proveedor_sin_jwt_devuelve_401()
    {
        var respuesta = await _cliente.PostAsync("/api/v1/productos/1/proveedores", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Asociar_proveedor_sin_permiso_PRODUCTO_CREAR_devuelve_403()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresaId = 700_000 + NumeroBootstrap + 1;
        db.Empresas.Add(new AuropaqPedidos.Domain.Entities.Empresa(empresaId, "Empresa bootstrap sin permiso"));
        await db.SaveChangesAsync();
        var tokenSinPermiso = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, NumeroBootstrap + 1, empresaId, "PRODUCTO_VER");

        var solicitud = ConToken(HttpMethod.Post, "/api/v1/productos/1/proveedores", tokenSinPermiso);
        solicitud.Content = JsonContent.Create(new { proveedorId = 1, codigoProveedor = "ABC-001" });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ErrorEnvoltorio(ErrorDetalleDto Error);

    private sealed record ErrorDetalleDto(string Code, string Message, IReadOnlyList<string> Details);

    private sealed record CategoriaDto(int Id, string Nombre, string? Descripcion, bool Activo);

    private sealed record UnidadMedidaDto(int Id, string Codigo, string Nombre, bool Activo);

    private sealed record ProductoDto(int Id, string Nombre, bool Activo);

    private sealed record ProveedorDto(int Id, string Nombre, bool Activo);

    private sealed record ProductoProveedorDto(
        int Id, int ProductoId, string ProductoNombre, int ProveedorId, string ProveedorNombre,
        string CodigoProveedor, string? DescripcionProveedor, string? CategoriaProveedor, string? UnidadProveedor, bool Activo);
}
