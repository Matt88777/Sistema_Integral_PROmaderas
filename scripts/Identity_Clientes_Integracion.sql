/*
 Script de integración entre Identity y Cliente para Pedidos360.
 Ejecutar después de aplicar el script base de Identity.
*/

USE Pedidos360DB;
GO

IF COL_LENGTH('dbo.Cliente', 'UsuarioIdentityId') IS NULL
BEGIN
    ALTER TABLE dbo.Cliente
    ADD UsuarioIdentityId NVARCHAR(450) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Cliente_UsuarioIdentityId'
      AND object_id = OBJECT_ID('dbo.Cliente')
)
BEGIN
    CREATE UNIQUE INDEX IX_Cliente_UsuarioIdentityId
        ON dbo.Cliente(UsuarioIdentityId)
        WHERE UsuarioIdentityId IS NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Cliente_AspNetUsers_UsuarioIdentityId'
)
BEGIN
    ALTER TABLE dbo.Cliente
    ADD CONSTRAINT FK_Cliente_AspNetUsers_UsuarioIdentityId
        FOREIGN KEY (UsuarioIdentityId) REFERENCES dbo.AspNetUsers(Id)
        ON DELETE SET NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Ventas')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Ventas', 'VENTAS', NEWID());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Operaciones')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Operaciones', 'OPERACIONES', NEWID());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = 'Cliente')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Cliente', 'CLIENTE', NEWID());
END
GO
