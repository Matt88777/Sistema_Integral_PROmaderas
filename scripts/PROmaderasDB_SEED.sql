/* ============================================================================
   PROmaderasDB_SEED.sql  -  SEED de Fase 1 (datos base que la app NO puede crear)
   ----------------------------------------------------------------------------
   OBJETIVO
     Dejar la base recien creada (por PROmaderasDB_NEW2.0.sql) en estado USABLE,
     poblando por SQL las tablas que NO tienen pantalla de alta en la app:
        Departamento -> Puesto -> Rol -> Empleado -> dbo.Usuario
     (+ ParametroPlanilla, para que el modulo de Planilla pueda calcular).

   POR QUE HACE FALTA
     - AdminUsuarios crea usuarios de IDENTITY (AspNetUsers), NO dbo.Usuario.
     - Ningun controller inserta en dbo.Usuario / dbo.Rol / Departamento / Puesto.
     - Casi todo lo transaccional (ordenes, facturas, inventario, planilla) tiene
       FK a dbo.Usuario. Si esa tabla esta vacia, crear ordenes falla con error de
       validacion y emitir facturas REVIENTA por violacion de FK (el codigo cae al
       fallback IdUsuario = 1, que tampoco existe si la tabla esta vacia).

   EL PUENTE (importante)
     La app relaciona al usuario logueado con dbo.Usuario POR EL CORREO:
       AspNetUsers.Email  ==  dbo.Usuario.Correo
     Por eso los correos de abajo deben coincidir EXACTO con los que crea el
     IdentitySeeder (verificados: usan @PROmaderas.local, con PRO en mayusculas).

   IDEMPOTENTE
     Cada INSERT esta guardado con IF NOT EXISTS por su clave UNIQUE natural, asi
     que este script se puede re-ejecutar sin duplicar ni tronar. Las FK se
     resuelven por subconsulta (por nombre/cedula), no por Id fijo, para que
     funcione sin importar que valor de IDENTITY se haya asignado.

   CRITICO - admin con IdUsuario = 1
     El codigo usa "IdUsuario = 1" como fallback hardcodeado. Para que el admin
     quede con Id 1, se inserta PRIMERO en dbo.Usuario ESTANDO LA TABLA VACIA
     (el IDENTITY arranca en 1). Si ya corriste inserts en Usuario antes, borrala
     o reseteala antes de correr esto, o el admin no quedara en 1.

   NOTA appsettings
     El correo del admin es overrideable: IdentitySeeder lee
     configuracion["IdentitySeed:AdminEmail"] ?? "admin@PROmaderas.local".
     Si en TU appsettings.json cambiaste ese valor, ajusta el correo del admin
     aca abajo para que siga coincidiendo.

   Este script NO toca Identity (AspNetUsers/AspNetRoles): eso lo siembra la app
   al arrancar. Aca solo poblamos las tablas de NEGOCIO.
   ============================================================================ */

USE [PROmaderasDB_NEW];
GO

/* ----------------------------------------------------------------------------
   1) DEPARTAMENTO  (nivel 0, sin FK)
   UNIQUE: NombreDepartamento. Estado tiene DEFAULT (1).
   Departamentos coherentes para una fabrica de tarimas.
   ---------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Departamento WHERE NombreDepartamento = N'Administracion')
    INSERT INTO dbo.Departamento (NombreDepartamento, Estado) VALUES (N'Administracion', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Departamento WHERE NombreDepartamento = N'Produccion')
    INSERT INTO dbo.Departamento (NombreDepartamento, Estado) VALUES (N'Produccion', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Departamento WHERE NombreDepartamento = N'Ventas')
    INSERT INTO dbo.Departamento (NombreDepartamento, Estado) VALUES (N'Ventas', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Departamento WHERE NombreDepartamento = N'Contabilidad')
    INSERT INTO dbo.Departamento (NombreDepartamento, Estado) VALUES (N'Contabilidad', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Departamento WHERE NombreDepartamento = N'Bodega')
    INSERT INTO dbo.Departamento (NombreDepartamento, Estado) VALUES (N'Bodega', 1);
GO

/* ----------------------------------------------------------------------------
   2) PUESTO  (nivel 1, FK -> Departamento)
   Puesto NO tiene UNIQUE en el esquema, asi que la guarda de idempotencia usa
   (NombrePuesto + IdDepartamento). El IdDepartamento se resuelve por nombre.
   Al menos un puesto por departamento.
   ---------------------------------------------------------------------------- */
-- Administracion
IF NOT EXISTS (SELECT 1 FROM dbo.Puesto p
               JOIN dbo.Departamento d ON d.IdDepartamento = p.IdDepartamento
               WHERE p.NombrePuesto = N'Administrador de Sistema' AND d.NombreDepartamento = N'Administracion')
    INSERT INTO dbo.Puesto (NombrePuesto, IdDepartamento, Estado)
    VALUES (N'Administrador de Sistema',
            (SELECT IdDepartamento FROM dbo.Departamento WHERE NombreDepartamento = N'Administracion'), 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Puesto p
               JOIN dbo.Departamento d ON d.IdDepartamento = p.IdDepartamento
               WHERE p.NombrePuesto = N'Gerente General' AND d.NombreDepartamento = N'Administracion')
    INSERT INTO dbo.Puesto (NombrePuesto, IdDepartamento, Estado)
    VALUES (N'Gerente General',
            (SELECT IdDepartamento FROM dbo.Departamento WHERE NombreDepartamento = N'Administracion'), 1);

-- Produccion
IF NOT EXISTS (SELECT 1 FROM dbo.Puesto p
               JOIN dbo.Departamento d ON d.IdDepartamento = p.IdDepartamento
               WHERE p.NombrePuesto = N'Operador de Planta' AND d.NombreDepartamento = N'Produccion')
    INSERT INTO dbo.Puesto (NombrePuesto, IdDepartamento, Estado)
    VALUES (N'Operador de Planta',
            (SELECT IdDepartamento FROM dbo.Departamento WHERE NombreDepartamento = N'Produccion'), 1);

-- Ventas
IF NOT EXISTS (SELECT 1 FROM dbo.Puesto p
               JOIN dbo.Departamento d ON d.IdDepartamento = p.IdDepartamento
               WHERE p.NombrePuesto = N'Vendedor' AND d.NombreDepartamento = N'Ventas')
    INSERT INTO dbo.Puesto (NombrePuesto, IdDepartamento, Estado)
    VALUES (N'Vendedor',
            (SELECT IdDepartamento FROM dbo.Departamento WHERE NombreDepartamento = N'Ventas'), 1);

-- Contabilidad
IF NOT EXISTS (SELECT 1 FROM dbo.Puesto p
               JOIN dbo.Departamento d ON d.IdDepartamento = p.IdDepartamento
               WHERE p.NombrePuesto = N'Contador' AND d.NombreDepartamento = N'Contabilidad')
    INSERT INTO dbo.Puesto (NombrePuesto, IdDepartamento, Estado)
    VALUES (N'Contador',
            (SELECT IdDepartamento FROM dbo.Departamento WHERE NombreDepartamento = N'Contabilidad'), 1);

-- Bodega
IF NOT EXISTS (SELECT 1 FROM dbo.Puesto p
               JOIN dbo.Departamento d ON d.IdDepartamento = p.IdDepartamento
               WHERE p.NombrePuesto = N'Encargado de Bodega' AND d.NombreDepartamento = N'Bodega')
    INSERT INTO dbo.Puesto (NombrePuesto, IdDepartamento, Estado)
    VALUES (N'Encargado de Bodega',
            (SELECT IdDepartamento FROM dbo.Departamento WHERE NombreDepartamento = N'Bodega'), 1);
GO

/* ----------------------------------------------------------------------------
   3) ROL  (nivel 0, sin FK)   -- tabla de NEGOCIO dbo.Rol (NO es AspNetRoles)
   UNIQUE: NombreRol. Nombres EXACTOS verificados contra security/Roles.cs.
   ---------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE NombreRol = N'Administrador')
    INSERT INTO dbo.Rol (NombreRol, Descripcion, Estado)
    VALUES (N'Administrador', N'Acceso total al sistema', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE NombreRol = N'Gerente')
    INSERT INTO dbo.Rol (NombreRol, Descripcion, Estado)
    VALUES (N'Gerente', N'Supervision general y reportes', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE NombreRol = N'Contador')
    INSERT INTO dbo.Rol (NombreRol, Descripcion, Estado)
    VALUES (N'Contador', N'Contabilidad, facturacion y pagos', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE NombreRol = N'Operador de Planta')
    INSERT INTO dbo.Rol (NombreRol, Descripcion, Estado)
    VALUES (N'Operador de Planta', N'Produccion e inventario', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE NombreRol = N'Vendedor')
    INSERT INTO dbo.Rol (NombreRol, Descripcion, Estado)
    VALUES (N'Vendedor', N'Ordenes de compra y clientes', 1);
GO

/* ----------------------------------------------------------------------------
   4) EMPLEADO  (nivel 2, FK -> Puesto)   -- 5 empleados, uno por usuario de prueba
   UNIQUE: Cedula (ficticias). Obligatorios: Cedula, Nombre, PrimerApellido,
   FechaIngreso, IdPuesto. SalarioBase/TipoPago/JornadaLaboral se llenan con
   valores razonables (en colones) para que Planilla pueda calcular.
   El Correo del empleado se deja igual al del usuario para trazabilidad.
   IdPuesto se resuelve por NombrePuesto.
   ---------------------------------------------------------------------------- */
-- 4.1 Admin
IF NOT EXISTS (SELECT 1 FROM dbo.Empleado WHERE Cedula = N'101110111')
    INSERT INTO dbo.Empleado
        (Cedula, Nombre, PrimerApellido, SegundoApellido, Correo, FechaIngreso, IdPuesto,
         Estado, FechaCreacion, Departamento, SalarioBase, TipoPago, JornadaLaboral)
    VALUES
        (N'101110111', N'Admin', N'Sistema', N'Principal', N'admin@PROmaderas.local',
         '2024-01-15',
         (SELECT IdPuesto FROM dbo.Puesto WHERE NombrePuesto = N'Administrador de Sistema'),
         1, GETDATE(), N'Administracion', 900000.00, N'Mensual', N'Diurna');

-- 4.2 Gerente
IF NOT EXISTS (SELECT 1 FROM dbo.Empleado WHERE Cedula = N'102220222')
    INSERT INTO dbo.Empleado
        (Cedula, Nombre, PrimerApellido, SegundoApellido, Correo, FechaIngreso, IdPuesto,
         Estado, FechaCreacion, Departamento, SalarioBase, TipoPago, JornadaLaboral)
    VALUES
        (N'102220222', N'Gerente', N'Sistema', N'General', N'gerente@PROmaderas.local',
         '2024-01-15',
         (SELECT IdPuesto FROM dbo.Puesto WHERE NombrePuesto = N'Gerente General'),
         1, GETDATE(), N'Administracion', 1200000.00, N'Mensual', N'Diurna');

-- 4.3 Contador
IF NOT EXISTS (SELECT 1 FROM dbo.Empleado WHERE Cedula = N'103330333')
    INSERT INTO dbo.Empleado
        (Cedula, Nombre, PrimerApellido, SegundoApellido, Correo, FechaIngreso, IdPuesto,
         Estado, FechaCreacion, Departamento, SalarioBase, TipoPago, JornadaLaboral)
    VALUES
        (N'103330333', N'Contador', N'Sistema', N'Financiero', N'contador@PROmaderas.local',
         '2024-02-01',
         (SELECT IdPuesto FROM dbo.Puesto WHERE NombrePuesto = N'Contador'),
         1, GETDATE(), N'Contabilidad', 750000.00, N'Mensual', N'Diurna');

-- 4.4 Operador de Planta
IF NOT EXISTS (SELECT 1 FROM dbo.Empleado WHERE Cedula = N'104440444')
    INSERT INTO dbo.Empleado
        (Cedula, Nombre, PrimerApellido, SegundoApellido, Correo, FechaIngreso, IdPuesto,
         Estado, FechaCreacion, Departamento, SalarioBase, TipoPago, JornadaLaboral)
    VALUES
        (N'104440444', N'Operador', N'Planta', N'Uno', N'operador@PROmaderas.local',
         '2024-03-10',
         (SELECT IdPuesto FROM dbo.Puesto WHERE NombrePuesto = N'Operador de Planta'),
         1, GETDATE(), N'Produccion', 480000.00, N'Quincenal', N'Diurna');

-- 4.5 Vendedor
IF NOT EXISTS (SELECT 1 FROM dbo.Empleado WHERE Cedula = N'105550555')
    INSERT INTO dbo.Empleado
        (Cedula, Nombre, PrimerApellido, SegundoApellido, Correo, FechaIngreso, IdPuesto,
         Estado, FechaCreacion, Departamento, SalarioBase, TipoPago, JornadaLaboral)
    VALUES
        (N'105550555', N'Vendedor', N'Sistema', N'Comercial', N'vendedor@PROmaderas.local',
         '2024-03-10',
         (SELECT IdPuesto FROM dbo.Puesto WHERE NombrePuesto = N'Vendedor'),
         1, GETDATE(), N'Ventas', 520000.00, N'Mensual', N'Diurna');
GO

/* ----------------------------------------------------------------------------
   5) dbo.USUARIO  (nivel 3, FK -> Empleado y -> Rol)   -- LAS 5 FILAS
   UNIQUE: NombreUsuario, Correo. Obligatorios: IdEmpleado, IdRol, NombreUsuario,
   Correo, ContrasenaHash.

   >>> ORDEN CRITICO: el admin va PRIMERO para que el IDENTITY le asigne IdUsuario = 1
       (fallback hardcodeado del codigo). Requiere que la tabla este vacia al correr.

   Correo = EXACTO al Email de AspNetUsers (el puente app <-> negocio).
   ContrasenaHash = PLACEHOLDER DUMMY: la autenticacion real la maneja Identity
   (AspNetUsers.PasswordHash); esta columna es NOT NULL pero el login NO la usa.
   IdEmpleado se resuelve por Cedula; IdRol por NombreRol.
   ---------------------------------------------------------------------------- */
-- 5.1 ADMIN  -> debe quedar con IdUsuario = 1  (insertar primero)
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Correo = N'admin@PROmaderas.local')
    INSERT INTO dbo.Usuario (IdEmpleado, IdRol, NombreUsuario, Correo, ContrasenaHash, Estado)
    VALUES (
        (SELECT IdEmpleado FROM dbo.Empleado WHERE Cedula = N'101110111'),
        (SELECT IdRol FROM dbo.Rol WHERE NombreRol = N'Administrador'),
        N'admin', N'admin@PROmaderas.local',
        N'SEED_DUMMY_NO_USADO__auth_por_Identity', 1);

-- 5.2 GERENTE
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Correo = N'gerente@PROmaderas.local')
    INSERT INTO dbo.Usuario (IdEmpleado, IdRol, NombreUsuario, Correo, ContrasenaHash, Estado)
    VALUES (
        (SELECT IdEmpleado FROM dbo.Empleado WHERE Cedula = N'102220222'),
        (SELECT IdRol FROM dbo.Rol WHERE NombreRol = N'Gerente'),
        N'gerente', N'gerente@PROmaderas.local',
        N'SEED_DUMMY_NO_USADO__auth_por_Identity', 1);

-- 5.3 CONTADOR
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Correo = N'contador@PROmaderas.local')
    INSERT INTO dbo.Usuario (IdEmpleado, IdRol, NombreUsuario, Correo, ContrasenaHash, Estado)
    VALUES (
        (SELECT IdEmpleado FROM dbo.Empleado WHERE Cedula = N'103330333'),
        (SELECT IdRol FROM dbo.Rol WHERE NombreRol = N'Contador'),
        N'contador', N'contador@PROmaderas.local',
        N'SEED_DUMMY_NO_USADO__auth_por_Identity', 1);

-- 5.4 OPERADOR DE PLANTA
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Correo = N'operador@PROmaderas.local')
    INSERT INTO dbo.Usuario (IdEmpleado, IdRol, NombreUsuario, Correo, ContrasenaHash, Estado)
    VALUES (
        (SELECT IdEmpleado FROM dbo.Empleado WHERE Cedula = N'104440444'),
        (SELECT IdRol FROM dbo.Rol WHERE NombreRol = N'Operador de Planta'),
        N'operador', N'operador@PROmaderas.local',
        N'SEED_DUMMY_NO_USADO__auth_por_Identity', 1);

-- 5.5 VENDEDOR
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Correo = N'vendedor@PROmaderas.local')
    INSERT INTO dbo.Usuario (IdEmpleado, IdRol, NombreUsuario, Correo, ContrasenaHash, Estado)
    VALUES (
        (SELECT IdEmpleado FROM dbo.Empleado WHERE Cedula = N'105550555'),
        (SELECT IdRol FROM dbo.Rol WHERE NombreRol = N'Vendedor'),
        N'vendedor', N'vendedor@PROmaderas.local',
        N'SEED_DUMMY_NO_USADO__auth_por_Identity', 1);
GO

/* ----------------------------------------------------------------------------
   6) PARAMETRO PLANILLA  (nivel 0, sin FK)   -- parametros base de calculo
   UNIQUE: NombreParametro. Obligatorios: NombreParametro, Valor, FechaInicio.
   Estado tiene DEFAULT (1). Valores tipicos de Costa Rica.
     - PorcentajeCCSS       = 10.67  (deduccion obrera CCSS)
     - PorcentajeHoraExtra  = 1.5    (recargo hora extra)
     - DiasVacacionesPorMes = 1.0    (dias de vacaciones acumulados por mes)

   FechaInicio = '2026-01-01' FIJA, FechaFin = NULL (vigente).

   POR QUE FECHA FIJA Y NO GETDATE():
   FechaInicio es la VIGENCIA del parametro (dato de negocio: desde cuando rige ese
   valor), NO la fecha en que se instalo la base. Con GETDATE() cada maquina queda con
   una vigencia distinta -segun el dia en que corrio el SEED- y al resolver el parametro
   vigente para una planilla ANTERIOR a esa fecha no se encuentra ninguna version.
   El resto de los parametros (Seccion 7 de PROmaderasDB_SPRINT4.sql) ya arranca el
   2026-01-01: estos tres tienen que coincidir.
   ---------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = N'PorcentajeCCSS')
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES (N'PorcentajeCCSS', 10.6700, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = N'PorcentajeHoraExtra')
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES (N'PorcentajeHoraExtra', 1.5000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = N'DiasVacacionesPorMes')
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES (N'DiasVacacionesPorMes', 1.0000, '2026-01-01', NULL, 1);
GO

/* ----------------------------------------------------------------------------
   7) SEMILLA DE VENTAS  ->  CLIENTE + TIPO TARIMA
   Datos para poder probar el flujo Orden de compra -> Factura apenas se instala,
   sin tener que inventar datos a mano.

   CLIENTE (nivel 0, sin FK):
     UNIQUE: CedulaJuridica. Obligatorio: NombreCliente. Exonerado/PorcentajeExoneracion
     y Estado/FechaCreacion tienen DEFAULT, pero los ponemos explicitos para claridad.
     Se incluye AL MENOS UNO exonerado (exportadora al 100%) para probar ese caso.

   TIPO TARIMA (nivel 0, sin FK):
     UNIQUE: Codigo. Obligatorios: Codigo, Nombre, Medida, PrecioUnitario.
     StockMinimo tiene DEFAULT (0) pero lo seteamos. Medidas REALES del dominio
     PROMADERAS. Precios en colones (referenciales).
   ---------------------------------------------------------------------------- */
-- 7.1 CLIENTES ---------------------------------------------------------------
-- Exportadora: exonerada de impuesto (100%) -> sirve para probar el caso exonerado
IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE CedulaJuridica = N'3-101-100001')
    INSERT INTO dbo.Cliente
        (CedulaJuridica, NombreCliente, Telefono, Correo, Direccion,
         CondicionPago, Exonerado, PorcentajeExoneracion, Estado, FechaCreacion)
    VALUES
        (N'3-101-100001', N'Exportadora Del Valle S.A.', N'2711-1000',
         N'compras@delvalle.co.cr', N'Zona Franca, Alajuela',
         N'Credito 30 dias', 1, 100.00, 1, GETDATE());

-- Distribuidora: NO exonerada, paga contado
IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE CedulaJuridica = N'3-102-200002')
    INSERT INTO dbo.Cliente
        (CedulaJuridica, NombreCliente, Telefono, Correo, Direccion,
         CondicionPago, Exonerado, PorcentajeExoneracion, Estado, FechaCreacion)
    VALUES
        (N'3-102-200002', N'Distribuidora Tarimas del Caribe Ltda', N'2795-2000',
         N'ventas@tarimascaribe.cr', N'Limon centro',
         N'Contado', 0, 0.00, 1, GETDATE());

-- Agroindustria: NO exonerada, credito
IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE CedulaJuridica = N'3-101-300003')
    INSERT INTO dbo.Cliente
        (CedulaJuridica, NombreCliente, Telefono, Correo, Direccion,
         CondicionPago, Exonerado, PorcentajeExoneracion, Estado, FechaCreacion)
    VALUES
        (N'3-101-300003', N'Agroindustrial Pococi S.A.', N'2710-3000',
         N'administracion@agropococi.cr', N'Guapiles, Pococi',
         N'Credito 15 dias', 0, 0.00, 1, GETDATE());

-- 7.2 TIPO TARIMA (medidas reales del dominio) ------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.TipoTarima WHERE Codigo = N'TAR-4048-USA')
    INSERT INTO dbo.TipoTarima
        (Codigo, Nombre, Medida, Descripcion, PrecioUnitario, StockMinimo, Estado, FechaCreacion)
    VALUES
        (N'TAR-4048-USA', N'Tarima 40x48 USA', N'40x48 pulgadas (estandar USA / GMA)',
         N'Tarima estandar norteamericana de 4 entradas', 8500.00, 50, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.TipoTarima WHERE Codigo = N'TAR-4247-USA')
    INSERT INTO dbo.TipoTarima
        (Codigo, Nombre, Medida, Descripcion, PrecioUnitario, StockMinimo, Estado, FechaCreacion)
    VALUES
        (N'TAR-4247-USA', N'Tarima 42x47 USA', N'42x47 pulgadas (USA)',
         N'Tarima para tambores/bidones', 9000.00, 50, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.TipoTarima WHERE Codigo = N'TAR-100100-USA')
    INSERT INTO dbo.TipoTarima
        (Codigo, Nombre, Medida, Descripcion, PrecioUnitario, StockMinimo, Estado, FechaCreacion)
    VALUES
        (N'TAR-100100-USA', N'Tarima 100x100 USA', N'100x100 pulgadas (gran formato USA)',
         N'Tarima de gran formato para carga pesada', 15000.00, 20, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.TipoTarima WHERE Codigo = N'TAR-4048-EUR')
    INSERT INTO dbo.TipoTarima
        (Codigo, Nombre, Medida, Descripcion, PrecioUnitario, StockMinimo, Estado, FechaCreacion)
    VALUES
        (N'TAR-4048-EUR', N'Tarima 40x48 EUR', N'40x48 (formato europeo)',
         N'Tarima formato europeo', 9500.00, 40, 1, GETDATE());
GO

/* ----------------------------------------------------------------------------
   8) VERIFICACION  -- confirma lo insertado. Revisa que el admin quedo en IdUsuario = 1.
   ---------------------------------------------------------------------------- */
PRINT '--- Usuarios (el admin debe ser IdUsuario = 1) ---';
SELECT u.IdUsuario, u.NombreUsuario, u.Correo, r.NombreRol, e.Cedula
FROM dbo.Usuario u
JOIN dbo.Rol r      ON r.IdRol = u.IdRol
JOIN dbo.Empleado e ON e.IdEmpleado = u.IdEmpleado
ORDER BY u.IdUsuario;

PRINT '--- Conteos ---';
SELECT
    (SELECT COUNT(*) FROM dbo.Departamento)      AS Departamentos,
    (SELECT COUNT(*) FROM dbo.Puesto)            AS Puestos,
    (SELECT COUNT(*) FROM dbo.Rol)               AS Roles,
    (SELECT COUNT(*) FROM dbo.Empleado)          AS Empleados,
    (SELECT COUNT(*) FROM dbo.Usuario)           AS Usuarios,
    (SELECT COUNT(*) FROM dbo.ParametroPlanilla) AS ParametrosPlanilla,
    (SELECT COUNT(*) FROM dbo.Cliente)           AS Clientes,
    (SELECT COUNT(*) FROM dbo.TipoTarima)        AS TipoTarimas;
GO

/* ----------------------------------------------------------------------------
   PARA PROBAR: crea una orden con "Exportadora Del Valle S.A." + tarima 40x48 USA,
   despues emitila desde Facturacion.
   ---------------------------------------------------------------------------- */
