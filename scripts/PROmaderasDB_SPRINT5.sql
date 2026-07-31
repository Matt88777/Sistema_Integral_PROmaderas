/* =====================================================================================
   PROMADERAS S.A. - Sistema Integral de Gestión
   Script:  PROmaderasDB_SPRINT5.sql   (v1)
   Autor:   Jiménez Bogantes Mattias
   Curso:   SC-702 Diseño y Desarrollo de Sistemas - Universidad Fidélitas

   PROPÓSITO
   ---------
   Consolidar en el repositorio los cambios de esquema del Sprint 5 que hasta hoy solo
   existían como SQL corrido a mano en máquinas individuales. Sin este archivo, quien
   rehaga la base con los scripts oficiales obtiene un esquema INCOMPLETO y la aplicación
   revienta en la pantalla de Productos (falta Categoria/ImagenUrl) y en
   Deducciones/Asignar (faltan las 2 columnas de EmpleadoDeduccion).

   NO reemplaza a ningún script anterior: se corre DESPUÉS de todos ellos.

   ORDEN DE SETUP ACTUALIZADO
   --------------------------
     1) scripts/PROmaderasDB_NEW2.0.sql   (esquema base)
     2) scripts/PROmaderasDB_SEED.sql     (datos base obligatorios)
     3) scripts/PROmaderasDB_SPRINT4.sql  (esquema del Sprint 4)
     4) scripts/PROmaderasDB_SPRINT5.sql  (ESTE ARCHIVO)
     5) crear appsettings.json  ->  dotnet run

   *** EL ORDEN 2 ANTES DE 4 YA NO ES UNA SUGERENCIA: ES OBLIGATORIO ***
   Este script deja TipoTarima.IdCategoria en NOT NULL y sin DEFAULT. Los INSERT de
   TipoTarima que hace PROmaderasDB_SEED.sql no envían categoría, así que si el SEED se
   corre DESPUÉS de este archivo falla por violación de NOT NULL. Si ya te pasó: borrá
   las filas de TipoTarima, corré el SEED y volvé a correr este script.

   IDEMPOTENTE: se puede correr varias veces. Todo con guardas IF NOT EXISTS.

   QUÉ CUBRE
   ---------
     Bloque 1  Categorías de tarima (tabla Categoria + TipoTarima.IdCategoria + FK)
     Bloque 2  TipoTarima.ImagenUrl (+ DEFAULT + NOT NULL)
     Bloque 3  EmpleadoDeduccion.NumeroCuotas y .MontoTotal
     Bloque 4  Datos de emergencia y salud en Empleado (requerimiento del cliente)
     Bloque 5  Verificación

   QUÉ NO CUBRE (A PROPÓSITO)
   --------------------------
   Las correcciones de DATOS de facturación (recálculo de SaldoPendiente y sincronización
   de Estado='Pagada'). Eso NO es esquema: son UPDATE masivos sobre datos transaccionales
   de una base ajena. Correrlos a ciegas en la máquina de otra persona es peligroso y no
   hace falta para que la aplicación levante. Viven fuera del repositorio.

   VERIFICADO CONTRA LA BASE REAL (30-jul-2026, instancia local)
   -------------------------------------------------------------
   Los tipos, longitudes y nombres de constraint de este script fueron tomados de
   sys.columns / sys.foreign_keys / sys.default_constraints de una base que YA tiene los
   cambios aplicados a mano, no del modelo C# ni de la documentación. En particular:
     - Categoria.Nombre es NVARCHAR(100), Estado es BIT.
     - TipoTarima.ImagenUrl es NVARCHAR(300) y su DEFAULT se llama DF_TipoTarima_ImagenUrl.
     - La FK se llama FK_TipoTarima_Categoria y es NO ACTION.
     - EmpleadoDeduccion.NumeroCuotas es INT NULL y MontoTotal es DECIMAL(18,2) NULL.

   NOTA SOBRE MIGRACIONES EF
   -------------------------
   Existen en disco dos migraciones huérfanas del contexto de Negocio
   (20260724031523_Ded_NumeroCuotas y 20260724033336_Ded_MontoTotal) que NADIE aplica,
   porque Program.cs solo migra el contexto de Identity. El Bloque 3 crea esas dos columnas
   por SQL, que es la política del proyecto: el esquema lo gobiernan los scripts.
   NO corras 'dotnet ef database update' sobre el contexto de Negocio.
   ===================================================================================== */

USE PROmaderasDB_NEW;
GO

SET NOCOUNT ON;
GO

PRINT '=== PROmaderasDB_SPRINT5.sql (v1) : INICIO ===';
GO


/* =====================================================================================
   BLOQUE 1 - Categorías de tarima
   -------------------------------------------------------------------------------------
   Requerimiento del cliente traído por la profesora el 28-jul-2026. Revierte la decisión
   de SC-603 de eliminar las categorías: NO es un error, es un cambio de alcance.

   El DDL original lo escribió Daniela y quedó pegado al final de PROmaderasDB_NEW.sql
   (el archivo de la raíz que el README marca como "NO correr"), o sea que nunca formó
   parte del procedimiento de instalación. Acá se formaliza.

   DIFERENCIAS CONTRA EL DDL ORIGINAL, Y POR QUÉ
   ---------------------------------------------
   1. Las constraints van con NOMBRE EXPLÍCITO (PK_Categoria, DF_Categoria_Estado). El DDL
      original las declaraba inline, así que SQL Server les puso nombres autogenerados
      (PK__Categori__A3C02A10...) que son DISTINTOS en cada máquina, porque el sufijo sale
      del object_id. Un esquema determinista vale más que la simetría con una base vieja.
   2. Se agrega UQ_Categoria_Nombre. La tabla original no tenía nada que impidiera sembrar
      dos veces la misma categoría.
   3. La clasificación mira Nombre Y Codigo (el original solo miraba Nombre).
   4. Si queda alguna tarima sin clasificar, el script FALLA RUIDOSAMENTE y no aplica el
      NOT NULL. El original imprimía un SELECT y confiaba en que alguien lo leyera.
   ===================================================================================== */

/* --- 1.1  Tabla Categoria --------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Categoria' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Categoria
    (
        IdCategoria  INT IDENTITY(1,1) NOT NULL,
        Nombre       NVARCHAR(100)     NOT NULL,
        Estado       BIT               NOT NULL CONSTRAINT DF_Categoria_Estado DEFAULT (1),

        CONSTRAINT PK_Categoria PRIMARY KEY CLUSTERED (IdCategoria)
    );
    PRINT '  [+] Tabla Categoria creada.';
END
ELSE PRINT '  [=] Tabla Categoria ya existía.';
GO

/* --- 1.2  UNIQUE sobre el nombre --------------------------------------------------
   Se agrega también en bases que ya tenían la tabla sin esta constraint. Si por algún
   motivo ya hay nombres duplicados, no se fuerza: se avisa y se sigue, porque el UNIQUE
   no es indispensable para que la aplicación funcione.                                */
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints
               WHERE parent_object_id = OBJECT_ID('dbo.Categoria')
                 AND name = 'UQ_Categoria_Nombre')
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Categoria GROUP BY Nombre HAVING COUNT(1) > 1)
        PRINT '  [!] Hay categorías con nombre duplicado: NO se creó UQ_Categoria_Nombre. Limpialas y volvé a correr.';
    ELSE
    BEGIN
        ALTER TABLE dbo.Categoria ADD CONSTRAINT UQ_Categoria_Nombre UNIQUE (Nombre);
        PRINT '  [+] UQ_Categoria_Nombre agregado.';
    END
END
ELSE PRINT '  [=] UQ_Categoria_Nombre ya existía.';
GO

/* --- 1.3  Categorías base ---------------------------------------------------------
   OBLIGATORIAS: TipoTarima.IdCategoria queda NOT NULL sin DEFAULT, así que sin estas dos
   filas no se puede insertar ninguna tarima. Los literales son los que usa la aplicación. */
IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE Nombre = N'Estadounidense')
BEGIN
    INSERT INTO dbo.Categoria (Nombre, Estado) VALUES (N'Estadounidense', 1);
    PRINT '  [+] Categoría Estadounidense sembrada.';
END
ELSE PRINT '  [=] Categoría Estadounidense ya existía.';

IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE Nombre = N'Europea')
BEGIN
    INSERT INTO dbo.Categoria (Nombre, Estado) VALUES (N'Europea', 1);
    PRINT '  [+] Categoría Europea sembrada.';
END
ELSE PRINT '  [=] Categoría Europea ya existía.';
GO

/* --- 1.4  Columna TipoTarima.IdCategoria (primero NULL) ---------------------------
   Se agrega NULL a propósito: hay que clasificar las filas existentes ANTES de poder
   exigir la columna.                                                                  */
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.TipoTarima') AND name = 'IdCategoria')
BEGIN
    ALTER TABLE dbo.TipoTarima ADD IdCategoria INT NULL;
    PRINT '  [+] TipoTarima.IdCategoria agregada (nullable, temporal).';
END
ELSE PRINT '  [=] TipoTarima.IdCategoria ya existía.';
GO

/* --- 1.5  Clasificación, NOT NULL y llave foránea ---------------------------------
   TODO ESTO VA EN UN SOLO BATCH a propósito. Si la clasificación deja alguna tarima sin
   categoría, el RAISERROR corta acá y el NOT NULL NO se aplica. Con GO de por medio,
   SSMS seguiría ejecutando los batches siguientes y el ALTER fallaría con un error mucho
   menos claro ("Cannot insert the value NULL...").                                     */
UPDATE t
   SET t.IdCategoria = (SELECT c.IdCategoria FROM dbo.Categoria c WHERE c.Nombre = N'Estadounidense')
  FROM dbo.TipoTarima t
 WHERE t.IdCategoria IS NULL
   AND (t.Nombre LIKE '%USA%' OR t.Codigo LIKE '%USA%');

UPDATE t
   SET t.IdCategoria = (SELECT c.IdCategoria FROM dbo.Categoria c WHERE c.Nombre = N'Europea')
  FROM dbo.TipoTarima t
 WHERE t.IdCategoria IS NULL
   AND (t.Nombre LIKE '%EUR%' OR t.Codigo LIKE '%EUR%');

IF EXISTS (SELECT 1 FROM dbo.TipoTarima WHERE IdCategoria IS NULL)
BEGIN
    DECLARE @sinClasificar NVARCHAR(2000);

    SELECT @sinClasificar = STUFF((
        SELECT N', ' + t.Codigo
          FROM dbo.TipoTarima t
         WHERE t.IdCategoria IS NULL
         ORDER BY t.Codigo
           FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(2000)'), 1, 2, N'');

    RAISERROR (N'SPRINT5 Bloque 1 DETENIDO: hay tarimas sin categoría y su nombre no dice USA ni EUR. Asignales IdCategoria a mano y volvé a correr el script. Códigos afectados: %s',
               16, 1, @sinClasificar);
END
ELSE
BEGIN
    PRINT '  [+] Clasificación completa: 0 tarimas sin categoría.';

    IF EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.TipoTarima')
                 AND name = 'IdCategoria' AND is_nullable = 1)
    BEGIN
        ALTER TABLE dbo.TipoTarima ALTER COLUMN IdCategoria INT NOT NULL;
        PRINT '  [+] TipoTarima.IdCategoria ahora es NOT NULL.';
    END
    ELSE PRINT '  [=] TipoTarima.IdCategoria ya era NOT NULL.';

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
                   WHERE name = 'FK_TipoTarima_Categoria'
                     AND parent_object_id = OBJECT_ID('dbo.TipoTarima'))
    BEGIN
        ALTER TABLE dbo.TipoTarima
            ADD CONSTRAINT FK_TipoTarima_Categoria
                FOREIGN KEY (IdCategoria) REFERENCES dbo.Categoria (IdCategoria);
        PRINT '  [+] FK_TipoTarima_Categoria agregada.';
    END
    ELSE PRINT '  [=] FK_TipoTarima_Categoria ya existía.';
END
GO


/* =====================================================================================
   BLOQUE 2 - TipoTarima.ImagenUrl
   -------------------------------------------------------------------------------------
   La aplicación muestra la imagen del producto en el Index y en Detalles. Sin esta
   columna, Productos revienta con "Invalid column name 'ImagenUrl'".

   NOT NULL CON DEFAULT: se puede agregar sobre una tabla con filas porque SQL Server les
   escribe el valor por defecto. El modelo C# declara ImagenUrl como string no-nullable
   con [Required], así que una columna NULL le mentiría a EF.

   El nombre DF_TipoTarima_ImagenUrl NO es decorativo: es el nombre que ya tiene la
   constraint en las bases donde esto se corrió a mano. Mantenerlo evita que dos
   instalaciones queden con nombres distintos para la misma cosa.

   OJO CON EL DEFAULT: apunta a /imagenes/sin-imagen.jpg. Ese archivo TIENE que existir en
   la carpeta Imagenes/ de la raíz de la solución, o las tarimas sin foto muestran un 404.
   ===================================================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.TipoTarima') AND name = 'ImagenUrl')
BEGIN
    ALTER TABLE dbo.TipoTarima
        ADD ImagenUrl NVARCHAR(300) NOT NULL
            CONSTRAINT DF_TipoTarima_ImagenUrl DEFAULT ('/imagenes/sin-imagen.jpg');
    PRINT '  [+] TipoTarima.ImagenUrl NVARCHAR(300) NOT NULL agregada.';
END
ELSE PRINT '  [=] TipoTarima.ImagenUrl ya existía.';
GO

/* --- 2.1  Imágenes de las 4 tarimas del SEED --------------------------------------
   ESTO ES DATO, NO ESQUEMA, y está acá a conciencia. Es seed de las mismas 4 filas que
   siembra PROmaderasDB_SEED.sql, no una corrección sobre datos transaccionales de nadie:
   sin esto, una instalación nueva muestra las cuatro tarimas con la imagen genérica y la
   pantalla de Productos se ve rota en la demo.

   El WHERE exige que la imagen siga siendo la del DEFAULT, así que si alguien ya le
   asignó una foto propia desde la aplicación, el script NO se la pisa.

   La correspondencia archivo <-> tarima NO es correlativa (la 40x48 EUR usa Tarimas_1).
   Son los valores reales de la base de referencia; no los "ordenes".                    */
UPDATE dbo.TipoTarima SET ImagenUrl = '/imagenes/Tarimas_2.jpeg'
 WHERE Codigo = N'TAR-4048-USA'   AND ImagenUrl = '/imagenes/sin-imagen.jpg';

UPDATE dbo.TipoTarima SET ImagenUrl = '/imagenes/Tarimas_3.jpeg'
 WHERE Codigo = N'TAR-4247-USA'   AND ImagenUrl = '/imagenes/sin-imagen.jpg';

UPDATE dbo.TipoTarima SET ImagenUrl = '/imagenes/Tarimas_4.jpeg'
 WHERE Codigo = N'TAR-100100-USA' AND ImagenUrl = '/imagenes/sin-imagen.jpg';

UPDATE dbo.TipoTarima SET ImagenUrl = '/imagenes/Tarimas_1.jpeg'
 WHERE Codigo = N'TAR-4048-EUR'   AND ImagenUrl = '/imagenes/sin-imagen.jpg';

PRINT '  [+] Imágenes de las tarimas del seed verificadas/asignadas.';
GO


/* =====================================================================================
   BLOQUE 3 - EmpleadoDeduccion: NumeroCuotas y MontoTotal
   -------------------------------------------------------------------------------------
   Sin estas dos columnas, Deducciones/Asignar?idEmpleado=N revienta con
   "Invalid column name 'MontoTotal'. Invalid column name 'NumeroCuotas'."

   Por qué el Index y el Create de Deducciones SÍ funcionan sin ellas: esas pantallas
   trabajan sobre DeduccionInterna (el catálogo). La que las necesita es EmpleadoDeduccion
   (la asignación).

   NULLABLE A PROPÓSITO: en EmpleadoDeduccionAD las propiedades son int? y decimal?, y las
   asignaciones que ya existen no tienen esos datos. Ponerlas NOT NULL obligaría a inventar
   un valor por defecto que no significa nada.

   Los tipos son idénticos a los de las dos migraciones EF huérfanas, para que el día que
   alguien decida hacer el refactor de migraciones no haya diferencias que reconciliar.
   ===================================================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.EmpleadoDeduccion') AND name = 'NumeroCuotas')
BEGIN
    ALTER TABLE dbo.EmpleadoDeduccion ADD NumeroCuotas INT NULL;
    PRINT '  [+] EmpleadoDeduccion.NumeroCuotas INT NULL agregada.';
END
ELSE PRINT '  [=] EmpleadoDeduccion.NumeroCuotas ya existía.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.EmpleadoDeduccion') AND name = 'MontoTotal')
BEGIN
    ALTER TABLE dbo.EmpleadoDeduccion ADD MontoTotal DECIMAL(18,2) NULL;
    PRINT '  [+] EmpleadoDeduccion.MontoTotal DECIMAL(18,2) NULL agregada.';
END
ELSE PRINT '  [=] EmpleadoDeduccion.MontoTotal ya existía.';
GO


/* =====================================================================================
   BLOQUE 4 - Empleado: contacto de emergencia, alergias y medicamentos
   -------------------------------------------------------------------------------------
   Requerimiento textual del cliente: "En los datos de empleado es importante que
   contemplen información de contacto en caso de emergencia, alergias, medicamentos."

   DECISIONES DE DISEÑO
   --------------------
   - UN SOLO contacto de emergencia (nombre + teléfono + parentesco), no una tabla 1:N.
     Nadie pidió varios, y una tabla más con su CRUD es sobre-ingeniería para el alcance.
   - Alergias y medicamentos como TEXTO LIBRE, por lo mismo. No alimentan ningún cálculo,
     no tienen catálogo y no afectan planilla ni liquidación: son informativos.
   - TODAS NULL. Los empleados que ya están en la base no tienen estos datos y son
     opcionales por naturaleza. NOT NULL obligaría a inventar valores falsos.
   - NVARCHAR (no VARCHAR): alergias y medicamentos van a llevar tildes sí o sí. Empleado
     tiene una sola columna de texto no-Unicode, Departamento varchar(100), y esa es
     justamente la razón por la que sus literales van sin tildes. No repetimos el patrón.
   - ContactoEmergenciaTelefono es NVARCHAR(25) para igualar a Empleado.Telefono, que ya
     es 25. No tiene sentido que el teléfono del contacto aguante menos que el del empleado.

   PRIVACIDAD: alergias y medicamentos son datos de salud. EmpleadosController es
   [Authorize(Roles = Roles.Administrador)], así que solo el Administrador los ve. Es una
   decisión consciente y hay que mantenerla.

   SEGURO PARA EF: EmpleadoAD mapea por convención. Las columnas que el modelo todavía no
   declare simplemente se ignoran, así que este bloque se puede correr antes de tocar el C#.
   ===================================================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Empleado') AND name = 'ContactoEmergenciaNombre')
BEGIN
    ALTER TABLE dbo.Empleado ADD ContactoEmergenciaNombre NVARCHAR(150) NULL;
    PRINT '  [+] Empleado.ContactoEmergenciaNombre NVARCHAR(150) NULL agregada.';
END
ELSE PRINT '  [=] Empleado.ContactoEmergenciaNombre ya existía.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Empleado') AND name = 'ContactoEmergenciaTelefono')
BEGIN
    ALTER TABLE dbo.Empleado ADD ContactoEmergenciaTelefono NVARCHAR(25) NULL;
    PRINT '  [+] Empleado.ContactoEmergenciaTelefono NVARCHAR(25) NULL agregada.';
END
ELSE PRINT '  [=] Empleado.ContactoEmergenciaTelefono ya existía.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Empleado') AND name = 'ContactoEmergenciaParentesco')
BEGIN
    ALTER TABLE dbo.Empleado ADD ContactoEmergenciaParentesco NVARCHAR(50) NULL;
    PRINT '  [+] Empleado.ContactoEmergenciaParentesco NVARCHAR(50) NULL agregada.';
END
ELSE PRINT '  [=] Empleado.ContactoEmergenciaParentesco ya existía.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Empleado') AND name = 'Alergias')
BEGIN
    ALTER TABLE dbo.Empleado ADD Alergias NVARCHAR(500) NULL;
    PRINT '  [+] Empleado.Alergias NVARCHAR(500) NULL agregada.';
END
ELSE PRINT '  [=] Empleado.Alergias ya existía.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Empleado') AND name = 'Medicamentos')
BEGIN
    ALTER TABLE dbo.Empleado ADD Medicamentos NVARCHAR(500) NULL;
    PRINT '  [+] Empleado.Medicamentos NVARCHAR(500) NULL agregada.';
END
ELSE PRINT '  [=] Empleado.Medicamentos ya existía.';
GO


/* =====================================================================================
   BLOQUE 5 - Verificación
   -------------------------------------------------------------------------------------
   Los 13 chequeos deben dar OK. Si alguno dice FALTA, el script no terminó su trabajo:
   revisá la pestaña Messages, donde el bloque que falló dejó su motivo.

   Se consulta SOLO el catálogo del sistema (sys.*, COL_LENGTH, OBJECT_ID) y nunca las
   tablas directamente. Motivo: si una columna todavía no existe, un SELECT que la nombre
   ni siquiera compila y el batch entero se cae con un error de sintaxis, escondiendo el
   diagnóstico. Con COL_LENGTH el batch siempre corre y reporta.
   ===================================================================================== */

PRINT '';
PRINT '--- Verificación ---';

SELECT 'Tabla Categoria' AS Objeto,
       IIF(OBJECT_ID('dbo.Categoria') IS NULL, 'FALTA', 'OK') AS Estado
UNION ALL SELECT 'Categoria.Nombre',
       IIF(COL_LENGTH('dbo.Categoria','Nombre') IS NULL, 'FALTA', 'OK')
UNION ALL SELECT 'UQ_Categoria_Nombre',
       IIF(EXISTS (SELECT 1 FROM sys.key_constraints
                   WHERE parent_object_id = OBJECT_ID('dbo.Categoria')
                     AND name = 'UQ_Categoria_Nombre'), 'OK', 'FALTA')
UNION ALL SELECT 'TipoTarima.IdCategoria',
       IIF(COL_LENGTH('dbo.TipoTarima','IdCategoria') IS NULL, 'FALTA', 'OK')
UNION ALL SELECT 'TipoTarima.IdCategoria es NOT NULL',
       IIF(EXISTS (SELECT 1 FROM sys.columns
                   WHERE object_id = OBJECT_ID('dbo.TipoTarima')
                     AND name = 'IdCategoria' AND is_nullable = 0), 'OK', 'FALTA')
UNION ALL SELECT 'FK_TipoTarima_Categoria',
       IIF(EXISTS (SELECT 1 FROM sys.foreign_keys
                   WHERE name = 'FK_TipoTarima_Categoria'
                     AND parent_object_id = OBJECT_ID('dbo.TipoTarima')), 'OK', 'FALTA')
UNION ALL SELECT 'TipoTarima.ImagenUrl',
       IIF(COL_LENGTH('dbo.TipoTarima','ImagenUrl') IS NULL, 'FALTA', 'OK')
UNION ALL SELECT 'DF_TipoTarima_ImagenUrl',
       IIF(EXISTS (SELECT 1 FROM sys.default_constraints
                   WHERE parent_object_id = OBJECT_ID('dbo.TipoTarima')
                     AND name = 'DF_TipoTarima_ImagenUrl'), 'OK', 'FALTA')
UNION ALL SELECT 'EmpleadoDeduccion.NumeroCuotas',
       IIF(COL_LENGTH('dbo.EmpleadoDeduccion','NumeroCuotas') IS NULL, 'FALTA', 'OK')
UNION ALL SELECT 'EmpleadoDeduccion.MontoTotal',
       IIF(COL_LENGTH('dbo.EmpleadoDeduccion','MontoTotal') IS NULL, 'FALTA', 'OK')
UNION ALL SELECT 'Empleado: contacto de emergencia (3 columnas)',
       IIF((SELECT COUNT(1) FROM sys.columns
             WHERE object_id = OBJECT_ID('dbo.Empleado')
               AND name IN ('ContactoEmergenciaNombre','ContactoEmergenciaTelefono',
                            'ContactoEmergenciaParentesco')) = 3, 'OK', 'FALTA')
UNION ALL SELECT 'Empleado.Alergias',
       IIF(COL_LENGTH('dbo.Empleado','Alergias') IS NULL, 'FALTA', 'OK')
UNION ALL SELECT 'Empleado.Medicamentos',
       IIF(COL_LENGTH('dbo.Empleado','Medicamentos') IS NULL, 'FALTA', 'OK');
GO

/* Chequeos de DATOS. Van por sp_executesql para que el batch compile aunque la columna
   IdCategoria todavía no exista (ver la nota de arriba). */
IF COL_LENGTH('dbo.TipoTarima','IdCategoria') IS NOT NULL AND OBJECT_ID('dbo.Categoria') IS NOT NULL
    EXEC sp_executesql N'
        SELECT ''Categorías sembradas (esperado 2)'' AS Chequeo,
               CAST(COUNT(1) AS NVARCHAR(10)) AS Valor,
               IIF(COUNT(1) >= 2, ''OK'', ''FALTA'') AS Estado
          FROM dbo.Categoria
        UNION ALL
        SELECT ''Tarimas sin categoría (esperado 0)'',
               CAST(COUNT(1) AS NVARCHAR(10)),
               IIF(COUNT(1) = 0, ''OK'', ''REVISAR'')
          FROM dbo.TipoTarima WHERE IdCategoria IS NULL
        UNION ALL
        SELECT ''Tarimas con imagen genérica'',
               CAST(COUNT(1) AS NVARCHAR(10)),
               ''INFO''
          FROM dbo.TipoTarima WHERE ImagenUrl = ''/imagenes/sin-imagen.jpg'';';
ELSE
    PRINT '  [!] No se corrieron los chequeos de datos: falta la columna IdCategoria o la tabla Categoria.';
GO

PRINT '=== PROmaderasDB_SPRINT5.sql (v1) : FIN ===';
GO
