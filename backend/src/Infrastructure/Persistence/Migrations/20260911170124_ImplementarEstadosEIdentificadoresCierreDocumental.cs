using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuropaqPedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImplementarEstadosEIdentificadoresCierreDocumental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PedidosProveedor_ProveedorId",
                table: "PedidosProveedor");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_ProveedorId",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Entregas_PedidoProveedorId",
                table: "Entregas");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroPedido",
                table: "PedidosProveedor",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroFactura",
                table: "Facturas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroRemision",
                table: "Entregas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosProveedor_ProveedorId_NumeroPedido",
                table: "PedidosProveedor",
                columns: new[] { "ProveedorId", "NumeroPedido" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ProveedorId_NumeroFactura",
                table: "Facturas",
                columns: new[] { "ProveedorId", "NumeroFactura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_PedidoProveedorId_NumeroRemision",
                table: "Entregas",
                columns: new[] { "PedidoProveedorId", "NumeroRemision" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PedidosProveedor_ProveedorId_NumeroPedido",
                table: "PedidosProveedor");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_ProveedorId_NumeroFactura",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Entregas_PedidoProveedorId_NumeroRemision",
                table: "Entregas");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroPedido",
                table: "PedidosProveedor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroFactura",
                table: "Facturas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroRemision",
                table: "Entregas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosProveedor_ProveedorId",
                table: "PedidosProveedor",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ProveedorId",
                table: "Facturas",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_PedidoProveedorId",
                table: "Entregas",
                column: "PedidoProveedorId");
        }
    }
}
