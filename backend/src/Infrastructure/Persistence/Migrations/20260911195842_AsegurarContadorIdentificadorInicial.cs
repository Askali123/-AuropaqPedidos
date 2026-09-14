using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuropaqPedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AsegurarContadorIdentificadorInicial : Migration
    {
        // Asegura (idempotente) la fila única que GeneradorDeIdentificadoresEfCore incrementa de
        // forma atómica (UPDATE...OUTPUT). Sin esta fila, el primer Siguiente() de la aplicación
        // tendría que crearla en tiempo de ejecución, lo cual era en sí mismo una carrera entre
        // las dos primeras llamadas concurrentes. Si la fila ya existe (bases ya usadas antes de
        // esta migración) no se toca su Valor, para no retroceder el contador.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM dbo.ContadorIdentificadores WHERE Id = 1)
                    INSERT INTO dbo.ContadorIdentificadores (Id, Valor) VALUES (1, 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Solo revierte la fila si nadie la usó todavía (Valor sigue en 0 tal como la dejó
            // el Up de esta migración); si ya se generaron identificadores, no se elimina.
            migrationBuilder.Sql(
                """
                DELETE FROM dbo.ContadorIdentificadores WHERE Id = 1 AND Valor = 0;
                """);
        }
    }
}
