using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuropaqPedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProductoProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductosProveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    CodigoProveedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescripcionProveedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoriaProveedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnidadProveedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosProveedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosProveedores_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductosProveedores_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductosProveedores_ProductoId_ProveedorId",
                table: "ProductosProveedores",
                columns: new[] { "ProductoId", "ProveedorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosProveedores_ProveedorId",
                table: "ProductosProveedores",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductosProveedores");
        }
    }
}
