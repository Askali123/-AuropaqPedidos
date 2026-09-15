using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de integración de Proveedor (TASK-018, 05-api.md §29) y Solicitud de producto no
// catalogado (TASK-017, 05-api.md §28), a través de la Api real (Controllers + Application +
// Infrastructure + SQL Server de pruebas). RN-059/060 (punto 8, 2026-09-15): PROVEEDOR_*
// (sin alcance); PRODUCTO_SOLICITAR para enviar la solicitud (alcance por empresa: el
// solicitante propone para la suya); PRODUCTO_VER/CREAR para resolverla (sin alcance — un
// gestor de catálogo resuelve de cualquier empresa, 06-seguridad.md §12).
public sealed class ProveedoresYSolicitudesFlujoTests : IClassFixture<ApiWebApplicationFactory>
{
    private const int NumeroBootstrapProveedor = 90_050;
    private const int NumeroBootstrapGestor = 90_051;

    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public ProveedoresYSolicitudesFlujoTests(ApiWebApplicationFactory factory)
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

    private async Task<string> TokenBootstrapAsync(int numero, params string[] permisos)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var empresaId = 700_000 + numero;
        if (await db.Empresas.FindAsync(empresaId) is null)
        {
            db.Empresas.Add(new AuropaqPedidos.Domain.Entities.Empresa(empresaId, $"Empresa bootstrap {numero}"));
            await db.SaveChangesAsync();
        }

        return await AutorizacionHelper.CrearTokenConPermisosAsync(_factory, db, numero, empresaId, permisos);
    }

    private Task<string> TokenProveedorAsync() =>
        TokenBootstrapAsync(NumeroBootstrapProveedor, "PROVEEDOR_VER", "PROVEEDOR_CREAR", "PROVEEDOR_EDITAR");

    private Task<string> TokenGestorCatalogoAsync() =>
        TokenBootstrapAsync(NumeroBootstrapGestor, "PRODUCTO_VER", "PRODUCTO_CREAR");

    private async Task<string> TokenSolicitanteAsync(int numero, int empresaId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        return await AutorizacionHelper.CrearTokenConPermisosAsync(_factory, db, numero, empresaId, "PRODUCTO_SOLICITAR");
    }

    private HttpRequestMessage ConToken(HttpMethod metodo, string url, string token)
    {
        var solicitud = new HttpRequestMessage(metodo, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return solicitud;
    }

    private async Task<ProveedorDto> CrearProveedorAsync(string token, string nombre)
    {
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/proveedores", token);
        solicitud.Content = JsonContent.Create(new { nombre });
        var respuesta = await _cliente.SendAsync(solicitud);
        return (await respuesta.Content.ReadFromJsonAsync<Envoltorio<ProveedorDto>>())!.Data;
    }

    private async Task<SolicitudDto> SolicitarProductoAsync(string tokenSolicitante, string nombreSolicitado)
    {
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/solicitudes-producto", tokenSolicitante);
        solicitud.Content = JsonContent.Create(new { nombreSolicitado });
        var respuesta = await _cliente.SendAsync(solicitud);
        return (await respuesta.Content.ReadFromJsonAsync<Envoltorio<SolicitudDto>>())!.Data;
    }

    [Fact]
    public async Task Crear_proveedor_responde_201_y_aparece_en_el_listado()
    {
        var token = await TokenProveedorAsync();
        var proveedor = await CrearProveedorAsync(token, "Distribuidora ABC 501");

        var listado = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/proveedores", token));
        var cuerpo = await listado.Content.ReadFromJsonAsync<Envoltorio<List<ProveedorDto>>>();
        Assert.Contains(cuerpo!.Data, p => p.Id == proveedor.Id && p.Activo);
    }

    [Fact]
    public async Task Obtener_proveedor_inexistente_devuelve_404()
    {
        var token = await TokenProveedorAsync();

        var respuesta = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/proveedores/999999", token));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_proveedor_cambia_datos_y_puede_desactivarlo()
    {
        var token = await TokenProveedorAsync();
        var proveedor = await CrearProveedorAsync(token, "Distribuidora ABC 503");

        var solicitud = ConToken(HttpMethod.Put, $"/api/v1/proveedores/{proveedor.Id}", token);
        solicitud.Content = JsonContent.Create(new
        {
            nombre = "Distribuidora ABC S.A.S. 503",
            nit = "900333333-1",
            contacto = (string?)null,
            telefono = (string?)null,
            correo = (string?)null,
            activo = false,
        });
        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<ProveedorDto>>();
        Assert.Equal("Distribuidora ABC S.A.S. 503", cuerpo!.Data.Nombre);
        Assert.False(cuerpo.Data.Activo);
    }

    [Fact]
    public async Task Solicitar_producto_sin_jwt_devuelve_401()
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Post, "/api/v1/solicitudes-producto");
        solicitud.Content = JsonContent.Create(new { nombreSolicitado = "Producto nuevo" });

        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Solicitar_producto_sin_permiso_devuelve_403()
    {
        var escenario = await NuevoEscenarioAsync(509);
        var token = await TokenBootstrapAsync(509); // sin PRODUCTO_SOLICITAR
        var solicitud = ConToken(HttpMethod.Post, "/api/v1/solicitudes-producto", token);
        solicitud.Content = JsonContent.Create(new { nombreSolicitado = "Producto nuevo" });

        var respuesta = await _cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Solicitar_producto_responde_201_pendiente_y_aparece_en_pendientes()
    {
        var escenario = await NuevoEscenarioAsync(504);
        var tokenSolicitante = await TokenSolicitanteAsync(504, escenario.EmpresaId);
        var tokenGestor = await TokenGestorCatalogoAsync();

        var solicitud = await SolicitarProductoAsync(tokenSolicitante, "Producto nuevo 504");

        Assert.Equal("Pendiente", solicitud.Estado);
        Assert.Equal(escenario.EmpresaId, solicitud.EmpresaId);
        var pendientes = await _cliente.SendAsync(ConToken(HttpMethod.Get, "/api/v1/solicitudes-producto/pendientes", tokenGestor));
        var cuerpo = await pendientes.Content.ReadFromJsonAsync<Envoltorio<List<SolicitudDto>>>();
        Assert.Contains(cuerpo!.Data, s => s.Id == solicitud.Id);
    }

    [Fact]
    public async Task Homologar_solicitud_la_asocia_a_un_producto_existente()
    {
        var escenario = await NuevoEscenarioAsync(505);
        var tokenSolicitante = await TokenSolicitanteAsync(505, escenario.EmpresaId);
        var tokenGestor = await TokenGestorCatalogoAsync();
        var solicitud = await SolicitarProductoAsync(tokenSolicitante, "Producto nuevo 505");

        var homologar = ConToken(HttpMethod.Post, $"/api/v1/solicitudes-producto/{solicitud.Id}/homologar", tokenGestor);
        homologar.Content = JsonContent.Create(new { productoId = escenario.ProductoId });

        var respuesta = await _cliente.SendAsync(homologar);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<SolicitudDto>>();
        Assert.Equal("Homologado", cuerpo!.Data.Estado);
        Assert.Equal(escenario.ProductoId, cuerpo.Data.ProductoResultanteId);
    }

    [Fact]
    public async Task Crear_producto_desde_solicitud_crea_un_producto_nuevo()
    {
        var escenario = await NuevoEscenarioAsync(506);
        var tokenSolicitante = await TokenSolicitanteAsync(506, escenario.EmpresaId);
        var tokenGestor = await TokenGestorCatalogoAsync();
        var solicitud = await SolicitarProductoAsync(tokenSolicitante, "Producto nuevo 506");

        var crearCategoria = ConToken(HttpMethod.Post, "/api/v1/categorias", tokenGestor);
        crearCategoria.Content = JsonContent.Create(new { nombre = "Categoría 506" });
        var categoria = await _cliente.SendAsync(crearCategoria);
        var categoriaId = (await categoria.Content.ReadFromJsonAsync<Envoltorio<CategoriaDto>>())!.Data.Id;

        var crearUnidad = ConToken(HttpMethod.Post, "/api/v1/unidades-medida", tokenGestor);
        crearUnidad.Content = JsonContent.Create(new { codigo = "UNIDAD506", nombre = "Unidad" });
        var unidad = await _cliente.SendAsync(crearUnidad);
        var unidadId = (await unidad.Content.ReadFromJsonAsync<Envoltorio<UnidadMedidaDto>>())!.Data.Id;

        var crearProducto = ConToken(HttpMethod.Post, $"/api/v1/solicitudes-producto/{solicitud.Id}/crear-producto", tokenGestor);
        crearProducto.Content = JsonContent.Create(new { categoriaId, unidadMedidaId = unidadId });

        var respuesta = await _cliente.SendAsync(crearProducto);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<SolicitudDto>>();
        Assert.Equal("Creado", cuerpo!.Data.Estado);
        Assert.NotNull(cuerpo.Data.ProductoResultanteId);
    }

    [Fact]
    public async Task Rechazar_solicitud_exige_motivo()
    {
        var escenario = await NuevoEscenarioAsync(507);
        var tokenSolicitante = await TokenSolicitanteAsync(507, escenario.EmpresaId);
        var tokenGestor = await TokenGestorCatalogoAsync();
        var solicitud = await SolicitarProductoAsync(tokenSolicitante, "Producto nuevo 507");

        var rechazar = ConToken(HttpMethod.Post, $"/api/v1/solicitudes-producto/{solicitud.Id}/rechazar", tokenGestor);
        rechazar.Content = JsonContent.Create(new { motivo = "No cumple las condiciones del catálogo" });

        var respuesta = await _cliente.SendAsync(rechazar);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<Envoltorio<SolicitudDto>>();
        Assert.Equal("Rechazado", cuerpo!.Data.Estado);
    }

    [Fact]
    public async Task Rechazar_solicitud_inexistente_devuelve_404()
    {
        var tokenGestor = await TokenGestorCatalogoAsync();
        var rechazar = ConToken(HttpMethod.Post, "/api/v1/solicitudes-producto/999999/rechazar", tokenGestor);
        rechazar.Content = JsonContent.Create(new { motivo = "Motivo" });

        var respuesta = await _cliente.SendAsync(rechazar);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record ProveedorDto(int Id, string Nombre, string? Nit, string? Contacto, string? Telefono, string? Correo, bool Activo);

    private sealed record SolicitudDto(
        int Id, int EmpresaId, int UsuarioId, string NombreSolicitado, string Estado, int? ProductoResultanteId);

    private sealed record CategoriaDto(int Id, string Nombre);

    private sealed record UnidadMedidaDto(int Id, string Codigo, string Nombre);
}
