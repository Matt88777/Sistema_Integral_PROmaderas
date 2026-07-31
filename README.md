# PROMADERAS S.A. — Sistema Integral de Gestión

Sistema de gestión administrativa e inventario para **PROMADERAS S.A.**, fabricante de tarimas de madera. Reemplaza los procesos manuales (Excel y registros físicos) por una plataforma web integrada.

**Curso:** SC-702 Diseño y Desarrollo de Sistemas — Universidad Fidélitas, 2026
Continuación de SC-603 Análisis y Modelado de Requerimientos.

**Cliente / patrocinador:** Olger Jiménez Berrocal

---

## Estado actual

Sprint 5 en curso. Los módulos base están implementados y funcionando de punta a punta:

| Módulo | Estado |
|---|---|
| Seguridad y acceso por rol | ✅ |
| Empleados y usuarios | ✅ |
| Clientes | ✅ |
| Inventario y producción | ✅ |
| Órdenes de compra | ✅ |
| Facturación (emisión, pagos parciales, anulación) | ✅ |
| Planilla, vacaciones, liquidaciones, pólizas INS | ✅ |
| Reportes con exportación | ✅ |
| Dashboard de ingresos y egresos | ✅ |

Las operaciones sensibles (edición de clientes, cambios de estado de factura, liquidaciones) quedan registradas en la bitácora de auditoría.

---

## Requisitos previos

| Herramienta | Versión mínima |
|---|---|
| .NET SDK | 8.0 |
| SQL Server (Developer o Express) | 2019 / 2022 |
| SQL Server Management Studio (SSMS) | — |
| Visual Studio 2022 **o** VS Code con C# Dev Kit | — |

---

## Puesta en marcha

### 1. Clonar el repositorio

```bash
git clone <url-del-repo>
cd Promaderas_Sistema
```

### 2. Crear la base de datos

El procedimiento completo está en **[`scripts/README.md`](scripts/README.md)**. Son **cuatro** scripts que se corren en orden desde SSMS:

```text
1) scripts/PROmaderasDB_NEW2.0.sql    (esquema base)
2) scripts/PROmaderasDB_SEED.sql      (datos base obligatorios)
3) scripts/PROmaderasDB_SPRINT4.sql   (esquema del Sprint 4)
4) scripts/PROmaderasDB_SPRINT5.sql   (esquema del Sprint 5)
```

> ⚠️ **No corras el `PROmaderasDB_NEW.sql` de la raíz.** Es el diseño aprobado en SC-603 y se conserva únicamente como referencia histórica; le faltan tablas y columnas que la aplicación necesita hoy. **No se corre y no se edita.**

El seed es obligatorio: hay tablas (`Departamento`, `Puesto`, `dbo.Rol`, `dbo.Usuario`) que la aplicación no puede crear desde ninguna pantalla. Sin ellas no se pueden dar de alta empleados ni emitir facturas.

### 3. Configurar la cadena de conexión

`PROmaderas/appsettings.json` **no está en el repositorio** (está en `.gitignore`, porque cada quien tiene su propia instancia). Hay que crearlo. El contenido exacto está en [`scripts/README.md`](scripts/README.md), paso 5.

### 4. Restaurar dependencias y arrancar

```powershell
dotnet restore
dotnet build
dotnet run --project PROmaderas
```

O desde Visual Studio: **F5** con el perfil `https`.
Los puertos están en `PROmaderas/Properties/launchSettings.json`.

---

## Usuarios de prueba

Los siembra `IdentitySeeder.cs` en cada arranque de la aplicación.

⚠️ **El dominio lleva `PRO` en MAYÚSCULA**: `@PROmaderas.local`. Es el error más común al intentar entrar.

| Correo | Contraseña | Rol |
|---|---|---|
| `admin@PROmaderas.local` | `Admin123!` | Administrador |
| `gerente@PROmaderas.local` | `Gerente123!` | Gerente |
| `contador@PROmaderas.local` | `Contador123!` | Contador |
| `operador@PROmaderas.local` | `Operador123!` | Operador de Planta |
| `vendedor@PROmaderas.local` | `Vendedor123!` | Vendedor |

El correo y la contraseña del administrador se pueden cambiar antes del primer arranque agregando esto al `appsettings.json`:

```json
"IdentitySeed": {
  "AdminEmail": "tu@email.com",
  "AdminPassword": "OtraContrasena1!",
  "AdminNombre": "Tu Nombre"
}
```

> Si lo cambiás, ajustá también el correo del admin en `scripts/PROmaderasDB_SEED.sql`. La aplicación relaciona al usuario logueado con la tabla de negocio `dbo.Usuario` **por el correo**; si no coinciden, emitir facturas falla.

### Roles

| Rol | Alcance |
|---|---|
| `Administrador` | Acceso total |
| `Gerente` | Supervisión de clientes, órdenes, inventario y reportes |
| `Contador` | Facturación, pagos y planilla |
| `Operador de Planta` | Producción e inventario |
| `Vendedor` | Clientes y órdenes de compra |

Los clientes **no** son usuarios del sistema: el registro público está deshabilitado.

---

## Arquitectura

Tres capas en proyectos separados:

```text
PROmaderas/                      UI — Controllers, Views, wwwroot
PROmaderas.Abstracciones/        Modelos (sufijo AD), catálogos, DTOs e interfaces
PROmaderas.AccesoADatos/         DbContext (clase Contexto) y repositorios
PROmaderas.LogicaDeNegocio/      Servicios de negocio
```

Convenciones que conviene conocer antes de tocar el código:

- **Los modelos llevan sufijo `AD`** (`EmpleadoAD`, `FacturaAD`, ...).
- **Algunos modelos se mapean a tablas con otro nombre** vía Fluent API (por ejemplo `ProductoAD` → tabla `TipoTarima`). El mapeo vive en `PROmaderas.AccesoADatos/Contexto.cs`.
- **Los catálogos de literales** (estados de factura, formas de pago, motivos de salida) están en `PROmaderas.Abstracciones/Catalogos`, no en la UI. Varias de esas columnas no tienen `CHECK` en la base: el catálogo es la única defensa contra un typo.
- **El esquema lo gobiernan los scripts SQL**, no las migraciones de EF. No corras `dotnet ef database update` sobre el contexto de Negocio. `Program.cs` migra únicamente el contexto de Identity, y eso es intencional. El detalle está en [`scripts/README.md`](scripts/README.md).
- **`AspNetUsers` (login) y `dbo.Usuario` (negocio) son tablas distintas.** El puente entre ambas es el correo.

---

## Tecnologías

**Backend:** ASP.NET Core 8 MVC · Entity Framework Core 8 · ASP.NET Identity · C#
**Base de datos:** SQL Server · SSMS
**Frontend:** HTML5 · CSS3 · Bootstrap · JavaScript
**Herramientas:** Visual Studio 2022 · Git · GitHub

---

## Documentos del proyecto (SC-603)

IN01 Alcance · IN02 Factibilidad · AN01 Requerimientos · DN01 Arquitectura · Capa Final (mockups)

---

## Integrantes

- Mattias Jiménez Bogantes
- Angie Melissa Borbón Arias
- Yasser Enrique Mora León
- Allison Daniela Murillo Delgado

**Universidad Fidélitas** — Ingeniería en Sistemas de Computación

---

## Licencia

Proyecto desarrollado con fines académicos.
