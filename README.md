# PROMADERAS

Sistema Integral de Gestión Administrativa e Inventario
Proyecto de Diseño y Desarrollo de Sistemas - Universidad Fidélitas 2026

---

## Requisitos previos

| Herramienta                                     | Versión mínima |
| ----------------------------------------------- | -------------- |
| .NET SDK                                        | 8.0            |
| SQL Server (Express o superior)                 | 2019 / 2022    |
| Visual Studio 2022 **o** VS Code con C# Dev Kit | —              |

---

## 1. Clonar el repositorio

```bash
git clone <url-del-repo>
cd Promaderas_Sistema
```

---

## 2. Crear la base de datos

Ejecuta el script SQL que viene en la raíz del repositorio:

```text
PROmaderasDB_NEW.sql
```

desde SQL Server Management Studio (SSMS) o Azure Data Studio, conectado a tu instancia local de SQL Server.

Esto crea la base `PROmaderasDB_NEW` con:

* Tablas del sistema (Cliente, TipoTarima, OrdenCompra, Factura, planilla, etc.)
* Relaciones y llaves foráneas
* Roles base (`Administrador`, `Gerente`, `Contador`, `Operador de Planta`, `Vendedor`)
* Departamentos, puestos y empleado administrador
* Catálogo inicial de tipos de tarima
* Parámetros y deducciones de planilla

---

## 3. Verificar la cadena de conexión

El archivo `PROmaderas/appsettings.json` ya apunta a una instancia local por defecto:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=PROmaderasDB_NEW;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False"
}
```

Si tu SQL Server local está en otra instancia (por ejemplo `localhost\SQLEXPRESS` o un nombre distinto), edita el `Server=` antes de arrancar.

Si usas autenticación SQL en lugar de Windows, reemplaza por:

```text
Server=localhost;Database=PROmaderasDB_NEW;User Id=sa;Password=TuPassword;TrustServerCertificate=True;
```

---

## 4. Restaurar dependencias y arrancar

```powershell
dotnet restore
dotnet run --project PROmaderas
```

O desde Visual Studio:

* Presiona **F5**
* Ejecuta el perfil `https` o `http`

La aplicación queda disponible en:

```text
https://localhost:7xxx
http://localhost:5xxx
```

(ver `Properties/launchSettings.json` para los puertos exactos)

---

# Usuarios y roles de prueba

## Roles disponibles

| Rol                  | Descripción                                             |
| -------------------- | ------------------------------------------------------- |
| `Administrador`      | Acceso total al sistema                                 |
| `Gerente`            | Supervisión de clientes, órdenes, inventario y reportes |
| `Contador`           | Gestión de facturación, pagos y planilla                |
| `Operador de Planta` | Producción e inventario                                 |
| `Vendedor`           | Gestión de clientes y órdenes de compra                 |

---

## Usuario administrador inicial (seed automático)

| Campo          | Valor por defecto        |
| -------------- | ------------------------ |
| **Email**      | `admin@promaderas.local` |
| **Contraseña** | `Admin123!`              |
| **Rol**        | `Administrador`          |

Estos valores pueden modificarse antes de arrancar la aplicación agregando en `appsettings.json`:

```json
"IdentitySeed": {
  "AdminEmail": "tu@email.com",
  "AdminPassword": "OtraContrasena1!",
  "AdminNombre": "Tu Nombre"
}
```

---

## Crear usuarios adicionales con rol Administrador (opcional)

Usa el script incluido en:

```text
scripts/
```

```powershell
cd scripts
.\Crear-AdminIdentity.ps1
```

Con parámetros personalizados:

```powershell
.\Crear-AdminIdentity.ps1 -Correo "nuevo.admin@promaderas.com" -NombreCompleto "Ana López" -Telefono "88991234"
```

Los demás usuarios pueden registrarse desde la pantalla de administración interna del sistema.

---

# Módulos del sistema

* Gestión de usuarios y roles
* Gestión de clientes
* Inventario y producción de tarimas
* Órdenes de compra
* Facturación
* Planilla
* Reportes administrativos
* Dashboard general

---

# Arquitectura del proyecto

```text
PROmaderas/
PROmaderas.Abstracciones/
PROmaderas.AccesoADatos/
PROmaderas.LogicaDeNegocio/
```

---

# Tecnologías utilizadas

## Backend

* ASP.NET Core MVC
* Entity Framework Core
* ASP.NET Identity
* C#

## Base de Datos

* SQL Server
* SSMS

## Frontend

* HTML5
* CSS3
* Bootstrap
* JavaScript

## Herramientas

* Visual Studio 2022
* Git
* GitHub

---

# Estado actual del proyecto (Sprint 0 — PL01)

* Esquema de base de datos `PROmaderasDB_NEW` aprobado y aplicado.
* Mapeo de Entity Framework reapuntado a las tablas reales (`TipoTarima`,
  `Cliente`, `OrdenCompra`, `OrdenCompraDetalle`, `Factura`).
* Roles internos sincronizados con el AN01: `Administrador`, `Gerente`,
  `Contador`, `Operador de Planta`, `Vendedor`. El registro público de
  clientes está deshabilitado (los clientes no son usuarios del sistema).
* Módulos solo-lectura en este sprint: Productos (catálogo), Clientes
  (listado), Pedidos (listado), Facturación (listado).
* Módulos pendientes (vista "En construcción" en esta entrega):
  * Crear / editar / eliminar productos (requiere conectar `TipoTarima` y
    `InventarioMovimiento`).
  * Crear / editar / cancelar pedidos (requiere generar `NumeroOrden` y
    conectar `IdVendedor`).
  * Crear / editar facturas y registrar pagos.
  * Planilla (modelo relacional `PlanillaPeriodo` + `PlanillaDetalle`).

---

# Integrantes

* Mattias Jiménez Bogantes
* Angie Melissa Borbón Arias
* Yasser Enrique Mora León
* Allison Daniela Murillo Delgado

---

# Institución

Universidad Fidélitas
Ingeniería en Sistemas de Computación

---

# Licencia

Proyecto desarrollado con fines académicos.
