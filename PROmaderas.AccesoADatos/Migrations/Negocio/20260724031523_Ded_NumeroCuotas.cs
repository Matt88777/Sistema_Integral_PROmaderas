using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROmaderas.AccesoADatos.Migrations.Negocio
{
    /// <inheritdoc />
    public partial class Ded_NumeroCuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroCuotas",
                table: "EmpleadoDeduccion",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroCuotas",
                table: "EmpleadoDeduccion");
        }
    }
}
