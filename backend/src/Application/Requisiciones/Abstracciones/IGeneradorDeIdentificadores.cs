namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

// Requisicion/DetalleRequisicion/DistribucionRequisicion reciben su Id por constructor
// (no hay autoincremento en Domain). Mientras no exista EF Core, algo debe proveer ese Id
// antes de construir la entidad; esta es la abstracción mínima para ese propósito.
// La estrategia definitiva (identity, secuencia, etc.) se decidirá en el bloque de Infrastructure.
public interface IGeneradorDeIdentificadores
{
    int Siguiente();
}
