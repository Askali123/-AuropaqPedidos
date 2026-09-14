using AuropaqPedidos.Application.Entregas.Abstracciones;
using AuropaqPedidos.Infrastructure.Persistence.Context;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class TransaccionDeEntregaEfCore : ITransaccionDeEntrega
{
    private readonly AuropaqPedidosDbContext _contexto;

    public TransaccionDeEntregaEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    // BeginTransaction() usa la misma conexión/DbContext que EntregaRepositoryEfCore y
    // PedidoProveedorRepositoryEfCore (ambos scoped a la misma petición): sus SaveChanges()
    // dentro de "operacion" se enlistan automáticamente en esta transacción explícita (EF Core lo
    // hace por diseño cuando ya hay una transacción ambiente en el contexto). Si algo falla en
    // cualquier punto, ningún cambio de "operacion" queda persistido.
    public void Ejecutar(Action operacion)
    {
        using var transaccion = _contexto.Database.BeginTransaction();

        try
        {
            operacion();
            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }
}
