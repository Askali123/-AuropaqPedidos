using AuropaqPedidos.Application.Entregas.Abstracciones;

namespace Application.Tests.Fakes;

// Los fakes de este proyecto no tienen una base de datos real que pueda fallar a mitad de
// camino, así que aquí "todo o nada" se cumple trivialmente ejecutando la operación directamente.
// El comportamiento real de COMMIT/ROLLBACK contra SQL Server se prueba en Api.Tests
// (TransaccionDeEntregaEfCore), que es donde tiene sentido validarlo.
internal sealed class FakeTransaccionDeEntrega : ITransaccionDeEntrega
{
    public void Ejecutar(Action operacion) => operacion();
}
