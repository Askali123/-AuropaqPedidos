using AuropaqPedidos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Persistence.Context;

public sealed class AuropaqPedidosDbContext : DbContext
{
    public AuropaqPedidosDbContext(DbContextOptions<AuropaqPedidosDbContext> options) : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Sede> Sedes => Set<Sede>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioSede> UsuariosSedes => Set<UsuarioSede>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuariosRoles => Set<UsuarioRol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();
    public DbSet<Periodo> Periodos => Set<Periodo>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<ProductoProveedor> ProductosProveedores => Set<ProductoProveedor>();
    public DbSet<SolicitudProductoCatalogo> SolicitudesProductoCatalogo => Set<SolicitudProductoCatalogo>();
    public DbSet<Requisicion> Requisiciones => Set<Requisicion>();
    public DbSet<Consolidacion> Consolidaciones => Set<Consolidacion>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<PedidoProveedor> PedidosProveedor => Set<PedidoProveedor>();
    public DbSet<Entrega> Entregas => Set<Entrega>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    // DetalleRequisicion, DistribucionRequisicion y HistorialRequisicion NO se exponen como
    // DbSet propios: solo son alcanzables a través de Requisicion.Detalles/.Historial, para no
    // introducir una vía de acceso que rompa las reglas del agregado (instrucción explícita).
    // Mismo criterio para DetalleConsolidacion/AsignacionConsolidacion (vía Consolidacion),
    // DetallePedidoProveedor/DistribucionPedido (vía PedidoProveedor) y
    // DetalleEntrega/DistribucionEntrega (vía Entrega).

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuropaqPedidosDbContext).Assembly);
    }
}
