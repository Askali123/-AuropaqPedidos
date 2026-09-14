using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;

namespace Api.Tests;

// Datos maestros mínimos (Empresa, Periodo, Categoria, UnidadMedida, Producto, Sede) para que
// cada prueba de flujo tenga su propia Empresa+Periodo y no choque con la unicidad
// Requisicion(EmpresaId, PeriodoId) de otras pruebas.
internal sealed record Escenario(int EmpresaId, int PeriodoId, int ProductoId, int SedeId)
{
    public static async Task<Escenario> CrearAsync(AuropaqPedidosDbContext db, int numero)
    {
        var empresa = new Empresa(numero, $"Empresa de prueba {numero}");
        var periodo = new Periodo(
            numero, 2026, numero % 12 + 1,
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 31),
            new DateTime(2026, 1, 1), new DateTime(2100, 1, 1),
            "ABIERTO");
        var categoria = new Categoria(numero, $"Categoría {numero}");
        var unidadMedida = new UnidadMedida(numero, "UNIDAD", "Unidad");
        var producto = new Producto(numero, $"Producto {numero}", categoria, unidadMedida);
        var sede = new Sede(numero, empresa, $"Sede {numero}");

        db.Empresas.Add(empresa);
        db.Periodos.Add(periodo);
        db.Categorias.Add(categoria);
        db.UnidadesMedida.Add(unidadMedida);
        db.Productos.Add(producto);
        db.Sedes.Add(sede);

        await db.SaveChangesAsync();

        return new Escenario(empresa.Id, periodo.Id, producto.Id, sede.Id);
    }
}
