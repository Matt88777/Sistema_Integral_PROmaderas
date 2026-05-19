/*
 ═══════════════════════════════════════════════════════════════════════════════
  SP_CrearUsuarioAdminIdentity
  Crea (o actualiza) un usuario con rol Admin en ASP.NET Core Identity.
 ═══════════════════════════════════════════════════════════════════════════════
 COMO USAR
 ───────────────────────────────────────────────────────────────────────────────
    Nuevo SP para crear usuarios de tipo Admin
    En terminal 
    Crear el SP sqlcmd -S localhost -d Pedidos360DB -E -Q "SET NOCOUNT ON; SELECT name FROM sys.procedures WHERE name = 'SP_CrearUsuarioAdminIdentity';"

    Ejemplo de uso: 
    powershell -ExecutionPolicy Bypass -File .\Crear-AdminIdentity.ps1 `
        -Correo "andresadmin@pedidos360.com" `
        -NombreCompleto "Andres Gutierrez"
    Luego les pide crear contraseña. 
 ───────────────────────────────────────────────────────────────────────────────
*/

USE Pedidos360DB;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.SP_CrearUsuarioAdminIdentity
	@Correo NVARCHAR(256),
	@NombreCompleto NVARCHAR(200),
	@PasswordHash NVARCHAR(MAX),
	@Telefono NVARCHAR(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF @Correo IS NULL OR LTRIM(RTRIM(@Correo)) = ''
	BEGIN
		RAISERROR('El correo es requerido.', 16, 1);
		RETURN;
	END

	IF @PasswordHash IS NULL OR LTRIM(RTRIM(@PasswordHash)) = ''
	BEGIN
		RAISERROR('El PasswordHash es requerido.', 16, 1);
		RETURN;
	END

	DECLARE @RolAdminId NVARCHAR(450);
	DECLARE @UsuarioId NVARCHAR(450);

	-- Garantiza la existencia del rol Admin
	SELECT @RolAdminId = Id
	FROM dbo.AspNetRoles
	WHERE NormalizedName = 'ADMIN';

	IF @RolAdminId IS NULL
	BEGIN
		SET @RolAdminId = CONVERT(NVARCHAR(450), NEWID());

		INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
		VALUES (@RolAdminId, 'Admin', 'ADMIN', CONVERT(NVARCHAR(36), NEWID()));
	END

	SELECT @UsuarioId = Id
	FROM dbo.AspNetUsers
	WHERE NormalizedEmail = UPPER(@Correo);

	IF @UsuarioId IS NULL
	BEGIN
		SET @UsuarioId = CONVERT(NVARCHAR(450), NEWID());

		INSERT INTO dbo.AspNetUsers
		(
			Id,
			UserName,
			NormalizedUserName,
			Email,
			NormalizedEmail,
			EmailConfirmed,
			PasswordHash,
			SecurityStamp,
			ConcurrencyStamp,
			PhoneNumber,
			PhoneNumberConfirmed,
			TwoFactorEnabled,
			LockoutEnd,
			LockoutEnabled,
			AccessFailedCount,
			NombreCompleto
		)
		VALUES
		(
			@UsuarioId,
			@Correo,
			UPPER(@Correo),
			@Correo,
			UPPER(@Correo),
			1,
			@PasswordHash,
			CONVERT(NVARCHAR(36), NEWID()),
			CONVERT(NVARCHAR(36), NEWID()),
			@Telefono,
			0,
			0,
			NULL,
			1,
			0,
			ISNULL(@NombreCompleto, '')
		);
	END
	ELSE
	BEGIN
		UPDATE dbo.AspNetUsers
		SET
			NombreCompleto = ISNULL(@NombreCompleto, NombreCompleto),
			PhoneNumber = ISNULL(@Telefono, PhoneNumber)
		WHERE Id = @UsuarioId;
	END

	IF NOT EXISTS (
		SELECT 1
		FROM dbo.AspNetUserRoles
		WHERE UserId = @UsuarioId
		  AND RoleId = @RolAdminId
	)
	BEGIN
		INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
		VALUES (@UsuarioId, @RolAdminId);
	END
END
GO
