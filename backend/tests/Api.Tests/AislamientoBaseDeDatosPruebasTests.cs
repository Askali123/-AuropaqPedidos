using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Prueba de infraestructura (no de negocio): demuestra que Api.Tests está aislada de la base de
// datos de desarrollo. Ver ApiWebApplicationFactory para el detalle del incidente que corrige
// (la suite borraba/recreaba "AuropaqPedidos" en vez de "AuropaqPedidosTests").
public sealed class AislamientoBaseDeDatosPruebasTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AislamientoBaseDeDatosPruebasTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void El_DbContext_resuelto_por_DI_apunta_a_la_base_de_datos_de_tests()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();

        var cadenaConexion = db.Database.GetConnectionString();
        Assert.NotNull(cadenaConexion);

        var baseDeDatos = new SqlConnectionStringBuilder(cadenaConexion!).InitialCatalog;

        Assert.Equal("AuropaqPedidosTests", baseDeDatos);
        Assert.NotEqual("AuropaqPedidos", baseDeDatos);
    }

    [Fact]
    public void La_conexion_real_abierta_por_el_DbContext_es_la_base_de_datos_de_tests()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();

        // GetDbConnection().Database refleja la base de datos de la conexión ADO.NET real, no
        // solo el string de configuración: confirma que SQL Server efectivamente resolvió y
        // aceptó "AuropaqPedidosTests" (la base existe porque ApiWebApplicationFactory ya la
        // creó/migró en InitializeAsync antes de que corra cualquier prueba de la clase).
        Assert.Equal("AuropaqPedidosTests", db.Database.GetDbConnection().Database);
    }
}
