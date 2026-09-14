using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Prueba de integración contra SQL Server real (misma infraestructura que las demás pruebas de
// Api.Tests: ApiWebApplicationFactory recrea "AuropaqPedidosTests" con EnsureDeleted+Migrate).
// No se puede validar la corrección de la solución de concurrencia con un mock/fake ni con el
// proveedor InMemory de EF Core, porque el mecanismo depende del comportamiento real de locking
// de SQL Server durante un UPDATE (GeneradorDeIdentificadoresEfCore.Siguiente()).
public sealed class GeneradorDeIdentificadoresConcurrenciaTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public GeneradorDeIdentificadoresConcurrenciaTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Llamadas_concurrentes_generan_identificadores_unicos_sin_perder_incrementos()
    {
        const int llamadas = 100;

        // Cada llamada usa su propio scope (y por lo tanto su propio DbContext/conexión),
        // igual que ocurriría con peticiones HTTP concurrentes reales.
        var tareas = Enumerable.Range(0, llamadas).Select(_ => Task.Run(() =>
        {
            using var scope = _factory.Services.CreateScope();
            var generador = scope.ServiceProvider.GetRequiredService<IGeneradorDeIdentificadores>();
            return generador.Siguiente();
        }));

        var ids = await Task.WhenAll(tareas);

        // Únicos: ninguna colisión de Id entre llamadas concurrentes.
        Assert.Equal(llamadas, ids.Distinct().Count());

        // Sin huecos ni repeticiones: la secuencia obtenida es exactamente 1..N (no se perdió
        // ningún incremento por una condición de carrera).
        var esperados = Enumerable.Range(1, llamadas);
        Assert.Equal(esperados, ids.OrderBy(id => id));

        // El contador persistido queda consistente con el número de llamadas realizadas.
        using var scopeVerificacion = _factory.Services.CreateScope();
        var db = scopeVerificacion.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var valorFinal = (await db.Database
            .SqlQuery<int>($"SELECT Valor FROM dbo.ContadorIdentificadores WHERE Id = 1")
            .ToListAsync())
            .Single();
        Assert.Equal(llamadas, valorFinal);
    }
}
