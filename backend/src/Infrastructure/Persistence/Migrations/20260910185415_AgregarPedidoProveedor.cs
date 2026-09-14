using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuropaqPedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPedidoProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contacto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PedidosProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ConsolidacionId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    NumeroPedido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaPedido = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntregaEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosProveedor_Consolidaciones_ConsolidacionId",
                        column: x => x.ConsolidacionId,
                        principalTable: "Consolidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosProveedor_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPedidoProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    CantidadNecesaria = table.Column<int>(type: "int", nullable: false),
                    CantidadPedida = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PedidoProveedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPedidoProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesPedidoProveedor_PedidosProveedor_PedidoProveedorId",
                        column: x => x.PedidoProveedorId,
                        principalTable: "PedidosProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesPedidoProveedor_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DistribucionesPedido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    SedeId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    DetallePedidoProveedorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistribucionesPedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistribucionesPedido_DetallesPedidoProveedor_DetallePedidoProveedorId",
                        column: x => x.DetallePedidoProveedorId,
                        principalTable: "DetallesPedidoProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistribucionesPedido_Sedes_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedidoProveedor_PedidoProveedorId",
                table: "DetallesPedidoProveedor",
                column: "PedidoProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedidoProveedor_ProductoId",
                table: "DetallesPedidoProveedor",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DistribucionesPedido_DetallePedidoProveedorId",
                table: "DistribucionesPedido",
                column: "DetallePedidoProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DistribucionesPedido_SedeId",
                table: "DistribucionesPedido",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosProveedor_ConsolidacionId",
                table: "PedidosProveedor",
                column: "ConsolidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosProveedor_ProveedorId",
                table: "PedidosProveedor",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistribucionesPedido");

            migrationBuilder.DropTable(
                name: "DetallesPedidoProveedor");

            migrationBuilder.DropTable(
                name: "PedidosProveedor");

            migrationBuilder.DropTable(
                name: "Proveedores");
        }
    }
}
