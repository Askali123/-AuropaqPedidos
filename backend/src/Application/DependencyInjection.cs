using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Consolidaciones;
using AuropaqPedidos.Application.Entregas;
using AuropaqPedidos.Application.Facturas;
using AuropaqPedidos.Application.Organizacion;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.Periodos;
using AuropaqPedidos.Application.Requisiciones;
using Microsoft.Extensions.DependencyInjection;

namespace AuropaqPedidos.Application;

// Registra los casos de uso de Application (nada de Infrastructure/HTTP aquí), simétrico a
// Infrastructure.DependencyInjection.AddInfrastructure.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListarEmpresasUseCase>();
        services.AddScoped<CrearUsuarioUseCase>();
        services.AddScoped<ListarUsuariosUseCase>();
        services.AddScoped<AsignarUsuarioASedeUseCase>();
        services.AddScoped<ObtenerSedesAutorizadasUseCase>();
        services.AddScoped<CrearRolUseCase>();
        services.AddScoped<ListarRolesUseCase>();
        services.AddScoped<AsignarRolAUsuarioUseCase>();
        services.AddScoped<ObtenerRolesDeUsuarioUseCase>();
        services.AddScoped<CrearPermisoUseCase>();
        services.AddScoped<ListarPermisosUseCase>();
        // TASK-013: registros que habían quedado pendientes (corregido en TASK-015 al tocar este
        // archivo de nuevo; sin controller que los consuma todavía, mismo caso ya existente para
        // CrearPermisoUseCase/ListarPermisosUseCase antes de tener API).
        services.AddScoped<AsignarPermisoARolUseCase>();
        services.AddScoped<ObtenerPermisosDeRolUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<UsuarioTienePermisoUseCase>();
        services.AddScoped<UsuarioTieneAlcanceSobreRequisicionUseCase>();
        services.AddScoped<ListarSedesPorEmpresaUseCase>();
        services.AddScoped<ListarProductosUseCase>();
        services.AddScoped<ListarPeriodosUseCase>();
        services.AddScoped<CrearPeriodoUseCase>();

        services.AddScoped<IniciarOContinuarRequisicionUseCase>();
        services.AddScoped<AgregarDetalleRequisicionUseCase>();
        services.AddScoped<ActualizarDetalleRequisicionUseCase>();
        services.AddScoped<EliminarDetalleRequisicionUseCase>();
        services.AddScoped<AgregarDistribucionRequisicionUseCase>();
        services.AddScoped<ModificarDistribucionRequisicionUseCase>();
        services.AddScoped<EliminarDistribucionRequisicionUseCase>();
        services.AddScoped<GuardarBorradorRequisicionUseCase>();
        services.AddScoped<EnviarRequisicionUseCase>();
        services.AddScoped<IniciarRevisionRequisicionUseCase>();
        services.AddScoped<AprobarRequisicionUseCase>();
        services.AddScoped<DevolverRequisicionUseCase>();

        services.AddScoped<CrearConsolidacionUseCase>();

        services.AddScoped<CrearPedidoProveedorUseCase>();
        services.AddScoped<AgregarDetallePedidoProveedorUseCase>();
        services.AddScoped<AgregarDistribucionPedidoUseCase>();
        services.AddScoped<EnviarPedidoProveedorUseCase>();
        services.AddScoped<CerrarPedidoProveedorUseCase>();
        services.AddScoped<CancelarPedidoProveedorUseCase>();

        services.AddScoped<CrearEntregaUseCase>();
        services.AddScoped<AgregarDetalleEntregaUseCase>();
        services.AddScoped<AgregarDistribucionEntregaUseCase>();
        services.AddScoped<CalcularCantidadPendienteUseCase>();
        services.AddScoped<AnularEntregaUseCase>();

        services.AddScoped<RegistrarFacturaUseCase>();
        services.AddScoped<AgregarDetalleFacturaUseCase>();
        services.AddScoped<AnularFacturaUseCase>();

        return services;
    }
}
