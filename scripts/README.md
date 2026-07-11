# Base de datos — Cómo montar el entorno local

> **Léeme antes de correr cualquier script.**
> Esta carpeta tiene varios `.sql`. Solo dos son los que hay que correr, y en este orden.

---

## ⚠️ Cuál script correr (y cuál NO)

| Archivo | ¿Correrlo? | Qué es |
|---|---|---|
| `PROmaderasDB_NEW2.0.sql` | ✅ **SÍ — paso 1** | Esquema completo y actualizado. Crea la base y las 35 tablas. |
| `PROmaderasDB_SEED.sql` | ✅ **SÍ — paso 2** | Datos base sin los cuales el sistema NO funciona. Ver "Por qué el seed es obligatorio". |
| `PROmaderasDB_NEW.sql` (raíz del repo) | ❌ **NO** | Script aprobado en SC-603. Quedó desactualizado respecto al código del Sprint 3: le faltan columnas y tablas que la app necesita. Se conserva como referencia del diseño original. |
| Otros `.sql` de esta carpeta | ❌ **NO** | Scripts de sprints anteriores. Obsoletos. |

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

---

## Paso 2 — Cargar los datos base (seed)

1. Abrí `scripts/PROmaderasDB_SEED.sql` y ejecutalo (**F5**).
2. **Resultado esperado:** al final aparece una tabla de resultados.
   **Verificá que el usuario `admin` tenga `IdUsuario = 1`.** Si no, algo salió mal — avisá antes de seguir.
3. Los conteos deben dar: 5 departamentos, 6 puestos, 5 roles, 5 empleados, 5 usuarios, 3 parámetros de planilla, 3 clientes, 4 tipos de tarima.

El seed es **idempotente**: si lo corrés dos veces, no duplica nada.

---

## Paso 3 — Configurar `appsettings.json`

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

> **Nunca commitees este archivo.** Ya está ignorado, pero no lo fuerces con `git add -f`.

---

## Paso 4 — Correr la app

```bash
dotnet build
dotnet run --project PROmaderas
```

Al arrancar, ASP.NET Identity siembra solo los usuarios de login.

### Usuarios de prueba

| Correo | Contraseña | Rol |
|---|---|---|
| `admin@PROmaderas.local` | `Admin123!` | Administrador |
| `gerente@PROmaderas.local` | `Gerente123!` | Gerente |
| `contador@PROmaderas.local` | `Contador123!` | Contador |
| `operador@PROmaderas.local` | `Operador123!` | Operador de Planta |
| `vendedor@PROmaderas.local` | `Vendedor123!` | Vendedor |

---

## Probar que todo funciona

1. Entrá como **vendedor** → creá una **orden de compra** con "Exportadora Del Valle S.A." + tarima 40x48 USA.
2. Entrá como **contador** → **emití la factura** de esa orden.

Si la factura sale, el entorno está sano.

---

## Por qué el seed es obligatorio

Hay tablas que **la app no puede crear desde ninguna pantalla**: `Departamento`, `Puesto`, `Rol` y `dbo.Usuario`. No tienen CRUD. Si no corrés el seed:

- **No podés crear empleados** (el dropdown de Puesto está vacío).
- **No podés crear órdenes** (error: *"El usuario '...' no existe en la tabla Usuario"*).
- **Emitir facturas revienta** con un error de FK.

### La tabla `dbo.Usuario` vs. Identity

Son **dos cosas distintas** y confunde a todos:

- **`AspNetUsers`** = login (la maneja ASP.NET Identity, se siembra sola al arrancar).
- **`dbo.Usuario`** = tabla de negocio (la usan facturas, órdenes, planilla, inventario). **La llena el seed.**

**El puente entre ambas es el correo:** `AspNetUsers.Email` debe coincidir con `dbo.Usuario.Correo`. Por eso el seed usa exactamente los mismos correos que el `IdentitySeeder`.

Si agregan un usuario nuevo, hay que crearlo **en los dos lados** o el sistema no lo va a poder identificar.

---

## Problemas comunes

| Error | Causa | Solución |
|---|---|---|
| `There is already an object named 'AspNetRoles'` | Creaste la base con otro script (sin el fix de migraciones). | Borrá la base y corré `PROmaderasDB_NEW2.0.sql`. |
| `Invalid column name 'SalarioBase'` (o `JornadaLaboral`, `DeduccionCCSS`...) | Estás usando el `PROmaderasDB_NEW.sql` viejo. | Borrá la base y corré `PROmaderasDB_NEW2.0.sql`. |
| `El usuario '...' no existe en la tabla Usuario` | No corriste el seed. | Corré `PROmaderasDB_SEED.sql`. |
| `Database 'PROmaderasDB_NEW' already exists` | La base ya existía. | Borrala primero (ver Paso 1). |
| La app no conecta a la base | Falta el `appsettings.json` o la instancia está mal. | Ver Paso 3. |

---

## Deuda técnica conocida

- La base crea **tablas duplicadas sin uso**: `HistorialSalario` y `SalarioHistorial` (el código solo usa la segunda), y `TipoDeduccion` y `DeduccionInterna` (el código solo usa la segunda). No rompen nada, pero convendría limpiarlas.
- `dbo.Usuario` no tiene pantalla de alta. Usuarios nuevos hay que meterlos por SQL.
