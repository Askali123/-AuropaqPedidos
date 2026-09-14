using Xunit;

// RequisicionesFlujoTests y FacturasFlujoTests comparten la misma base de datos física de
// pruebas ("AuropaqPedidosTests", ApiWebApplicationFactory) y cada una la recrea
// (EnsureDeletedAsync + MigrateAsync) al inicializarse. Sin esto, xUnit ejecutaría ambas clases
// de prueba en paralelo por defecto (cada IClassFixture<ApiWebApplicationFactory> es su propia
// colección), lo que corrompería la base de datos compartida entre ambas.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
