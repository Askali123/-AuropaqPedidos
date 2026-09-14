using AuropaqPedidos.Application.Consolidaciones.Abstracciones;
using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using AuropaqPedidos.Infrastructure.Repositories;
using AuropaqPedidos.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuropaqPedidos.Infrastructure;

public static class DependencyInjection
{
    public const string NombreCadenaConexion = "AuropaqPedidos";

    // Registra el DbContext (SQL Server) y las implementaciones de los repositorios que
    // Application define como interfaces. No registra nada de Api/HTTP/autenticación.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(NombreCadenaConexion)
            ?? throw new InvalidOperationException(
                $"Falta la cadena de conexión '{NombreCadenaConexion}'. Configúrala en " +
                "appsettings.Development.json (entorno local) o en un mecanismo seguro de " +
                "secretos en otros ambientes. No debe versionarse una cadena con credenciales reales.");

        services.AddDbContext<AuropaqPedidosDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IEmpresaRepository, EmpresaRepositoryEfCore>();
        services.AddScoped<IUsuarioRepository, UsuarioRepositoryEfCore>();
        services.AddScoped<IUsuarioSedeRepository, UsuarioSedeRepositoryEfCore>();
        services.AddScoped<IRolRepository, RolRepositoryEfCore>();
        services.AddScoped<IUsuarioRolRepository, UsuarioRolRepositoryEfCore>();
        services.AddScoped<IPermisoRepository, PermisoRepositoryEfCore>();
        services.AddScoped<IRolPermisoRepository, RolPermisoRepositoryEfCore>();
        services.AddScoped<IPeriodoRepository, PeriodoRepositoryEfCore>();
        services.AddScoped<IProductoRepository, ProductoRepositoryEfCore>();
        services.AddScoped<ISedeRepository, SedeRepositoryEfCore>();
        services.AddScoped<IRequisicionRepository, RequisicionRepositoryEfCore>();
        services.AddScoped<IConsolidacionRepository, ConsolidacionRepositoryEfCore>();
        services.AddScoped<IProveedorRepository, ProveedorRepositoryEfCore>();
        services.AddScoped<IPedidoProveedorRepository, PedidoProveedorRepositoryEfCore>();
        services.AddScoped<IEntregaRepository, EntregaRepositoryEfCore>();
        services.AddScoped<ITransaccionDeEntrega, TransaccionDeEntregaEfCore>();
        services.AddScoped<IFacturaRepository, FacturaRepositoryEfCore>();
        services.AddScoped<IGeneradorDeIdentificadores, GeneradorDeIdentificadoresEfCore>();
        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<IGeneradorDeToken, JwtTokenGenerator>();

        return services;
    }
}
