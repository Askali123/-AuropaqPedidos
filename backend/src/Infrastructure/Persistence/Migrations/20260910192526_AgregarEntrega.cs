using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuropaqPedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entregas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PedidoProveedorId = table.Column<int>(type: "int", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroRemision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entregas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entregas_PedidosProveedor_PedidoProveedorId",
                        column: x => x.PedidoProveedorId,
                        principalTable: "PedidosProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesEntrega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DetallePedidoProveedorId = table.Column<int>(type: "int", nullable: false),
                    CantidadEntregada = table.Column<int>(type: "int", nullable: false),
                    EntregaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesEntrega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesEntrega_DetallesPedidoProveedor_DetallePedidoProveedorId",
                        column: x => x.DetallePedidoProveedorId,
                        principalTable: "DetallesPedidoProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesEntrega_Entregas_EntregaId",
                        column: x => x.EntregaId,
                        principalTable: "Entregas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DistribucionesEntrega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    SedeId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    DireccionEntrega = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CiudadEntrega = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactoEntrega = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetalleEntregaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistribucionesEntrega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistribucionesEntrega_DetallesEntrega_DetalleEntregaId",
                        column: x => x.DetalleEntregaId,
                        principalTable: "DetallesEntrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistribucionesEntrega_Sedes_SedeId",
                        column: x => x.SedeId,
                        principalTable: "Sedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesEntrega_DetallePedidoProveedorId",
                table: "DetallesEntrega",
                column: "DetallePedidoProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesEntrega_EntregaId",
                table: "DetallesEntrega",
                column: "EntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_DistribucionesEntrega_DetalleEntregaId",
                table: "DistribucionesEntrega",
                column: "DetalleEntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_DistribucionesEntrega_SedeId",
                table: "DistribucionesEntrega",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_PedidoProveedorId",
                table: "Entregas",
                column: "PedidoProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistribucionesEntrega");

            migrationBuilder.DropTable(
                name: "DetallesEntrega");

            migrationBuilder.DropTable(
                name: "Entregas");
        }
    }
}
