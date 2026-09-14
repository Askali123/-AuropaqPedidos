using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuropaqPedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidacionRequisicionUnica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsignacionesConsolidacion_DetalleRequisicionOrigenId",
                table: "AsignacionesConsolidacion");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesConsolidacion_DetalleRequisicionOrigenId",
                table: "AsignacionesConsolidacion",
                column: "DetalleRequisicionOrigenId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AsignacionesConsolidacion_DetalleRequisicionOrigenId",
                table: "AsignacionesConsolidacion");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesConsolidacion_DetalleRequisicionOrigenId",
                table: "AsignacionesConsolidacion",
                column: "DetalleRequisicionOrigenId");
        }
    }
}
