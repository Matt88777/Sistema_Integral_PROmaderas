using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROmaderas.AccesoADatos.Migrations.Negocio
{
    /// <inheritdoc />
    public partial class Ded_MontoTotal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MontoTotal",
                table: "EmpleadoDeduccion",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MontoTotal",
                table: "EmpleadoDeduccion");
        }
    }
}
