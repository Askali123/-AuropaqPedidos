using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuropaqPedidos.Application.Entregas;
using AuropaqPedidos.Application.Entregas.Dtos;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// Pruebas de atomicidad de AgregarDetalleEntregaUseCase/AnularEntregaUseCase (auditoría: cada
// uno modificaba Entrega y, condicionalmente, PedidoProveedor.Estado en dos SaveChanges
// separados; un fallo entre ambos dejaba el pedido con un Estado que ya no correspondía a sus
// entregas reales). Verifican el comportamiento transaccional REAL contra SQL Server: las
// pruebas de rollback bloquean deliberadamente la fila de PedidoProveedor desde una conexión
// ADO.NET independiente (transacción sin confirmar) y fuerzan un timeout real de comando en la
// segunda escritura de ITransaccionDeEntrega, para comprobar que, aunque la primera escritura
// (Entrega) ya se haya ejecutado dentro de la misma transacción, nada queda persistido si la
// segunda falla. No se usan mocks para esta parte: un mock no puede demostrar que SQL Server
// realmente deshizo el INSERT/UPDATE ya ejecutado.
public sealed class EntregaAtomicidadTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public EntregaAtomicidadTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    // Autorización real agregada 2026-09-17 (RN-063/ADR-066): un único token con los permisos
    // necesarios, autenticado en `_cliente` por defecto — este archivo prueba atomicidad
    // transaccional, no autorización granular.
    private async Task<EscenarioPedidoProveedor> NuevoEscenarioAsync(int numero, int cantidadNecesaria = 100)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var escenario = await EscenarioPedidoProveedor.CrearAsync(db, numero, cantidadNecesaria);

        var token = await AutorizacionHelper.CrearTokenConPermisosAsync(
            _factory, db, numero, escenario.EmpresaId,
            "PEDIDO_CREAR", "PEDIDO_ENVIAR", "ENTREGA_REGISTRAR", "ENTREGA_ANULAR");
        _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return escenario;
    }

    private async Task<(int PedidoId, int DetallePedidoId)> CrearYEnviarPedidoAsync(
        EscenarioPedidoProveedor escenario, string numeroPedido, int cantidadPedida)
    {
        var respuestaCrear = await _cliente.PostAsJsonAsync("/api/v1/pedidos-proveedor", new
        {
            consolidacionId = escenario.ConsolidacionId,
            proveedorId = escenario.ProveedorId,
            numeroPedido
        });
        var pedido = await respuestaCrear.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();

        var respuestaDetalle = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido!.Data.Id}/detalles", new
        {
            detalleConsolidacionId = escenario.DetalleConsolidacionId,
            cantidadPedida
        });
        var conDetalle = await respuestaDetalle.Content.ReadFromJsonAsync<Envoltorio<PedidoProveedorDto>>();
        var detalleId = conDetalle!.Data.Detalles[0].Id;

        // RN-065/D-18 (2026-09-17): distribución completa exigida antes de enviar.
        await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/detalles/{detalleId}/distribuciones", new
        {
            sedeId = escenario.SedeId,
            cantidad = cantidadPedida
        });

        await _cliente.PostAsync($"/api/v1/pedidos-proveedor/{pedido.Data.Id}/enviar", content: null);

        return (pedido.Data.Id, detalleId);
    }

    private async Task<int> CrearEntregaAsync(int pedidoId, string numeroRemision)
    {
        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/pedidos-proveedor/{pedidoId}/entregas", new { numeroRemision });
        var entrega = await respuesta.Content.ReadFromJsonAsync<Envoltorio<EntregaDto>>();
        return entrega!.Data.Id;
    }

    // Bloquea la fila de PedidoProveedor en una transacción ADO.NET aparte, sin confirmarla:
    // cualquier UPDATE posterior de EF Core sobre esa misma fila queda esperando el lock hasta
    // agotar su CommandTimeout (real bloqueo/timeout de SQL Server, no una simulación). Se libera
    // al hacer Dispose (la transacción no confirmada hace ROLLBACK automáticamente).
    private sealed class BloqueoDeFilaPedido : IAsyncDisposable
    {
        private readonly SqlConnection _conexion;
        private readonly SqlTransaction _transaccion;

        private BloqueoDeFilaPedido(SqlConnection conexion, SqlTransaction transaccion)
        {
            _conexion = conexion;
            _transaccion = transaccion;
        }

        public static async Task<BloqueoDeFilaPedido> AdquirirAsync(string cadenaConexion, int pedidoId)
        {
            var conexion = new SqlConnection(cadenaConexion);
            await conexion.OpenAsync();
            var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();

            var comando = conexion.CreateCommand();
            comando.Transaction = transaccion;
            comando.CommandText = "UPDATE PedidosProveedor SET Estado = Estado WHERE Id = @id";
            comando.Parameters.AddWithValue("@id", pedidoId);
            await comando.ExecuteNonQueryAsync();

            return new BloqueoDeFilaPedido(conexion, transaccion);
        }

        public async ValueTask DisposeAsync()
        {
            await _transaccion.DisposeAsync(); // nunca se confirmó -> ROLLBACK, libera el lock.
            await _conexion.DisposeAsync();
        }
    }

    // 1. Operación exitosa -> todos los cambios persisten (Entrega y PedidoProveedor.Estado juntos).
    [Fact]
    public async Task Agregar_detalle_exitoso_persiste_entrega_y_estado_del_pedido_juntos()
    {
        var escenario = await NuevoEscenarioAsync(1);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-AT-001", cantidadPedida: 100);
        var entregaId = await CrearEntregaAsync(pedidoId, "REM-AT-001");

        var respuesta = await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entregaId}/detalles", new
        {
            detallePedidoProveedorId = detalleId,
            cantidadEntregada = 100
        });
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var entregaPersistida = await db.Entregas.Include(e => e.Detalles).SingleAsync(e => e.Id == entregaId);
        var pedidoPersistido = await db.PedidosProveedor.SingleAsync(p => p.Id == pedidoId);

        Assert.Single(entregaPersistida.Detalles);
        Assert.Equal(100, entregaPersistida.Detalles[0].CantidadEntregada);
        Assert.Equal(PedidoProveedorEstado.Entregado, pedidoPersistido.Estado);
    }

    // 2 y 3. Fallo después de escribir Entrega, durante la actualización de PedidoProveedor ->
    // rollback completo. En AgregarDetalleEntregaUseCase solo existe esta única ventana real
    // entre ambas escrituras (la segunda es siempre la de PedidoProveedor), así que un mismo
    // mecanismo real demuestra los dos puntos del pliego de pruebas.
    [Fact]
    public async Task Fallo_al_actualizar_el_pedido_revierte_tambien_la_entrega_ya_escrita()
    {
        var escenario = await NuevoEscenarioAsync(2);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-AT-002", cantidadPedida: 100);
        var entregaId = await CrearEntregaAsync(pedidoId, "REM-AT-002");

        using var scopeLectura = _factory.Services.CreateScope();
        var cadenaConexion = scopeLectura.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>()
            .Database.GetConnectionString()!;

        await using (await BloqueoDeFilaPedido.AdquirirAsync(cadenaConexion, pedidoId))
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            db.Database.SetCommandTimeout(2);
            var useCase = scope.ServiceProvider.GetRequiredService<AgregarDetalleEntregaUseCase>();

            var excepcion = Record.Exception(() =>
                useCase.Ejecutar(entregaId, new AgregarDetalleEntregaRequest(detalleId, 60)));

            Assert.NotNull(excepcion);
        }

        using var scopeVerificacion = _factory.Services.CreateScope();
        var dbVerificacion = scopeVerificacion.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var entregaFinal = await dbVerificacion.Entregas.Include(e => e.Detalles).SingleAsync(e => e.Id == entregaId);
        var pedidoFinal = await dbVerificacion.PedidosProveedor.SingleAsync(p => p.Id == pedidoId);

        // Ni el detalle de Entrega (ya "escrito" dentro de la transacción) ni el nuevo Estado del
        // pedido quedaron persistidos: el rollback deshizo ambos.
        Assert.Empty(entregaFinal.Detalles);
        Assert.Equal(PedidoProveedorEstado.Enviado, pedidoFinal.Estado);
    }

    // 4. Anulación exitosa -> Entrega y estado del PedidoProveedor quedan consistentes.
    [Fact]
    public async Task Anular_exitosa_deja_entrega_y_pedido_consistentes()
    {
        var escenario = await NuevoEscenarioAsync(3);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-AT-003", cantidadPedida: 100);
        var entregaId = await CrearEntregaAsync(pedidoId, "REM-AT-003");
        await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entregaId}/detalles",
            new { detallePedidoProveedorId = detalleId, cantidadEntregada = 60 });

        var respuesta = await _cliente.PostAsync($"/api/v1/entregas/{entregaId}/anular", content: null);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var entregaFinal = await db.Entregas.SingleAsync(e => e.Id == entregaId);
        var pedidoFinal = await db.PedidosProveedor.SingleAsync(p => p.Id == pedidoId);

        Assert.Equal(EntregaEstado.Anulada, entregaFinal.Estado);
        Assert.Equal(PedidoProveedorEstado.Enviado, pedidoFinal.Estado);
    }

    // 5. Fallo durante una anulación -> ningún cambio parcial persiste (ni la Entrega queda
    // Anulada, ni el PedidoProveedor cambia de Estado).
    [Fact]
    public async Task Fallo_durante_una_anulacion_no_deja_ningun_cambio_parcial()
    {
        var escenario = await NuevoEscenarioAsync(4);
        var (pedidoId, detalleId) = await CrearYEnviarPedidoAsync(escenario, "PO-AT-004", cantidadPedida: 100);
        var entregaId = await CrearEntregaAsync(pedidoId, "REM-AT-004");
        await _cliente.PostAsJsonAsync($"/api/v1/entregas/{entregaId}/detalles",
            new { detallePedidoProveedorId = detalleId, cantidadEntregada = 60 });
        // Pedido queda PARCIALMENTE_ENTREGADO; anular la única entrega dispara la actualización
        // condicional de PedidoProveedor.Estado (volvería a ENVIADO si no fallara).

        using var scopeLectura = _factory.Services.CreateScope();
        var cadenaConexion = scopeLectura.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>()
            .Database.GetConnectionString()!;

        await using (await BloqueoDeFilaPedido.AdquirirAsync(cadenaConexion, pedidoId))
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
            db.Database.SetCommandTimeout(2);
            var useCase = scope.ServiceProvider.GetRequiredService<AnularEntregaUseCase>();

            var excepcion = Record.Exception(() => useCase.Ejecutar(entregaId));

            Assert.NotNull(excepcion);
        }

        using var scopeVerificacion = _factory.Services.CreateScope();
        var dbVerificacion = scopeVerificacion.ServiceProvider.GetRequiredService<AuropaqPedidosDbContext>();
        var entregaFinal = await dbVerificacion.Entregas.SingleAsync(e => e.Id == entregaId);
        var pedidoFinal = await dbVerificacion.PedidosProveedor.SingleAsync(p => p.Id == pedidoId);

        Assert.Equal(EntregaEstado.Registrada, entregaFinal.Estado);
        Assert.Equal(PedidoProveedorEstado.ParcialmenteEntregado, pedidoFinal.Estado);
    }

    private sealed record Envoltorio<T>(T Data);

    private sealed record PedidoProveedorDto(int Id, string NumeroPedido, string Estado, IReadOnlyList<DetallePedidoProveedorDto> Detalles);

    private sealed record DetallePedidoProveedorDto(int Id, int CantidadNecesaria, int CantidadPedida);

    private sealed record EntregaDto(int Id, int PedidoProveedorId, string NumeroRemision, string Estado, IReadOnlyList<object> Detalles);
}
