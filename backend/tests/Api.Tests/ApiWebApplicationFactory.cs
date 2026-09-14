using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Tests;

// Base de datos SQL Server real y separada de la de desarrollo (03-arquitectura.md §42:
// "Application + EF Core + SQL Server" es el nivel de prueba de integración documentado).
// Se recrea desde cero en cada ejecución de la suite para que las pruebas sean deterministas.
//
// AISLAMIENTO (corrige incidente detectado en auditoría: la suite estaba borrando/recreando la
// base de datos de desarrollo "AuropaqPedidos"):
// Program.cs (AddInfrastructure) llama a IConfiguration.GetConnectionString(...) de forma
// ansiosa (eager) y captura el string resultante en el closure de "options.UseSqlServer(...)"
// ANTES de que exista el DbContextOptions final. Ese valor queda fijo para siempre en esa
// instancia de opciones. El ConfigureAppConfiguration de abajo agrega la cadena de tests a
// IConfiguration, pero para el modelo de hosting mínimo (Program.cs con WebApplication.
// CreateBuilder + top-level statements) NO hay garantía de que ese override ya esté fusionado
// en builder.Configuration en el momento exacto en que Program.cs invoca AddInfrastructure — de
// hecho, en este proyecto no lo estaba, y por eso el DbContext terminaba apuntando a la cadena
// de appsettings.Development.json ("AuropaqPedidos") en vez de a la de tests. Por eso NO basta
// con sobrescribir IConfiguration: hay que reemplazar directamente el registro de
// DbContextOptions<AuropaqPedidosDbContext> en el contenedor de DI (patrón documentado por
// Microsoft para "Integration tests" con EF Core), lo cual es efectivo sin importar en qué
// momento leyó Program.cs la configuración.
public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string BaseDeDatosTests = "AuropaqPedidosTests";

    private const string ConnectionString =
        $"Server=localhost;Database={BaseDeDatosTests};Trusted_Connection=True;TrustServerCertificate=True;";

    // TASK-015: Program.cs lee 'Jwt:Key' de forma ansiosa (builder.Configuration[...], igual que
    // el connection string documentado abajo) y falla explícitamente si falta — un override vía
    // ConfigureAppConfiguration NO llega a tiempo (mismo problema de timing ya documentado en
    // esta clase). Se fija como variable de entorno en el constructor estático: se ejecuta antes
    // de que exista cualquier instancia de esta clase y, por lo tanto, antes de que
    // WebApplication.CreateBuilder(args) construya el host — CreateBuilder lee variables de
    // entorno (AddEnvironmentVariables, "__" como separador de jerarquía) desde su propio
    // arranque, sin depender de ningún hook posterior de WebApplicationFactory. Valor exclusivo
    // de pruebas, nunca el mismo que desarrollo (user-secrets) ni producción.
    static ApiWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", "clave-exclusiva-de-pruebas-api-tests-nunca-usar-fuera-de-aqui");
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        // Se conserva por si algo más lee IConfiguration directamente, pero NO es lo que
        // garantiza el aislamiento (ver comentario de la clase) — eso lo hace ConfigureServices.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AuropaqPedidos"] = ConnectionString
            });
        });

        // Reemplaza el registro de DbContextOptions que Program.cs ya armó con la cadena
        // equivocada: se quita ese descriptor puntual y se vuelve a registrar el DbContext
        // apuntando explícitamente a la base de datos de tests. Esto corre después de que la
        // Api ya ejecutó su propio AddInfrastructure, así que gana siempre.
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AuropaqPedidosDbContext>>();
            services.AddDbContext<AuropaqPedidosDbContext>(options => options.UseSqlServer(ConnectionString));
        });
    }

    // Protección explícita: si por cualquier motivo (cambio futuro en la configuración de DI,
    // en Program.cs, etc.) el DbContext de pruebas terminara apuntando a otra base de datos que
    // no sea la de tests, se aborta ANTES de ejecutar EnsureDeleted/Migrate — nunca se asume que
    // "seguramente sí" apunta a la base correcta (docs/06-seguridad.md §67, aplicado aquí como
    // protección de infraestructura de pruebas, no de negocio).
    private static void AsegurarBaseDeDatosDeTests(AuropaqPedidosDbContext db)
    {
        var cadenaConexion = db.Database.GetConnectionString()
            ?? throw new InvalidOperationException(
                "El DbContext de Api.Tests no tiene connection string configurada.");

        var baseDeDatos = new SqlConnectionStringBuilder(cadenaConexion).InitialCatalog;

        if (!string.Equals(baseDeDatos, BaseDeDatosTests, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Protección de aislamiento de Api.Tests: se esperaba la base de datos " +
                $"'{BaseDeDatosTests}', pero el DbContext está configurado para " +
                $"'{baseDeDatos}'. Se aborta antes de ejecutar cualquier operación destructiva " +
                "(EnsureDeleted/Migrate) para no afectar una base de datos que no es de pruebas.");
        }
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        AsegurarBaseDeDatosDeTests(db);
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            AsegurarBaseDeDatosDeTests(db);
            await db.Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
    }
}
