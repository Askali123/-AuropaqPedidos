using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuropaqPedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConsolidacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consolidaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PeriodoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consolidaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consolidaciones_Periodos_PeriodoId",
                        column: x => x.PeriodoId,
                        principalTable: "Periodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesConsolidacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    ConsolidacionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesConsolidacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesConsolidacion_Consolidaciones_ConsolidacionId",
                        column: x => x.ConsolidacionId,
                        principalTable: "Consolidaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesConsolidacion_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesConsolidacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DetalleRequisicionOrigenId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    DetalleConsolidacionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesConsolidacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesConsolidacion_DetallesConsolidacion_DetalleConsolidacionId",
                        column: x => x.DetalleConsolidacionId,
                        principalTable: "DetallesConsolidacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesConsolidacion_DetallesRequisicion_DetalleRequisicionOrigenId",
                        column: x => x.DetalleRequisicionOrigenId,
                        principalTable: "DetallesRequisicion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesConsolidacion_DetalleConsolidacionId",
                table: "AsignacionesConsolidacion",
                column: "DetalleConsolidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesConsolidacion_DetalleRequisicionOrigenId",
                table: "AsignacionesConsolidacion",
                column: "DetalleRequisicionOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_Consolidaciones_PeriodoId",
                table: "Consolidaciones",
                column: "PeriodoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesConsolidacion_ConsolidacionId",
                table: "DetallesConsolidacion",
                column: "ConsolidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesConsolidacion_ProductoId",
                table: "DetallesConsolidacion",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesConsolidacion");

            migrationBuilder.DropTable(
                name: "DetallesConsolidacion");

            migrationBuilder.DropTable(
                name: "Consolidaciones");
        }
    }
}
