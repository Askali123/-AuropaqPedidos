namespace AuropaqPedidos.Application.Entregas.Abstracciones;

// AgregarDetalleEntregaUseCase y AnularEntregaUseCase modifican Entrega y, condicionalmente,
// PedidoProveedor.Estado a través de dos repositorios distintos, cada uno con su propio punto de
// persistencia. Sin esta abstracción, un fallo entre ambos pasos deja el pedido con un Estado que
// ya no corresponde a sus entregas reales (auditoría: problema crítico de atomicidad). Application
// solo declara que necesita "todo o nada"; el mecanismo concreto (transacción de EF Core/SQL
// Server) es responsabilidad de Infrastructure.
public interface ITransaccionDeEntrega
{
    void Ejecutar(Action operacion);
}
