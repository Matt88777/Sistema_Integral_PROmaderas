using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROmaderas.AccesoADatos.Migrations.Negocio
{
	public partial class PLA_HU_018_014_013_Sprint4 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// PLA-HU-014: completar la tabla Incapacidad existente.
			// Se agrega inicialmente nullable para conservar los registros históricos.
			migrationBuilder.AddColumn<string>(
				name: "NumeroCertificado",
				table: "Incapacidad",
				type: "nvarchar(100)",
				maxLength: 100,
				nullable: true);

			migrationBuilder.Sql(
				"""
                UPDATE dbo.Incapacidad
                   SET NumeroCertificado = CONCAT('LEGACY-', IdIncapacidad)
                 WHERE NumeroCertificado IS NULL
                    OR LTRIM(RTRIM(NumeroCertificado)) = '';
                """);

			migrationBuilder.AlterColumn<string>(
				name: "NumeroCertificado",
				table: "Incapacidad",
				type: "nvarchar(100)",
				maxLength: 100,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(100)",
				oldMaxLength: 100,
				oldNullable: true);

			migrationBuilder.AddColumn<string>(
				name: "EntidadEmisora",
				table: "Incapacidad",
				type: "nvarchar(50)",
				maxLength: 50,
				nullable: false,
				defaultValue: "CCSS");

			migrationBuilder.AddColumn<bool>(
				name: "Activa",
				table: "Incapacidad",
				type: "bit",
				nullable: false,
				defaultValue: true);

			migrationBuilder.AddColumn<DateTime>(
				name: "FechaRegistro",
				table: "Incapacidad",
				type: "datetime",
				nullable: false,
				defaultValueSql: "GETDATE()");

			migrationBuilder.CreateIndex(
				name: "IX_Incapacidad_NumeroCertificado",
				table: "Incapacidad",
				column: "NumeroCertificado",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_Incapacidad_IdEmpleado_FechaInicio_FechaFin_Activa",
				table: "Incapacidad",
				columns: new[] { "IdEmpleado", "FechaInicio", "FechaFin", "Activa" });

			// PLA-HU-018: resultado de la validación de cobertura en planilla.
			migrationBuilder.AddColumn<bool>(
				name: "TienePolizaINSVigente",
				table: "PlanillaDetalle",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<string>(
				name: "AdvertenciaPolizaINS",
				table: "PlanillaDetalle",
				type: "nvarchar(300)",
				maxLength: 300,
				nullable: true);

			// PLA-HU-014: desglose monetario de la incapacidad.
			migrationBuilder.AddColumn<decimal>(
				name: "MontoPagoPatronalIncapacidad",
				table: "PlanillaDetalle",
				type: "decimal(18,2)",
				nullable: false,
				defaultValue: 0m);

			migrationBuilder.AddColumn<decimal>(
				name: "RebajoIncapacidad",
				table: "PlanillaDetalle",
				type: "decimal(18,2)",
				nullable: false,
				defaultValue: 0m);

			migrationBuilder.AddColumn<string>(
				name: "DetalleIncapacidad",
				table: "PlanillaDetalle",
				type: "nvarchar(300)",
				maxLength: 300,
				nullable: true);

			// PLA-HU-013: explicación del cálculo de vacaciones.
			migrationBuilder.AddColumn<string>(
				name: "DetalleVacaciones",
				table: "PlanillaDetalle",
				type: "nvarchar(300)",
				maxLength: 300,
				nullable: true);

			// Parámetros que no estaban incluidos en el script Sprint 4.
			migrationBuilder.Sql(
				"""
                IF NOT EXISTS (
                    SELECT 1 FROM dbo.ParametroPlanilla
                    WHERE NombreParametro = 'IncapacidadCCSSDiasPatrono' AND Estado = 1)
                    INSERT INTO dbo.ParametroPlanilla
                        (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
                    VALUES
                        ('IncapacidadCCSSDiasPatrono', 3.0000, '2026-01-01', NULL, 1);

                IF NOT EXISTS (
                    SELECT 1 FROM dbo.ParametroPlanilla
                    WHERE NombreParametro = 'IncapacidadINSDiasPatrono' AND Estado = 1)
                    INSERT INTO dbo.ParametroPlanilla
                        (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
                    VALUES
                        ('IncapacidadINSDiasPatrono', 0.0000, '2026-01-01', NULL, 1);

                IF NOT EXISTS (
                    SELECT 1 FROM dbo.ParametroPlanilla
                    WHERE NombreParametro = 'IncapacidadMaternidadDiasPatrono' AND Estado = 1)
                    INSERT INTO dbo.ParametroPlanilla
                        (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
                    VALUES
                        ('IncapacidadMaternidadDiasPatrono', 9999.0000, '2026-01-01', NULL, 1);

                IF NOT EXISTS (
                    SELECT 1 FROM dbo.ParametroPlanilla
                    WHERE NombreParametro = 'IncapacidadMaternidadPorcPatrono' AND Estado = 1)
                    INSERT INTO dbo.ParametroPlanilla
                        (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
                    VALUES
                        ('IncapacidadMaternidadPorcPatrono', 50.0000, '2026-01-01', NULL, 1);
                """);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Incapacidad_NumeroCertificado",
				table: "Incapacidad");

			migrationBuilder.DropIndex(
				name: "IX_Incapacidad_IdEmpleado_FechaInicio_FechaFin_Activa",
				table: "Incapacidad");

			migrationBuilder.DropColumn(name: "NumeroCertificado", table: "Incapacidad");
			migrationBuilder.DropColumn(name: "EntidadEmisora", table: "Incapacidad");
			migrationBuilder.DropColumn(name: "Activa", table: "Incapacidad");
			migrationBuilder.DropColumn(name: "FechaRegistro", table: "Incapacidad");

			migrationBuilder.DropColumn(name: "TienePolizaINSVigente", table: "PlanillaDetalle");
			migrationBuilder.DropColumn(name: "AdvertenciaPolizaINS", table: "PlanillaDetalle");
			migrationBuilder.DropColumn(name: "MontoPagoPatronalIncapacidad", table: "PlanillaDetalle");
			migrationBuilder.DropColumn(name: "RebajoIncapacidad", table: "PlanillaDetalle");
			migrationBuilder.DropColumn(name: "DetalleIncapacidad", table: "PlanillaDetalle");
			migrationBuilder.DropColumn(name: "DetalleVacaciones", table: "PlanillaDetalle");

			migrationBuilder.Sql(
				"""
                DELETE FROM dbo.ParametroPlanilla
                 WHERE NombreParametro IN (
                    'IncapacidadCCSSDiasPatrono',
                    'IncapacidadINSDiasPatrono',
                    'IncapacidadMaternidadDiasPatrono',
                    'IncapacidadMaternidadPorcPatrono')
                   AND FechaInicio = '2026-01-01';
                """);
		}
	}
}