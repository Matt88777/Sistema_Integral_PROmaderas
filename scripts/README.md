# Base de datos — Cómo montar el entorno local

> **Léeme antes de correr cualquier script.**
> Esta carpeta tiene varios `.sql`. Solo **cuatro** son los que hay que correr, y en este orden.

---

## ⚠️ Cuál script correr (y cuál NO)

| Archivo | ¿Correrlo? | Qué es |
|---|---|---|
| `PROmaderasDB_NEW2.0.sql` | ✅ **SÍ — paso 1** | Esquema completo y actualizado. Crea la base y las 35 tablas. |
| `PROmaderasDB_SEED.sql` | ✅ **SÍ — paso 2** | Datos base sin los cuales el sistema NO funciona. Ver "Por qué el seed es obligatorio". |
| `PROmaderasDB_SPRINT4.sql` | ✅ **SÍ — paso 3** | Esquema del Sprint 4: columnas nuevas en `Empleado` y `PlanillaDetalle` + tablas `Liquidacion` / `PolizaINS` / `EmpleadoPoliza` + versionado de `ParametroPlanilla`. Aditivo e idempotente. |
| `PROmaderasDB_SPRINT5.sql` | ✅ **SÍ — paso 4** | Esquema del Sprint 5: categorías de tarima, imagen del producto, columnas de deducciones y datos de emergencia/salud del empleado. Aditivo e idempotente. |
| `PROmaderasDB_NEW.sql` (raíz del repo) | ❌ **NO** | Script aprobado en SC-603. Quedó desactualizado respecto al código: le faltan columnas y tablas que la app necesita. Se conserva **solo** como referencia del diseño original. **No se corre y no se edita.** |
| `Identity_Base.sql` | ❌ **NO** | Fósil del sistema anterior (Pedidos360). Las tablas `AspNet*` las crea `PROmaderasDB_NEW2.0.sql`. |
| `Identity_Clientes_Integracion.sql` | ❌ **NO** | Fósil. **El más peligroso:** siembra 4 roles que ya no existen (`Admin`, `Ventas`, `Operaciones`, `Cliente`). |
| `Seed_Productos_Clientes.sql` | ❌ **NO** | Fósil. Siembra un **menú de restaurante** en `Categoria` y `Producto`. |
| `SP_CrearUsuarioAdminIdentity.sql` | ❌ **NO** | Fósil. Reemplazado por `IdentitySeeder.cs`, que corre solo al arrancar la app. |
| `Crear-AdminIdentity.ps1` | ❌ **NO** | Fósil. Es el front-end en PowerShell del procedimiento anterior. |
| `CrearAdminTool/` | ❌ **NO** | Fósil. Utilitario de consola de Pedidos360. Ni siquiera está en el `.sln`. |

> **Los 6 fósiles no pueden correr por accidente:** todos empiezan con `USE Pedidos360DB;`, así que revientan en la primera línea. Se conservan por historial del repositorio, no porque sirvan.

---

## 🔴 El orden NO es una sugerencia

Los cuatro scripts se corren **1 → 2 → 3 → 4**. En particular:

**El `SEED` va SIEMPRE antes del `SPRINT5`.** El Sprint 5 deja `TipoTarima.IdCategoria` en `NOT NULL` y sin valor por defecto. Los `INSERT` de tarimas que hace el seed no envían categoría, así que si corrés el seed **después** falla con una violación de `NOT NULL`.

Si ya te pasó: borrá las filas de `TipoTarima`, corré el seed y volvé a correr el Sprint 5.

---

## Requisitos previos

- **SQL Server** (Developer o Express) + **SSMS**
- **.NET 8 SDK**
- Saber el nombre de tu instancia de SQL Server (ej. `localhost`, `.\SQLEXPRESS`, `MIPC\SSAS`)

---

## Paso 1 — Crear la base (esquema)

1. Abrí SSMS y conectate a tu instancia local.
2. **Verificá que NO exista ya una base llamada `PROmaderasDB_NEW`.**
   Si existe y querés empezar limpio:
   - Hacé un **backup** primero si tenés datos que te importan (clic derecho → Tasks → Back Up).
   - Después: clic derecho en la base → **Delete** → marcá **"Close existing connections"** → OK.
   - ⚠️ El script **no tiene guardas**: si la base ya existe, falla en la primera línea.
3. Abrí `scripts/PROmaderasDB_NEW2.0.sql` y ejecutalo (**F5**).
4. **Resultado esperado:** se crea la base con 35 tablas.
   Vas a ver 3 *warnings* sobre "maximum key length for a clustered index" en tablas `AspNet*` — **son normales**, los tira siempre ASP.NET Identity. Ignoralos.
   Al final debe decir `(2 rows affected)`.

> Esas 2 filas son importantes: son las dos migraciones de Identity que el script inserta en `__EFMigrationsHistory` para que EF no intente recrear las tablas `AspNet*` al arrancar. Si el script se corta a la mitad y esas filas no entran, la app revienta con `There is already an object named 'AspNetRoles'`.

---

## Paso 2 — Cargar los datos base (seed)

1. Abrí `scripts/PROmaderasDB_SEED.sql` y ejecutalo (**F5**).
2. **Resultado esperado:** al final aparece una tabla de resultados.
   **Verificá que el usuario `admin` tenga `IdUsuario = 1`.** Si no, algo salió mal — avisá antes de seguir.
3. Los conteos deben dar: 5 departamentos, 6 puestos, 5 roles, 5 empleados, 5 usuarios, 3 parámetros de planilla, 3 clientes, 4 tipos de tarima.

El seed es **idempotente**: si lo corrés dos veces, no duplica nada.

---

## Paso 3 — Aplicar el esquema del Sprint 4

1. Abrí `scripts/PROmaderasDB_SPRINT4.sql` y ejecutalo (**F5**).
2. **Resultado esperado:** los 11 chequeos de la sección de Verificación (al final del script) deben devolver `OK`.

Este script agrega columnas nuevas en `Empleado` (saldo inicial de vacaciones, datos de salida) y en `PlanillaDetalle` (vacaciones e incapacidades), crea las tablas `Liquidacion`, `PolizaINS` y `EmpleadoPoliza`, y cambia el `UNIQUE` de `ParametroPlanilla` a `(NombreParametro, FechaInicio)` para poder versionar parámetros. Es **aditivo e idempotente**.

---

## Paso 4 — Aplicar el esquema del Sprint 5

1. Abrí `scripts/PROmaderasDB_SPRINT5.sql` y ejecutalo (**F5**).
2. **Resultado esperado:** los **13 chequeos** de la primera grilla deben devolver `OK`, y en la segunda grilla:
   - Categorías sembradas: **2**
   - Tarimas sin categoría: **0**

Qué hace:

- **Categorías de tarima** — crea la tabla `Categoria`, siembra `Estadounidense` y `Europea`, agrega `TipoTarima.IdCategoria` con su llave foránea y clasifica las tarimas existentes según su nombre o su código.
- **Imagen del producto** — agrega `TipoTarima.ImagenUrl` con su valor por defecto y asigna la foto de las 4 tarimas del seed.
- **Deducciones** — agrega `NumeroCuotas` y `MontoTotal` a `EmpleadoDeduccion`. Sin estas dos columnas, la pantalla `Deducciones/Asignar` revienta.
- **Empleado** — agrega contacto de emergencia (nombre, teléfono, parentesco), alergias y medicamentos. Todas opcionales.

⚠️ **Si el script se detiene con un error que dice "hay tarimas sin categoría":** significa que tenés tarimas cuyo nombre y código no dicen ni `USA` ni `EUR`, así que no se pudieron clasificar solas. El mensaje te dice cuáles. Asignales `IdCategoria` a mano y volvé a correr el script. Es a propósito: preferimos que falle ruidosamente antes que meter todo en una categoría al azar.

---

## Paso 5 — Configurar `appsettings.json`

**Este archivo NO está en el repo** (está en `.gitignore`), porque cada uno tiene su propia instancia de SQL Server. **Tenés que crearlo vos.**

Creá `PROmaderas/appsettings.json` con este contenido, cambiando `TU_INSTANCIA` por la tuya:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_INSTANCIA;Database=PROmaderasDB_NEW;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> Si tu instancia lleva backslash (ej. `MIPC\SSAS`), en JSON hay que escribirlo doble: `Server=MIPC\\SSAS;...`

**Nunca commitees este archivo.** Ya está ignorado, pero no lo fuerces con `git add -f`.

---

## Paso 6 — Correr la app

```powershell
dotnet build
dotnet run --project PROmaderas
```

Al arrancar, ASP.NET Identity siembra solo los usuarios de login.

### Usuarios de prueba

Ojo con el dominio: es `@PROmaderas.local`, con **PRO en mayúscula**.

| Correo | Contraseña | Rol |
|---|---|---|
| `admin@PROmaderas.local` | `Admin123!` | Administrador |
| `gerente@PROmaderas.local` | `Gerente123!` | Gerente |
| `contador@PROmaderas.local` | `Contador123!` | Contador |
| `operador@PROmaderas.local` | `Operador123!` | Operador de Planta |
| `vendedor@PROmaderas.local` | `Vendedor123!` | Vendedor |

---

## Probar que todo funciona

1. Entrá como **vendedor** → creá una orden de compra con "Exportadora Del Valle S.A." + tarima 40x48 USA.
2. Entrá como **contador** → emití la factura de esa orden.
3. Entrá como **administrador** → abrí Productos (deben verse las 4 tarimas **con imagen y categoría**) y Deducciones → Asignar.

Si eso sale, el entorno está sano.

---

## Por qué el seed es obligatorio

Hay tablas que la app **no puede crear desde ninguna pantalla**: `Departamento`, `Puesto`, `dbo.Rol` y `dbo.Usuario`. No tienen CRUD. Si no corrés el seed:

- No podés crear empleados (el dropdown de Puesto está vacío).
- No podés crear órdenes (error: "El usuario '...' no existe en la tabla Usuario").
- Emitir facturas revienta con un error de llave foránea.

`Categoria` **sí** tiene repositorio, pero su seed igual es obligatorio: `TipoTarima.IdCategoria` es `NOT NULL`, así que sin categorías no se puede dar de alta ninguna tarima.

## La tabla `dbo.Usuario` vs. Identity

Son dos cosas distintas y confunde a todos:

- **`AspNetUsers`** = login (la maneja ASP.NET Identity, se siembra sola al arrancar).
- **`dbo.Usuario`** = tabla de negocio (la usan facturas, órdenes, planilla, inventario). La llena el seed.

El puente entre ambas es **el correo**: `AspNetUsers.Email` debe coincidir con `dbo.Usuario.Correo`. Por eso el seed usa exactamente los mismos correos que el `IdentitySeeder`.

Si agregan un usuario nuevo, hay que crearlo en los dos lados o el sistema no lo va a poder identificar.

---

## Problemas comunes

| Error | Causa | Solución |
|---|---|---|
| `There is already an object named 'AspNetRoles'` | Creaste la base con otro script (sin el fix de migraciones). | Borrá la base y corré `PROmaderasDB_NEW2.0.sql`. |
| `Invalid column name 'SalarioBase'` (o `JornadaLaboral`, `DeduccionCCSS`...) | Estás usando el `PROmaderasDB_NEW.sql` viejo de la raíz. | Borrá la base y corré `PROmaderasDB_NEW2.0.sql`. |
| `Invalid column name 'ImagenUrl'` o `'IdCategoria'` — revienta Productos | No corriste el paso 4. | Corré `PROmaderasDB_SPRINT5.sql`. |
| `Invalid column name 'MontoTotal'` / `'NumeroCuotas'` — revienta Deducciones → Asignar | No corriste el paso 4. | Corré `PROmaderasDB_SPRINT5.sql`. |
| `Cannot insert the value NULL into column 'IdCategoria'` al correr el seed | Corriste el seed **después** del Sprint 5. | Borrá las filas de `TipoTarima`, corré el seed y volvé a correr el Sprint 5. |
| `El usuario '...' no existe en la tabla Usuario` | No corriste el seed. | Corré `PROmaderasDB_SEED.sql`. |
| `Database 'PROmaderasDB_NEW' already exists` | La base ya existía. | Borrala primero (ver Paso 1). |
| La app no conecta a la base | Falta el `appsettings.json` o la instancia está mal escrita. | Ver Paso 5. |
| La app no arranca y tira `DirectoryNotFoundException` | Falta la carpeta `Imagenes/` en la raíz de la solución. | Está versionada en el repo; verificá que el `git pull` la haya traído. |
| Los productos sin foto muestran una imagen rota | Falta `Imagenes/sin-imagen.jpg`. | Es el valor por defecto de `ImagenUrl`; el archivo tiene que existir. |

---

## Migraciones de Entity Framework

**El esquema lo gobiernan estos scripts. No se usan migraciones de EF sobre el contexto de Negocio.**

No corras `dotnet ef migrations add` ni `dotnet ef database update` sobre `Contexto`. El modelo de C# no describe el esquema real (hay propiedades `[NotMapped]`, nombres de tabla remapeados por Fluent API y nullabilidad que no coincide), así que EF generaría un esquema distinto al que existe.

`Program.cs` migra **únicamente** el contexto de Identity. Eso es intencional.

**Chequeo rápido en una instalación limpia:**

```sql
SELECT COUNT(1) FROM __EFMigrationsHistory;   -- debe dar 2
```

Si da más de 2, alguien corrió migraciones de Negocio contra la base: hay dos fuentes de verdad del esquema y hay que avisarle al equipo.

---

## Deuda técnica conocida

- La base crea tablas duplicadas sin uso: `HistorialSalario` y `SalarioHistorial` (el código solo usa la segunda), y `TipoDeduccion` y `DeduccionInterna` (el código solo usa la segunda). No rompen nada, pero convendría limpiarlas.
- `dbo.Usuario` no tiene pantalla de alta. Usuarios nuevos hay que meterlos por SQL.
- Los 6 fósiles de Pedidos360 siguen en esta carpeta. Están muertos y no pueden correr, pero ensucian el directorio.
