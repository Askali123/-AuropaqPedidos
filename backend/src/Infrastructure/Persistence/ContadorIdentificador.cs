namespace AuropaqPedidos.Infrastructure.Persistence;

// Mecanismo interno de Infrastructure (no es una entidad de Domain) para resolver el problema
// de que Requisicion/DetalleRequisicion/DistribucionRequisicion reciben su Id por constructor,
// sin autoincremento. Fila única (Id = 1), asegurada por la migración
// AsegurarContadorIdentificadorInicial. GeneradorDeIdentificadoresEfCore la incrementa mediante
// una sentencia UPDATE...OUTPUT atómica (no a través del change tracker de esta entidad), por lo
// que esta clase solo describe la forma de la tabla para el modelo de EF Core/migraciones.
internal sealed class ContadorIdentificador
{
    public int Id { get; private set; } = 1;
    public int Valor { get; private set; }

    private ContadorIdentificador()
    {
    }

    public ContadorIdentificador(int valorInicial)
    {
        Valor = valorInicial;
    }
}
