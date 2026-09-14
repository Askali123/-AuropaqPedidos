using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class GeneradorDeIdentificadoresEfCore : IGeneradorDeIdentificadores
{
    private readonly AuropaqPedidosDbContext _contexto;

    public GeneradorDeIdentificadoresEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    // La fila (Id = 1) se asegura por migración (ver AsegurarContadorIdentificadorInicial), así
    // que aquí no hay que crearla: solo incrementarla. El UPDATE...OUTPUT es una única sentencia
    // atómica: SQL Server toma un lock exclusivo de fila durante todo el UPDATE, sin importar el
    // nivel de aislamiento, de modo que dos llamadas concurrentes se serializan automáticamente
    // (la segunda espera a que la primera libere el lock y ve el Valor ya incrementado). Esto
    // elimina la carrera de leer-incrementar-guardar sin cambiar el mecanismo ni introducir
    // IDENTITY/SEQUENCE.
    public int Siguiente()
    {
        var siguiente = _contexto.Database
            .SqlQuery<int>($"UPDATE dbo.ContadorIdentificadores SET Valor = Valor + 1 OUTPUT INSERTED.Valor WHERE Id = 1")
            .ToList();

        if (siguiente.Count == 0)
        {
            throw new InvalidOperationException(
                "No existe la fila de ContadorIdentificadores (Id = 1). Verifique que las migraciones se hayan aplicado.");
        }

        return siguiente[0];
    }
}
