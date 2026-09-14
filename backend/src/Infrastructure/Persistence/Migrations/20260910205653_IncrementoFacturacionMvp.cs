using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuropaqPedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncrementoFacturacionMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Facturas");

            migrationBuilder.AddColumn<int>(
                name: "PedidoProveedorId",
                table: "Facturas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DetallesFactura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DetallePedidoProveedorId = table.Column<int>(type: "int", nullable: false),
                    CantidadFacturada = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FacturaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesFactura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_DetallesPedidoProveedor_DetallePedidoProveedorId",
                        column: x => x.DetallePedidoProveedorId,
                        principalTable: "DetallesPedidoProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PedidoProveedorId",
                table: "Facturas",
                column: "PedidoProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_DetallePedidoProveedorId",
                table: "DetallesFactura",
                column: "DetallePedidoProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_FacturaId",
                table: "DetallesFactura",
                column: "FacturaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_PedidosProveedor_PedidoProveedorId",
                table: "Facturas",
                column: "PedidoProveedorId",
                principalTable: "PedidosProveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_PedidosProveedor_PedidoProveedorId",
                table: "Facturas");

            migrationBuilder.DropTable(
                name: "DetallesFactura");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_PedidoProveedorId",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "PedidoProveedorId",
                table: "Facturas");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Facturas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Facturas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
