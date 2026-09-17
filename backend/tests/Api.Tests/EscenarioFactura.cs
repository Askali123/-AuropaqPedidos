using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;

namespace Api.Tests;

// Grafo mínimo Empresa→Periodo→Producto→Requisicion(Aprobada)→Consolidacion→PedidoProveedor
// (con un DetallePedidoProveedor) + Proveedor, necesario para probar Factura por HTTP: TASK-046
// se apoya en PedidoProveedor, que todavía no tiene API propia (05-api.md §32, conceptual) — se
// siembra directamente en la base de datos, igual que Escenario.CrearAsync.
internal sealed record EscenarioFactura(
    int EmpresaId, int ProveedorId, int OtroProveedorId, int PedidoProveedorId, int DetallePedidoProveedorId, int CantidadPedida)
{
    public static async Task<EscenarioFactura> CrearAsync(AuropaqPedidosDbContext db, int numero, int cantidadPedida)
    {
        var fecha = new DateTime(2026, 9, 2);

        var empresa = new Empresa(numero, $"Empresa de prueba {numero}");
        var periodo = new Periodo(
            numero, 2026, numero % 12 + 1,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
            "ABIERTO");
        var categoria = new Categoria(numero, $"Categoría {numero}");
        var unidadMedida = new UnidadMedida(numero, "UNIDAD", "Unidad");
        var producto = new Producto(numero, $"Producto {numero}", categoria, unidadMedida);
        var sede = new Sede(numero, empresa, $"Sede {numero}");

        var requisicion = new Requisicion(numero, empresa, periodo, usuarioCreacionId: 1, fecha);
        var detalleReq = requisicion.AgregarDetalle(numero, producto, cantidadPedida);
        requisicion.AgregarDistribucion(numero, detalleReq, sede, cantidadPedida);
        requisicion.Enviar(1, fecha);
        requisicion.IniciarRevision(1, fecha);
        requisicion.Aprobar(1, fecha);

        var consolidacion = new Consolidacion(numero, periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: fecha);
        consolidacion.AgregarAsignacion(numero, numero, requisicion, detalleReq, cantidadPedida);

        var proveedor = new Proveedor(numero, $"Proveedor {numero}");
        // Un segundo proveedor (sin relación con el pedido) para probar la validación de
        // coherencia proveedor↔pedido.
        var otroProveedor = new Proveedor(numero + 500, $"Otro proveedor {numero}");

        var pedido = new PedidoProveedor(numero, consolidacion, proveedor, $"PO-{numero}", usuarioCreacionId: 1, fecha);
        var detallePedido = pedido.AgregarDetalle(numero, consolidacion.Detalles[0], cantidadPedida);

        db.Empresas.Add(empresa);
        db.Periodos.Add(periodo);
        db.Categorias.Add(categoria);
        db.UnidadesMedida.Add(unidadMedida);
        db.Productos.Add(producto);
        db.Sedes.Add(sede);
        db.Requisiciones.Add(requisicion);
        db.Consolidaciones.Add(consolidacion);
        db.Proveedores.Add(proveedor);
        db.Proveedores.Add(otroProveedor);
        db.PedidosProveedor.Add(pedido);

        await db.SaveChangesAsync();

        return new EscenarioFactura(empresa.Id, proveedor.Id, otroProveedor.Id, pedido.Id, detallePedido.Id, cantidadPedida);
    }
}
