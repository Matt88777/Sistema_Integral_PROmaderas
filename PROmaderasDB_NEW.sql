CREATE DATABASE PROmaderasDB_NEW;
GO

USE PROmaderasDB_NEW;
GO

/* =========================
   SEGURIDAD Y USUARIOS
========================= */

CREATE TABLE Rol (
    IdRol INT IDENTITY(1,1) PRIMARY KEY,
    NombreRol NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(200),
    Estado BIT NOT NULL DEFAULT 1
);

CREATE TABLE Departamento (
    IdDepartamento INT IDENTITY(1,1) PRIMARY KEY,
    NombreDepartamento NVARCHAR(100) NOT NULL UNIQUE,
    Estado BIT NOT NULL DEFAULT 1
);

CREATE TABLE Puesto (
    IdPuesto INT IDENTITY(1,1) PRIMARY KEY,
    NombrePuesto NVARCHAR(100) NOT NULL,
    IdDepartamento INT NOT NULL,
    Estado BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Puesto_Departamento
        FOREIGN KEY (IdDepartamento) REFERENCES Departamento(IdDepartamento)
);

CREATE TABLE Empleado (
    IdEmpleado INT IDENTITY(1,1) PRIMARY KEY,
    Cedula NVARCHAR(25) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    PrimerApellido NVARCHAR(100) NOT NULL,
    SegundoApellido NVARCHAR(100),
    Telefono NVARCHAR(25),
    Correo NVARCHAR(150),
    Direccion NVARCHAR(250),
    FechaIngreso DATE NOT NULL,
    IdPuesto INT NOT NULL,
    Estado BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Empleado_Puesto
        FOREIGN KEY (IdPuesto) REFERENCES Puesto(IdPuesto)
);

CREATE TABLE Usuario (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    IdEmpleado INT NOT NULL,
    IdRol INT NOT NULL,
    NombreUsuario NVARCHAR(100) NOT NULL UNIQUE,
    Correo NVARCHAR(150) NOT NULL UNIQUE,
    ContrasenaHash NVARCHAR(500) NOT NULL,
    Estado BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    UltimoAcceso DATETIME NULL,
    CONSTRAINT FK_Usuario_Empleado
        FOREIGN KEY (IdEmpleado) REFERENCES Empleado(IdEmpleado),
    CONSTRAINT FK_Usuario_Rol
        FOREIGN KEY (IdRol) REFERENCES Rol(IdRol)
);

/* =========================
   CLIENTES
========================= */

CREATE TABLE Cliente (
    IdCliente INT IDENTITY(1,1) PRIMARY KEY,
    CedulaJuridica NVARCHAR(30) NOT NULL UNIQUE,
    NombreCliente NVARCHAR(150) NOT NULL,
    Telefono NVARCHAR(25),
    Correo NVARCHAR(150),
    Direccion NVARCHAR(250),
    CondicionPago NVARCHAR(100),
    Exonerado BIT NOT NULL DEFAULT 0,
    PorcentajeExoneracion DECIMAL(5,2) NOT NULL DEFAULT 0,
    Estado BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

/* =========================
   INVENTARIO Y PRODUCCIÓN
========================= */

CREATE TABLE TipoTarima (
    IdTipoTarima INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(150) NOT NULL,
    Medida NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(250),
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    StockMinimo INT NOT NULL DEFAULT 0,
    Estado BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Produccion (
    IdProduccion INT IDENTITY(1,1) PRIMARY KEY,
    IdTipoTarima INT NOT NULL,
    IdUsuarioRegistro INT NOT NULL,
    FechaProduccion DATE NOT NULL,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    Observacion NVARCHAR(250),
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Produccion_TipoTarima
        FOREIGN KEY (IdTipoTarima) REFERENCES TipoTarima(IdTipoTarima),
    CONSTRAINT FK_Produccion_Usuario
        FOREIGN KEY (IdUsuarioRegistro) REFERENCES Usuario(IdUsuario)
);

CREATE TABLE InventarioMovimiento (
    IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
    IdTipoTarima INT NOT NULL,
    IdUsuarioRegistro INT NOT NULL,
    TipoMovimiento NVARCHAR(30) NOT NULL,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    FechaMovimiento DATETIME NOT NULL DEFAULT GETDATE(),
    Motivo NVARCHAR(250),
    IdProduccion INT NULL,
    IdOrdenCompra INT NULL,
    CONSTRAINT FK_InventarioMovimiento_TipoTarima
        FOREIGN KEY (IdTipoTarima) REFERENCES TipoTarima(IdTipoTarima),
    CONSTRAINT FK_InventarioMovimiento_Usuario
        FOREIGN KEY (IdUsuarioRegistro) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_InventarioMovimiento_Produccion
        FOREIGN KEY (IdProduccion) REFERENCES Produccion(IdProduccion),
    CONSTRAINT CK_InventarioMovimiento_Tipo
        CHECK (TipoMovimiento IN ('Entrada', 'Salida', 'AjusteEntrada', 'AjusteSalida'))
);

/* =========================
   ÓRDENES DE COMPRA
========================= */

CREATE TABLE OrdenCompra (
    IdOrdenCompra INT IDENTITY(1,1) PRIMARY KEY,
    NumeroOrden NVARCHAR(50) NOT NULL UNIQUE,
    IdCliente INT NOT NULL,
    IdVendedor INT NOT NULL,
    FechaOrden DATETIME NOT NULL DEFAULT GETDATE(),
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    Observacion NVARCHAR(250),
    Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL DEFAULT 0,
    Activa BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_OrdenCompra_Cliente
        FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente),
    CONSTRAINT FK_OrdenCompra_Vendedor
        FOREIGN KEY (IdVendedor) REFERENCES Usuario(IdUsuario),
    CONSTRAINT CK_OrdenCompra_Estado
        CHECK (Estado IN ('Pendiente', 'En Produccion', 'Lista para Entrega', 'Entregada', 'Cancelada'))
);

ALTER TABLE InventarioMovimiento
ADD CONSTRAINT FK_InventarioMovimiento_OrdenCompra
FOREIGN KEY (IdOrdenCompra) REFERENCES OrdenCompra(IdOrdenCompra);

CREATE TABLE OrdenCompraDetalle (
    IdOrdenCompraDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IdOrdenCompra INT NOT NULL,
    IdTipoTarima INT NOT NULL,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrdenCompraDetalle_Orden
        FOREIGN KEY (IdOrdenCompra) REFERENCES OrdenCompra(IdOrdenCompra),
    CONSTRAINT FK_OrdenCompraDetalle_TipoTarima
        FOREIGN KEY (IdTipoTarima) REFERENCES TipoTarima(IdTipoTarima)
);

CREATE TABLE HistorialEstadoOrden (
    IdHistorialEstadoOrden INT IDENTITY(1,1) PRIMARY KEY,
    IdOrdenCompra INT NOT NULL,
    EstadoAnterior NVARCHAR(50),
    EstadoNuevo NVARCHAR(50) NOT NULL,
    IdUsuarioCambio INT NOT NULL,
    FechaCambio DATETIME NOT NULL DEFAULT GETDATE(),
    Observacion NVARCHAR(250),
    CONSTRAINT FK_HistorialEstadoOrden_Orden
        FOREIGN KEY (IdOrdenCompra) REFERENCES OrdenCompra(IdOrdenCompra),
    CONSTRAINT FK_HistorialEstadoOrden_Usuario
        FOREIGN KEY (IdUsuarioCambio) REFERENCES Usuario(IdUsuario)
);

/* =========================
   FACTURACIÓN
========================= */

CREATE TABLE Factura (
    IdFactura INT IDENTITY(1,1) PRIMARY KEY,
    NumeroFactura NVARCHAR(50) NOT NULL UNIQUE,
    IdOrdenCompra INT NOT NULL,
    IdCliente INT NOT NULL,
    IdUsuarioEmisor INT NOT NULL,
    FechaEmision DATETIME NOT NULL DEFAULT GETDATE(),
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Emitida',
    Subtotal DECIMAL(18,2) NOT NULL,
    Impuesto DECIMAL(18,2) NOT NULL,
    Exoneracion DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    SaldoPendiente DECIMAL(18,2) NOT NULL,
    Activa BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Factura_Orden
        FOREIGN KEY (IdOrdenCompra) REFERENCES OrdenCompra(IdOrdenCompra),
    CONSTRAINT FK_Factura_Cliente
        FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente),
    CONSTRAINT FK_Factura_Usuario
        FOREIGN KEY (IdUsuarioEmisor) REFERENCES Usuario(IdUsuario),
    CONSTRAINT CK_Factura_Estado
        CHECK (Estado IN ('Emitida', 'Pendiente de Pago', 'Pagada', 'Anulada'))
);

CREATE TABLE PagoFactura (
    IdPagoFactura INT IDENTITY(1,1) PRIMARY KEY,
    IdFactura INT NOT NULL,
    FechaPago DATETIME NOT NULL DEFAULT GETDATE(),
    Monto DECIMAL(18,2) NOT NULL CHECK (Monto > 0),
    FormaPago NVARCHAR(50) NOT NULL,
    Referencia NVARCHAR(100),
    IdUsuarioRegistro INT NOT NULL,
    CONSTRAINT FK_PagoFactura_Factura
        FOREIGN KEY (IdFactura) REFERENCES Factura(IdFactura),
    CONSTRAINT FK_PagoFactura_Usuario
        FOREIGN KEY (IdUsuarioRegistro) REFERENCES Usuario(IdUsuario)
);

CREATE TABLE HistorialEstadoFactura (
    IdHistorialEstadoFactura INT IDENTITY(1,1) PRIMARY KEY,
    IdFactura INT NOT NULL,
    EstadoAnterior NVARCHAR(50),
    EstadoNuevo NVARCHAR(50) NOT NULL,
    IdUsuarioCambio INT NOT NULL,
    FechaCambio DATETIME NOT NULL DEFAULT GETDATE(),
    Observacion NVARCHAR(250),
    CONSTRAINT FK_HistorialEstadoFactura_Factura
        FOREIGN KEY (IdFactura) REFERENCES Factura(IdFactura),
    CONSTRAINT FK_HistorialEstadoFactura_Usuario
        FOREIGN KEY (IdUsuarioCambio) REFERENCES Usuario(IdUsuario)
);

/* =========================
   PLANILLA
========================= */

CREATE TABLE HistorialSalario (
    IdHistorialSalario INT IDENTITY(1,1) PRIMARY KEY,
    IdEmpleado INT NOT NULL,
    SalarioBase DECIMAL(18,2) NOT NULL,
    TipoPago NVARCHAR(50) NOT NULL,
    JornadaLaboral NVARCHAR(50) NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NULL,
    IdUsuarioRegistro INT NOT NULL,
    CONSTRAINT FK_HistorialSalario_Empleado
        FOREIGN KEY (IdEmpleado) REFERENCES Empleado(IdEmpleado),
    CONSTRAINT FK_HistorialSalario_Usuario
        FOREIGN KEY (IdUsuarioRegistro) REFERENCES Usuario(IdUsuario)
);

CREATE TABLE PlanillaPeriodo (
    IdPlanillaPeriodo INT IDENTITY(1,1) PRIMARY KEY,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    TipoPeriodo NVARCHAR(50) NOT NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Borrador',
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    IdUsuarioCreacion INT NOT NULL,
    CONSTRAINT FK_PlanillaPeriodo_Usuario
        FOREIGN KEY (IdUsuarioCreacion) REFERENCES Usuario(IdUsuario),
    CONSTRAINT CK_PlanillaPeriodo_Estado
        CHECK (Estado IN ('Borrador', 'Revisada', 'Aprobada', 'Pagada'))
);

CREATE TABLE TipoDeduccion (
    IdTipoDeduccion INT IDENTITY(1,1) PRIMARY KEY,
    NombreDeduccion NVARCHAR(100) NOT NULL,
    Tipo NVARCHAR(50) NOT NULL,
    Porcentaje DECIMAL(5,2) NULL,
    MontoFijo DECIMAL(18,2) NULL,
    EsLegal BIT NOT NULL DEFAULT 0,
    Estado BIT NOT NULL DEFAULT 1
);

CREATE TABLE PlanillaDetalle (
    IdPlanillaDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IdPlanillaPeriodo INT NOT NULL,
    IdEmpleado INT NOT NULL,
    HorasOrdinarias DECIMAL(10,2) NOT NULL DEFAULT 0,
    HorasExtra DECIMAL(10,2) NOT NULL DEFAULT 0,
    SalarioBase DECIMAL(18,2) NOT NULL,
    MontoHorasExtra DECIMAL(18,2) NOT NULL DEFAULT 0,
    SalarioBruto DECIMAL(18,2) NOT NULL,
    TotalDeducciones DECIMAL(18,2) NOT NULL DEFAULT 0,
    SalarioNeto DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_PlanillaDetalle_Periodo
        FOREIGN KEY (IdPlanillaPeriodo) REFERENCES PlanillaPeriodo(IdPlanillaPeriodo),
    CONSTRAINT FK_PlanillaDetalle_Empleado
        FOREIGN KEY (IdEmpleado) REFERENCES Empleado(IdEmpleado)
);

CREATE TABLE PlanillaDeduccionDetalle (
    IdPlanillaDeduccionDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IdPlanillaDetalle INT NOT NULL,
    IdTipoDeduccion INT NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_PlanillaDeduccionDetalle_Detalle
        FOREIGN KEY (IdPlanillaDetalle) REFERENCES PlanillaDetalle(IdPlanillaDetalle),
    CONSTRAINT FK_PlanillaDeduccionDetalle_Tipo
        FOREIGN KEY (IdTipoDeduccion) REFERENCES TipoDeduccion(IdTipoDeduccion)
);

CREATE TABLE Vacacion (
    IdVacacion INT IDENTITY(1,1) PRIMARY KEY,
    IdEmpleado INT NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    Dias DECIMAL(10,2) NOT NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Registrada',
    Observacion NVARCHAR(250),
    CONSTRAINT FK_Vacacion_Empleado
        FOREIGN KEY (IdEmpleado) REFERENCES Empleado(IdEmpleado)
);

CREATE TABLE Incapacidad (
    IdIncapacidad INT IDENTITY(1,1) PRIMARY KEY,
    IdEmpleado INT NOT NULL,
    TipoIncapacidad NVARCHAR(100) NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    Dias DECIMAL(10,2) NOT NULL,
    Observacion NVARCHAR(250),
    CONSTRAINT FK_Incapacidad_Empleado
        FOREIGN KEY (IdEmpleado) REFERENCES Empleado(IdEmpleado)
);

CREATE TABLE Licencia (
    IdLicencia INT IDENTITY(1,1) PRIMARY KEY,
    IdEmpleado INT NOT NULL,
    TipoLicencia NVARCHAR(100) NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    Dias DECIMAL(10,2) NOT NULL,
    ConGoceSalarial BIT NOT NULL DEFAULT 0,
    Observacion NVARCHAR(250),
    CONSTRAINT FK_Licencia_Empleado
        FOREIGN KEY (IdEmpleado) REFERENCES Empleado(IdEmpleado)
);

CREATE TABLE ParametroPlanilla (
    IdParametroPlanilla INT IDENTITY(1,1) PRIMARY KEY,
    NombreParametro NVARCHAR(100) NOT NULL UNIQUE,
    Valor DECIMAL(18,4) NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NULL,
    Estado BIT NOT NULL DEFAULT 1
);

/* =========================
   AUDITORÍA
========================= */

CREATE TABLE BitacoraAuditoria (
    IdBitacora INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NULL,
    TablaAfectada NVARCHAR(100) NOT NULL,
    IdRegistroAfectado INT NULL,
    Accion NVARCHAR(50) NOT NULL,
    ValorAnterior NVARCHAR(MAX),
    ValorNuevo NVARCHAR(MAX),
    FechaAccion DATETIME NOT NULL DEFAULT GETDATE(),
    DireccionIP NVARCHAR(50),
    CONSTRAINT FK_BitacoraAuditoria_Usuario
        FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
);

/* =========================
   ÍNDICES
========================= */

CREATE INDEX IX_Cliente_Nombre ON Cliente(NombreCliente);
CREATE INDEX IX_Empleado_Cedula ON Empleado(Cedula);
CREATE INDEX IX_OrdenCompra_Fecha ON OrdenCompra(FechaOrden);
CREATE INDEX IX_OrdenCompra_Estado ON OrdenCompra(Estado);
CREATE INDEX IX_Factura_Fecha ON Factura(FechaEmision);
CREATE INDEX IX_Factura_Estado ON Factura(Estado);
CREATE INDEX IX_InventarioMovimiento_TipoTarima ON InventarioMovimiento(IdTipoTarima);
CREATE INDEX IX_Bitacora_Fecha ON BitacoraAuditoria(FechaAccion);

/* =========================
   DATOS BASE
========================= */

INSERT INTO Rol (NombreRol, Descripcion)
VALUES
('Administrador', 'Acceso completo al sistema'),
('Gerente', 'Supervisión de clientes, órdenes, facturación e inventario'),
('Contador', 'Gestión de facturación, pagos y planilla'),
('Operador de Planta', 'Gestión de producción, inventario y despachos'),
('Vendedor', 'Gestión de clientes y órdenes de compra');

INSERT INTO Departamento (NombreDepartamento)
VALUES
('Administración'),
('Contabilidad'),
('Producción'),
('Ventas'),
('Gerencia');

INSERT INTO Puesto (NombrePuesto, IdDepartamento)
VALUES
('Administrador del Sistema', 1),
('Contador', 2),
('Operador de Planta', 3),
('Vendedor', 4),
('Gerente General', 5);

INSERT INTO Empleado
(Cedula, Nombre, PrimerApellido, SegundoApellido, Telefono, Correo, Direccion, FechaIngreso, IdPuesto)
VALUES
('0000000001', 'Administrador', 'General', NULL, '0000-0000', 'admin@promaderas.local', 'PROMADERAS', GETDATE(), 1);

INSERT INTO Usuario
(IdEmpleado, IdRol, NombreUsuario, Correo, ContrasenaHash)
VALUES
(1, 1, 'admin', 'admin@promaderas.local', 'CAMBIAR_POR_HASH_REAL');

INSERT INTO TipoDeduccion
(NombreDeduccion, Tipo, Porcentaje, MontoFijo, EsLegal)
VALUES
('CCSS', 'Porcentaje', 10.67, NULL, 1),
('Impuesto de Renta', 'Porcentaje', 0, NULL, 1),
('Deducción Interna', 'MontoFijo', NULL, 0, 0);

INSERT INTO ParametroPlanilla
(NombreParametro, Valor, FechaInicio)
VALUES
('PorcentajeCCSS', 10.67, GETDATE()),
('PorcentajeHoraExtra', 1.5, GETDATE()),
('DiasVacacionesPorMes', 1.0, GETDATE());

INSERT INTO TipoTarima
(Codigo, Nombre, Medida, Descripcion, PrecioUnitario, StockMinimo)
VALUES
('TAR-40X48-USA', 'Tarima estándar USA', '40x48', 'Tarima de madera estándar para exportación', 8500, 10),
('TAR-42X42', 'Tarima cuadrada', '42x42', 'Tarima cuadrada para uso comercial', 7500, 10),
('TAR-100X120-EUR', 'Tarima europea', '100x120', 'Tarima tipo europea', 9500, 10);
GO

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'

ALTER TABLE AspNetUsers
ADD Cedula NVARCHAR(50) NULL;

ALTER TABLE AspNetUsers
ADD Direccion NVARCHAR(250) NULL;

SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Empleado';

ALTER TABLE Empleado
ADD Departamento NVARCHAR(100) NULL;