using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;

namespace Api.Tests;

// Grafo mínimo Empresa→Periodo→Producto→Requisicion(Aprobada)→Consolidacion→Proveedor + Sede,
// necesario para probar PedidosProveedorController por HTTP (cierre documental 2026-09-11,
// D-01 a D-13). A diferencia de EscenarioFactura, el PedidoProveedor NO se siembra aquí: se crea
// a través de la Api (POST /api/v1/pedidos-proveedor), porque ya existe controller propio.
internal sealed record EscenarioPedidoProveedor(
    int EmpresaId, int ConsolidacionId, int DetalleConsolidacionId, int ProveedorId, int SedeId, int CantidadNecesaria)
{
    public static async Task<EscenarioPedidoProveedor> CrearAsync(AuropaqPedidosDbContext db, int numero, int cantidadNecesaria = 85)
    {
        var fecha = new DateTime(2026, 9, 2);

        var empresa = new Empresa(numero, $"Empresa de prueba {numero}");
        // Anio/Mes se derivan de "numero" para que cada escenario tenga un Periodo distinto
        // (UNIQUE(Anio, Mes)) sin colisionar al superar 12 escenarios en el mismo archivo de
        // pruebas (antes "numero % 12 + 1" con Anio fijo repetía Mes cada 12 llamadas).
        var periodo = new Periodo(
            numero, 2026 + (numero - 1) / 12, (numero - 1) % 12 + 1,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
            "ABIERTO");
        var categoria = new Categoria(numero, $"Categoría {numero}");
        var unidadMedida = new UnidadMedida(numero, "UNIDAD", "Unidad");
        var producto = new Producto(numero, $"Producto {numero}", categoria, unidadMedida);
        var sede = new Sede(numero, empresa, $"Sede {numero}");

        var requisicion = new Requisicion(numero, empresa, periodo, usuarioCreacionId: 1, fecha);
        var detalleReq = requisicion.AgregarDetalle(numero, producto, cantidadNecesaria);
        requisicion.AgregarDistribucion(numero, detalleReq, sede, cantidadNecesaria);
        requisicion.Enviar(1, fecha);
        requisicion.IniciarRevision(1, fecha);
        requisicion.Aprobar(1, fecha);

        var consolidacion = new Consolidacion(numero, periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: fecha);
        consolidacion.AgregarAsignacion(numero, numero, requisicion, detalleReq, cantidadNecesaria);

        var proveedor = new Proveedor(numero, $"Proveedor {numero}");

        db.Empresas.Add(empresa);
        db.Periodos.Add(periodo);
        db.Categorias.Add(categoria);
        db.UnidadesMedida.Add(unidadMedida);
        db.Productos.Add(producto);
        db.Sedes.Add(sede);
        db.Requisiciones.Add(requisicion);
        db.Consolidaciones.Add(consolidacion);
        db.Proveedores.Add(proveedor);

        await db.SaveChangesAsync();

        return new EscenarioPedidoProveedor(empresa.Id, consolidacion.Id, consolidacion.Detalles[0].Id, proveedor.Id, sede.Id, cantidadNecesaria);
    }
}
