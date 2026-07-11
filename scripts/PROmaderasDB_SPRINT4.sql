/* =====================================================================================
   PROMADERAS S.A. - Sistema Integral de Gestion
   Script:  PROmaderasDB_SPRINT4.sql   (v3 - corregido contra el esquema real)
   Autor:   Jimenez Bogantes Mattias
   Curso:   SC-702 Diseno y Desarrollo de Sistemas - Universidad Fidelitas

   PROPOSITO
   ---------
   Cambios de esquema requeridos por las HU pendientes del Sprint 4.
   NO reemplaza a scripts/PROmaderasDB_NEW2.0.sql: se corre DESPUES de el.

   ORDEN DE SETUP ACTUALIZADO
   --------------------------
     1) scripts/PROmaderasDB_NEW2.0.sql   (esquema base)
     2) scripts/PROmaderasDB_SEED.sql     (datos base obligatorios)
     3) scripts/PROmaderasDB_SPRINT4.sql  (ESTE ARCHIVO)
     4) crear appsettings.json  ->  dotnet run

   IDEMPOTENTE: se puede correr varias veces. Todo con guardas IF NOT EXISTS.

   HU CUBIERTAS
   ------------
     PLA-HU-012  Vacaciones acumuladas / disfrutadas / saldo   -> Seccion 1
     PLA-HU-013  Aplicar vacaciones en calculo de planilla     -> Seccion 2
     PLA-HU-014  Incapacidades en calculo de planilla          -> Seccion 2
     PLA-HU-017  Liquidacion de empleado                       -> Secciones 3 y 4
     PLA-HU-018  Polizas del INS                               -> Seccion 5
     PLA-HU-019  Parametros de planilla                        -> Seccion 6  (FIX CRITICO)

   HU QUE NO NECESITAN NADA DE ESTE SCRIPT
   ---------------------------------------
     FAC-HU-005, PLA-HU-021, REP-HU-001, REP-HU-002, REP-HU-003

   CAMBIOS DE LA v3
   ----------------
   Limpia el parametro duplicado 'VacacionesDiasPorMes' que la v2 insertaba de mas (el
   nombre bueno es 'DiasVacacionesPorMes', que ya siembra PROmaderasDB_SEED.sql) y agrega
   los parametros de renta (pisos y porcentajes de los 4 tramos) mas HorasMes, que hasta
   ahora estaban hardcodeados en PlanillaLogica.cs.

   Ademas normaliza la FechaInicio de 'DiasVacacionesPorMes' a 2026-01-01: el SEED lo
   siembra con GETDATE(), asi que su vigencia arrancaba el dia de la instalacion y los
   periodos de planilla anteriores a esa fecha se quedaban sin parametro (PLA-HU-012).
   Ver Seccion 7.
   ===================================================================================== */

USE PROmaderasDB_NEW;
GO

SET NOCOUNT ON;
GO

PRINT '=== PROmaderasDB_SPRINT4.sql (v3) : INICIO ===';
GO


/* =====================================================================================
   SECCION 1 - PLA-HU-012 : Saldo inicial de vacaciones
   -------------------------------------------------------------------------------------
   La tabla Vacacion YA EXISTE y registra las vacaciones DISFRUTADAS (periodos tomados).
   Las ACUMULADAS no se guardan: se DERIVAN en LogicaDeNegocio como

       acumuladas = SaldoVacacionesInicial
                  + (meses trabajados desde FechaIngreso * parametro 'DiasVacacionesPorMes')

       saldo      = acumuladas - SUM(Vacacion.Dias WHERE Estado = 'Disfrutada')

   Se agrega SaldoVacacionesInicial porque los empleados vienen migrados de Excel y
   ninguno arranca en cero. Sin esta columna el saldo daria mal desde el dia uno.

   Precision (10,2): identica a Vacacion.Dias e Incapacidad.Dias.
   ===================================================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Empleado') AND name = 'SaldoVacacionesInicial')
BEGIN
    ALTER TABLE dbo.Empleado
        ADD SaldoVacacionesInicial DECIMAL(10,2) NOT NULL
            CONSTRAINT DF_Empleado_SaldoVacacionesInicial DEFAULT (0);
    PRINT '  [+] Empleado.SaldoVacacionesInicial DECIMAL(10,2) agregada.';
END
ELSE PRINT '  [=] Empleado.SaldoVacacionesInicial ya existia.';
GO


/* =====================================================================================
   SECCION 2 - PLA-HU-013 y PLA-HU-014 : Vacaciones e incapacidades en la planilla
   -------------------------------------------------------------------------------------
   PlanillaDetalle no tiene NINGUNA columna para reflejar vacaciones ni incapacidades,
   que es literalmente lo que piden las HU 013 y 014.

   Convencion de precision detectada en PlanillaDetalle:
       horas/dias -> DECIMAL(10,2)      montos -> DECIMAL(18,2)

   NOT NULL DEFAULT 0 -> las filas de planilla existentes quedan intactas y el codigo
   del Sprint 3 sigue funcionando sin cambios.
   ===================================================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.PlanillaDetalle') AND name = 'DiasVacaciones')
BEGIN
    ALTER TABLE dbo.PlanillaDetalle
        ADD DiasVacaciones DECIMAL(10,2) NOT NULL
            CONSTRAINT DF_PlanillaDetalle_DiasVacaciones DEFAULT (0);
    PRINT '  [+] PlanillaDetalle.DiasVacaciones DECIMAL(10,2) agregada.';
END
ELSE PRINT '  [=] PlanillaDetalle.DiasVacaciones ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.PlanillaDetalle') AND name = 'MontoVacaciones')
BEGIN
    ALTER TABLE dbo.PlanillaDetalle
        ADD MontoVacaciones DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PlanillaDetalle_MontoVacaciones DEFAULT (0);
    PRINT '  [+] PlanillaDetalle.MontoVacaciones DECIMAL(18,2) agregada.';
END
ELSE PRINT '  [=] PlanillaDetalle.MontoVacaciones ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.PlanillaDetalle') AND name = 'DiasIncapacidad')
BEGIN
    ALTER TABLE dbo.PlanillaDetalle
        ADD DiasIncapacidad DECIMAL(10,2) NOT NULL
            CONSTRAINT DF_PlanillaDetalle_DiasIncapacidad DEFAULT (0);
    PRINT '  [+] PlanillaDetalle.DiasIncapacidad DECIMAL(10,2) agregada.';
END
ELSE PRINT '  [=] PlanillaDetalle.DiasIncapacidad ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.PlanillaDetalle') AND name = 'MontoIncapacidad')
BEGIN
    ALTER TABLE dbo.PlanillaDetalle
        ADD MontoIncapacidad DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_PlanillaDetalle_MontoIncapacidad DEFAULT (0);
    PRINT '  [+] PlanillaDetalle.MontoIncapacidad DECIMAL(18,2) agregada.';
END
ELSE PRINT '  [=] PlanillaDetalle.MontoIncapacidad ya existia.';
GO


/* =====================================================================================
   SECCION 3 - PLA-HU-017 : Datos de salida del empleado
   -------------------------------------------------------------------------------------
   Empleado solo tiene un bit 'Estado'. No hay forma de saber CUANDO ni POR QUE salio
   una persona, y sin eso no se puede calcular preaviso ni cesantia.

   Seguro para EF: EmpleadoAD mapea por convencion (solo ToTable + HasKey). Columnas
   nuevas en la tabla que el modelo no declare simplemente se ignoran. No rompe nada.
   ===================================================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Empleado') AND name = 'FechaSalida')
BEGIN
    ALTER TABLE dbo.Empleado ADD FechaSalida DATE NULL;
    PRINT '  [+] Empleado.FechaSalida agregada.';
END
ELSE PRINT '  [=] Empleado.FechaSalida ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.Empleado') AND name = 'MotivoSalida')
BEGIN
    ALTER TABLE dbo.Empleado ADD MotivoSalida NVARCHAR(50) NULL;
    PRINT '  [+] Empleado.MotivoSalida agregada.';
END
ELSE PRINT '  [=] Empleado.MotivoSalida ya existia.';
GO

/* SIN CHECK a proposito: el equipo no usa CHECK en las tablas de RRHH (solo en
   Factura, OrdenCompra, PlanillaPeriodo, InventarioMovimiento). Los literales se
   controlan desde PROmaderas.Abstracciones.Catalogos, igual que Vacacion.Estado
   e Incapacidad.TipoIncapacidad, que tampoco tienen CHECK.

   MotivoSalida define el derecho a preaviso/cesantia (Codigo de Trabajo CR):
     'Despido con responsabilidad'  -> preaviso SI, cesantia SI
     'Despido sin responsabilidad'  -> preaviso NO, cesantia NO
     'Renuncia'                     -> preaviso NO, cesantia NO
     'Mutuo acuerdo' / 'Pension'    -> segun lo pactado                                */


/* =====================================================================================
   SECCION 4 - PLA-HU-017 : Tabla Liquidacion
   -------------------------------------------------------------------------------------
   La liquidacion es un DOCUMENTO que se emite y se conserva, no un calculo al vuelo.
   Los montos se congelan al momento del calculo, para que sigan siendo evidencia
   valida aunque despues cambien los parametros de planilla.
   ===================================================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Liquidacion' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Liquidacion
    (
        IdLiquidacion               INT IDENTITY(1,1) NOT NULL,
        IdEmpleado                  INT            NOT NULL,

        FechaCalculo                DATETIME       NOT NULL CONSTRAINT DF_Liquidacion_FechaCalculo DEFAULT (GETDATE()),
        FechaIngreso                DATE           NOT NULL,   -- congelada al calcular
        FechaSalida                 DATE           NOT NULL,
        MotivoSalida                NVARCHAR(50)   NOT NULL,

        AniosLaborados              DECIMAL(10,2)  NOT NULL CONSTRAINT DF_Liquidacion_Anios     DEFAULT (0),
        SalarioPromedio             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Liquidacion_SalProm   DEFAULT (0),

        -- Los 4 rubros que pide la HU
        DiasVacacionesPendientes    DECIMAL(10,2)  NOT NULL CONSTRAINT DF_Liquidacion_DiasVac   DEFAULT (0),
        MontoVacaciones             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Liquidacion_MontoVac  DEFAULT (0),
        MontoAguinaldoProporcional  DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Liquidacion_Aguinaldo DEFAULT (0),
        MontoPreaviso               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Liquidacion_Preaviso  DEFAULT (0),
        MontoCesantia               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Liquidacion_Cesantia  DEFAULT (0),

        OtrosMontos                 DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Liquidacion_Otros     DEFAULT (0),
        TotalLiquidacion            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Liquidacion_Total     DEFAULT (0),

        Estado                      NVARCHAR(30)   NOT NULL CONSTRAINT DF_Liquidacion_Estado    DEFAULT ('Calculada'),
        Observacion                 NVARCHAR(500)  NULL,
        IdUsuarioRegistro           INT            NOT NULL,
        Activa                      BIT            NOT NULL CONSTRAINT DF_Liquidacion_Activa    DEFAULT (1),

        CONSTRAINT PK_Liquidacion PRIMARY KEY CLUSTERED (IdLiquidacion),
        CONSTRAINT FK_Liquidacion_Empleado
            FOREIGN KEY (IdEmpleado)        REFERENCES dbo.Empleado (IdEmpleado),
        CONSTRAINT FK_Liquidacion_Usuario
            FOREIGN KEY (IdUsuarioRegistro) REFERENCES dbo.Usuario  (IdUsuario),
        CONSTRAINT CK_Liquidacion_Estado
            CHECK (Estado IN ('Calculada','Aprobada','Pagada','Anulada')),
        CONSTRAINT CK_Liquidacion_Fechas
            CHECK (FechaSalida >= FechaIngreso)
    );

    CREATE INDEX IX_Liquidacion_IdEmpleado ON dbo.Liquidacion (IdEmpleado);
    PRINT '  [+] Tabla Liquidacion creada.';
END
ELSE PRINT '  [=] Tabla Liquidacion ya existia.';
GO


/* =====================================================================================
   SECCION 5 - PLA-HU-018 : Polizas del INS
   -------------------------------------------------------------------------------------
   Se copia el patron que el equipo YA usa en la BD:
       DeduccionInterna (maestro)  +  EmpleadoDeduccion (union N:M)
   ->  PolizaINS        (maestro)  +  EmpleadoPoliza    (union N:M)

   Razon: una poliza real del INS (Riesgos del Trabajo) es UNA poliza de la empresa que
   cubre a VARIOS empleados. Meter IdEmpleado dentro de la poliza obligaria a duplicar
   la misma poliza una vez por trabajador.
   ===================================================================================== */

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolizaINS' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PolizaINS
    (
        IdPoliza          INT IDENTITY(1,1) NOT NULL,
        NumeroPoliza      NVARCHAR(50)   NOT NULL,
        TipoPoliza        NVARCHAR(100)  NOT NULL,   -- 'Riesgos del Trabajo', 'Vida', 'Otra'
        Aseguradora       NVARCHAR(100)  NOT NULL CONSTRAINT DF_PolizaINS_Aseguradora DEFAULT ('INS'),

        FechaInicio       DATE           NOT NULL,
        FechaVencimiento  DATE           NOT NULL,

        MontoAsegurado    DECIMAL(18,2)  NULL,
        Prima             DECIMAL(18,2)  NULL,

        Estado            BIT            NOT NULL CONSTRAINT DF_PolizaINS_Estado        DEFAULT (1),
        Observacion       NVARCHAR(250)  NULL,
        FechaCreacion     DATETIME       NOT NULL CONSTRAINT DF_PolizaINS_FechaCreacion DEFAULT (GETDATE()),

        CONSTRAINT PK_PolizaINS PRIMARY KEY CLUSTERED (IdPoliza),
        CONSTRAINT UQ_PolizaINS_NumeroPoliza UNIQUE (NumeroPoliza),
        CONSTRAINT CK_PolizaINS_Fechas CHECK (FechaVencimiento >= FechaInicio)
    );
    PRINT '  [+] Tabla PolizaINS creada.';
END
ELSE PRINT '  [=] Tabla PolizaINS ya existia.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmpleadoPoliza' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.EmpleadoPoliza
    (
        IdEmpleadoPoliza  INT IDENTITY(1,1) NOT NULL,
        IdEmpleado        INT   NOT NULL,
        IdPoliza          INT   NOT NULL,
        FechaAsignacion   DATE  NOT NULL CONSTRAINT DF_EmpleadoPoliza_FechaAsig DEFAULT (CAST(GETDATE() AS DATE)),
        FechaExclusion    DATE  NULL,           -- NULL = sigue cubierto
        Activa            BIT   NOT NULL CONSTRAINT DF_EmpleadoPoliza_Activa    DEFAULT (1),

        CONSTRAINT PK_EmpleadoPoliza PRIMARY KEY CLUSTERED (IdEmpleadoPoliza),
        CONSTRAINT FK_EmpleadoPoliza_Empleado
            FOREIGN KEY (IdEmpleado) REFERENCES dbo.Empleado  (IdEmpleado),
        CONSTRAINT FK_EmpleadoPoliza_Poliza
            FOREIGN KEY (IdPoliza)   REFERENCES dbo.PolizaINS (IdPoliza),
        CONSTRAINT UQ_EmpleadoPoliza UNIQUE (IdEmpleado, IdPoliza)
    );

    CREATE INDEX IX_EmpleadoPoliza_IdPoliza ON dbo.EmpleadoPoliza (IdPoliza);
    PRINT '  [+] Tabla EmpleadoPoliza creada.';
END
ELSE PRINT '  [=] Tabla EmpleadoPoliza ya existia.';
GO


/* =====================================================================================
   SECCION 6 - *** FIX CRITICO *** PLA-HU-019 : versionado de ParametroPlanilla
   -------------------------------------------------------------------------------------
   PROBLEMA
   --------
   ParametroPlanilla se creo asi:

       [NombreParametro] nvarchar(100) NOT NULL,
       [FechaInicio] date NOT NULL, [FechaFin] date NULL, [Estado] bit NOT NULL,
       UNIQUE NONCLUSTERED ([NombreParametro] ASC)     <-- ESTE ES EL PROBLEMA

   Las columnas FechaInicio/FechaFin/Estado existen para VERSIONAR el parametro, pero
   el UNIQUE sobre NombreParametro lo IMPIDE: solo puede haber UNA fila por nombre.
   Nunca podrian convivir la version vieja (cerrada con FechaFin) y la nueva.

   Consecuencia: PLA-HU-019 pide "adaptar el sistema a cambios legales SIN AFECTAR
   DATOS HISTORICOS". Con el UNIQUE actual, cambiar un parametro obliga a EDITAR la
   fila -> las planillas ya cerradas se recalcularian con el valor nuevo. La HU no se
   puede cumplir.

   SOLUCION
   --------
   Reemplazar UNIQUE(NombreParametro) por UNIQUE(NombreParametro, FechaInicio).
   Asi cada parametro puede tener N versiones, una por fecha de vigencia, y la logica
   de negocio resuelve el valor vigente con:

       WHERE NombreParametro = @p
         AND @fecha >= FechaInicio
         AND (FechaFin IS NULL OR @fecha <= FechaFin)
         AND Estado = 1

   El UNIQUE original no tiene nombre explicito (SQL Server le puso uno autogenerado
   tipo UQ__Parametr__XXXXXXXX), asi que se busca en sys.key_constraints y se dropea
   dinamicamente.
   ===================================================================================== */

DECLARE @uqNombre  SYSNAME;
DECLARE @sqlDrop   NVARCHAR(500);

-- Buscar el UNIQUE definido SOLO sobre NombreParametro
SELECT TOP 1 @uqNombre = kc.name
FROM sys.key_constraints kc
JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id
                         AND ic.index_id  = kc.unique_index_id
JOIN sys.columns c        ON c.object_id  = ic.object_id
                         AND c.column_id  = ic.column_id
WHERE kc.parent_object_id = OBJECT_ID('dbo.ParametroPlanilla')
  AND kc.type = 'UQ'
GROUP BY kc.name
HAVING COUNT(*) = 1
   AND MAX(c.name) = 'NombreParametro';

IF @uqNombre IS NOT NULL
BEGIN
    SET @sqlDrop = N'ALTER TABLE dbo.ParametroPlanilla DROP CONSTRAINT ' + QUOTENAME(@uqNombre) + N';';
    EXEC sp_executesql @sqlDrop;
    PRINT '  [-] UNIQUE(NombreParametro) eliminado: ' + @uqNombre;
END
ELSE
    PRINT '  [=] No hay UNIQUE simple sobre NombreParametro (ya se habia quitado).';
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints
               WHERE parent_object_id = OBJECT_ID('dbo.ParametroPlanilla')
                 AND name = 'UQ_ParametroPlanilla_Nombre_Vigencia')
BEGIN
    ALTER TABLE dbo.ParametroPlanilla
        ADD CONSTRAINT UQ_ParametroPlanilla_Nombre_Vigencia
            UNIQUE (NombreParametro, FechaInicio);
    PRINT '  [+] UNIQUE(NombreParametro, FechaInicio) agregado. Versionado habilitado.';
END
ELSE PRINT '  [=] UQ_ParametroPlanilla_Nombre_Vigencia ya existia.';
GO


/* CONVENCION DE UNIDADES en ParametroPlanilla.Valor
   -------------------------------------------------
   *Porc / Porcentaje*  -> numero 0..100. HAY QUE DIVIDIR ENTRE 100 al usarlo.
                           ej: PorcentajeCCSS = 10.67  -> bruto * (10.67/100)
   *Factor*             -> multiplicador directo, NO se divide.
                           ej: PorcentajeHoraExtra = 1.5 -> valorHora * 1.5
   *Piso / Monto*       -> monto en colones, se usa tal cual.
                           ej: RentaTramo1Piso = 929000
   *Dias / Anios / Horas* -> cantidad, se usa tal cual.
                           ej: HorasMes = 240

   OJO: 'PorcentajeHoraExtra' se llama "Porcentaje" pero es un FACTOR (1.5), NO un
   porcentaje. Nombre heredado del SEED. No lo renombres, solo tenelo presente. */

/* =====================================================================================
   SECCION 7 - Parametros que consumen las HU 012, 013, 014, 017 y 019 (idempotente)
   -------------------------------------------------------------------------------------
   Valores de arranque. PLA-HU-019 permite cambiarlos desde la app (creando una version
   nueva, no editando estas filas).

   NOTA: si preferis mantener separados esquema y datos, movete esta seccion completa a
   scripts/PROmaderasDB_SEED.sql. No cambia nada mas.
   ===================================================================================== */

/* La v2 de este script insertaba 'VacacionesDiasPorMes', que duplica el concepto que el
   SEED ya siembra como 'DiasVacacionesPorMes'. El nombre bueno es el del SEED. Este DELETE
   limpia la fila mala en las bases donde la v2 ya se corrio. */
IF EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'VacacionesDiasPorMes')
BEGIN
    DELETE FROM dbo.ParametroPlanilla WHERE NombreParametro = 'VacacionesDiasPorMes';
    PRINT '  [-] Parametro duplicado VacacionesDiasPorMes eliminado (el bueno es DiasVacacionesPorMes).';
END
GO

/* El SEED siembra 'DiasVacacionesPorMes' con GETDATE(), asi que su vigencia arranca el dia
   en que se instalo la base. Todos los demas parametros arrancan el 2026-01-01. Si no se
   normaliza, al resolver el parametro vigente para un periodo de planilla anterior a esa
   fecha no se encuentra ninguna version y se rompe PLA-HU-012. */
IF EXISTS (SELECT 1 FROM dbo.ParametroPlanilla
           WHERE NombreParametro = 'DiasVacacionesPorMes' AND FechaInicio > '2026-01-01')
BEGIN
    UPDATE dbo.ParametroPlanilla
       SET FechaInicio = '2026-01-01'
     WHERE NombreParametro = 'DiasVacacionesPorMes';
    PRINT '  [~] DiasVacacionesPorMes: FechaInicio normalizada a 2026-01-01.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'IncapacidadCCSSPorcPatrono' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('IncapacidadCCSSPorcPatrono', 50.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'IncapacidadINSPorcPatrono' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('IncapacidadINSPorcPatrono', 0.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'PreavisoDiasPorAnio' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('PreavisoDiasPorAnio', 30.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'CesantiaDiasPorAnio' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('CesantiaDiasPorAnio', 19.5000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'CesantiaTopeAnios' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('CesantiaTopeAnios', 8.0000, '2026-01-01', NULL, 1);

/* PLA-HU-019: parametros del calculo de planilla que hoy estan hardcodeados en
   PlanillaLogica.cs (HorasMes = 240m y los 4 tramos de renta, lineas 126-141).
   El CCSS (PorcentajeCCSS) y el factor de hora extra (PorcentajeHoraExtra) ya los
   siembra PROmaderasDB_SEED.sql: no se repiten aca. */

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'HorasMes' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('HorasMes', 240.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'RentaTramo1Piso' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('RentaTramo1Piso', 929000.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'RentaTramo1Porc' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('RentaTramo1Porc', 10.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'RentaTramo2Piso' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('RentaTramo2Piso', 1360000.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'RentaTramo2Porc' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('RentaTramo2Porc', 15.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'RentaTramo3Piso' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('RentaTramo3Piso', 2392000.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'RentaTramo3Porc' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('RentaTramo3Porc', 20.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'RentaTramo4Piso' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('RentaTramo4Piso', 4783000.0000, '2026-01-01', NULL, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.ParametroPlanilla WHERE NombreParametro = 'RentaTramo4Porc' AND Estado = 1)
    INSERT INTO dbo.ParametroPlanilla (NombreParametro, Valor, FechaInicio, FechaFin, Estado)
    VALUES ('RentaTramo4Porc', 25.0000, '2026-01-01', NULL, 1);

PRINT '  [+] Parametros de planilla verificados/insertados.';
GO


/* =====================================================================================
   SECCION 8 - Verificacion
   ===================================================================================== */

PRINT '';
PRINT '--- Verificacion ---';

SELECT 'Empleado.SaldoVacacionesInicial' AS Objeto,
       IIF(COL_LENGTH('dbo.Empleado','SaldoVacacionesInicial')  IS NULL,'FALTA','OK') AS Estado
UNION ALL SELECT 'Empleado.FechaSalida',
       IIF(COL_LENGTH('dbo.Empleado','FechaSalida')             IS NULL,'FALTA','OK')
UNION ALL SELECT 'Empleado.MotivoSalida',
       IIF(COL_LENGTH('dbo.Empleado','MotivoSalida')            IS NULL,'FALTA','OK')
UNION ALL SELECT 'PlanillaDetalle.DiasVacaciones',
       IIF(COL_LENGTH('dbo.PlanillaDetalle','DiasVacaciones')   IS NULL,'FALTA','OK')
UNION ALL SELECT 'PlanillaDetalle.MontoVacaciones',
       IIF(COL_LENGTH('dbo.PlanillaDetalle','MontoVacaciones')  IS NULL,'FALTA','OK')
UNION ALL SELECT 'PlanillaDetalle.DiasIncapacidad',
       IIF(COL_LENGTH('dbo.PlanillaDetalle','DiasIncapacidad')  IS NULL,'FALTA','OK')
UNION ALL SELECT 'PlanillaDetalle.MontoIncapacidad',
       IIF(COL_LENGTH('dbo.PlanillaDetalle','MontoIncapacidad') IS NULL,'FALTA','OK')
UNION ALL SELECT 'Tabla Liquidacion',
       IIF(OBJECT_ID('dbo.Liquidacion')                         IS NULL,'FALTA','OK')
UNION ALL SELECT 'Tabla PolizaINS',
       IIF(OBJECT_ID('dbo.PolizaINS')                           IS NULL,'FALTA','OK')
UNION ALL SELECT 'Tabla EmpleadoPoliza',
       IIF(OBJECT_ID('dbo.EmpleadoPoliza')                      IS NULL,'FALTA','OK')
UNION ALL SELECT 'ParametroPlanilla versionable (UQ Nombre+FechaInicio)',
       IIF(EXISTS (SELECT 1 FROM sys.key_constraints
                   WHERE parent_object_id = OBJECT_ID('dbo.ParametroPlanilla')
                     AND name = 'UQ_ParametroPlanilla_Nombre_Vigencia'), 'OK','FALTA');
GO

PRINT '=== PROmaderasDB_SPRINT4.sql (v3) : FIN ===';
GO
