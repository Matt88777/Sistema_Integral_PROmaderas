USE [master]
GO
/****** Objeto: Database [PROmaderasDB_NEW] Fecha de script: 10/7/2026 19:35:57 ******/
CREATE DATABASE [PROmaderasDB_NEW]
GO
ALTER DATABASE [PROmaderasDB_NEW] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [PROmaderasDB_NEW].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [PROmaderasDB_NEW] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET ARITHABORT OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET  ENABLE_BROKER 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET  MULTI_USER 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [PROmaderasDB_NEW] SET DB_CHAINING OFF 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [PROmaderasDB_NEW] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [PROmaderasDB_NEW] SET QUERY_STORE = OFF
GO
USE [PROmaderasDB_NEW]
GO
/****** Objeto: Table [dbo].[__EFMigrationsHistory] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[AspNetRoleClaims] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[AspNetRoles] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[AspNetUserClaims] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[AspNetUserLogins] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[AspNetUserRoles] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[AspNetUsers] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[NombreCompleto] [nvarchar](max) NOT NULL,
	[IdEmpleado] [int] NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[AspNetUserTokens] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[BitacoraAuditoria] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BitacoraAuditoria](
	[IdBitacora] [int] IDENTITY(1,1) NOT NULL,
	[IdUsuario] [int] NULL,
	[TablaAfectada] [nvarchar](100) NOT NULL,
	[IdRegistroAfectado] [int] NULL,
	[Accion] [nvarchar](50) NOT NULL,
	[ValorAnterior] [nvarchar](max) NULL,
	[ValorNuevo] [nvarchar](max) NULL,
	[FechaAccion] [datetime] NOT NULL,
	[DireccionIP] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdBitacora] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Cliente] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cliente](
	[IdCliente] [int] IDENTITY(1,1) NOT NULL,
	[CedulaJuridica] [nvarchar](30) NOT NULL,
	[NombreCliente] [nvarchar](150) NOT NULL,
	[Telefono] [nvarchar](25) NULL,
	[Correo] [nvarchar](150) NULL,
	[Direccion] [nvarchar](250) NULL,
	[CondicionPago] [nvarchar](100) NULL,
	[Exonerado] [bit] NOT NULL,
	[PorcentajeExoneracion] [decimal](5, 2) NOT NULL,
	[Estado] [bit] NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdCliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[CedulaJuridica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[DeduccionInterna] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeduccionInterna](
	[IdDeduccion] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Monto] [decimal](18, 2) NULL,
	[Porcentaje] [decimal](5, 2) NULL,
	[EsPorcentaje] [bit] NOT NULL,
	[Activa] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdDeduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Departamento] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Departamento](
	[IdDepartamento] [int] IDENTITY(1,1) NOT NULL,
	[NombreDepartamento] [nvarchar](100) NOT NULL,
	[Estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdDepartamento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NombreDepartamento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Empleado] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Empleado](
	[IdEmpleado] [int] IDENTITY(1,1) NOT NULL,
	[Cedula] [nvarchar](25) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[PrimerApellido] [nvarchar](100) NOT NULL,
	[SegundoApellido] [nvarchar](100) NULL,
	[Telefono] [nvarchar](25) NULL,
	[Correo] [nvarchar](150) NULL,
	[Direccion] [nvarchar](250) NULL,
	[FechaIngreso] [date] NOT NULL,
	[IdPuesto] [int] NOT NULL,
	[Estado] [bit] NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
	[Departamento] [varchar](100) NULL,
	[SalarioBase] [decimal](18, 2) NULL,
	[TipoPago] [nvarchar](50) NULL,
	[JornadaLaboral] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdEmpleado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Cedula] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[EmpleadoDeduccion] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmpleadoDeduccion](
	[IdEmpleadoDeduccion] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NOT NULL,
	[IdDeduccion] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdEmpleadoDeduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Factura] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Factura](
	[IdFactura] [int] IDENTITY(1,1) NOT NULL,
	[NumeroFactura] [nvarchar](50) NOT NULL,
	[IdOrdenCompra] [int] NOT NULL,
	[IdCliente] [int] NOT NULL,
	[IdUsuarioEmisor] [int] NULL,
	[FechaEmision] [datetime] NOT NULL,
	[Estado] [nvarchar](50) NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
	[Impuesto] [decimal](18, 2) NOT NULL,
	[Exoneracion] [decimal](18, 2) NOT NULL,
	[Total] [decimal](18, 2) NOT NULL,
	[SaldoPendiente] [decimal](18, 2) NULL,
	[Activa] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdFactura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NumeroFactura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[HistorialEstadoFactura] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistorialEstadoFactura](
	[IdHistorialEstadoFactura] [int] IDENTITY(1,1) NOT NULL,
	[IdFactura] [int] NOT NULL,
	[EstadoAnterior] [nvarchar](50) NULL,
	[EstadoNuevo] [nvarchar](50) NOT NULL,
	[IdUsuarioCambio] [int] NOT NULL,
	[FechaCambio] [datetime] NOT NULL,
	[Observacion] [nvarchar](250) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdHistorialEstadoFactura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[HistorialEstadoOrden] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistorialEstadoOrden](
	[IdHistorialEstadoOrden] [int] IDENTITY(1,1) NOT NULL,
	[IdOrdenCompra] [int] NOT NULL,
	[EstadoAnterior] [nvarchar](50) NULL,
	[EstadoNuevo] [nvarchar](50) NOT NULL,
	[IdUsuarioCambio] [int] NOT NULL,
	[FechaCambio] [datetime] NOT NULL,
	[Observacion] [nvarchar](250) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdHistorialEstadoOrden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[HistorialSalario] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistorialSalario](
	[IdHistorialSalario] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NOT NULL,
	[SalarioBase] [decimal](18, 2) NOT NULL,
	[TipoPago] [nvarchar](50) NOT NULL,
	[JornadaLaboral] [nvarchar](50) NOT NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NULL,
	[IdUsuarioRegistro] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdHistorialSalario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Incapacidad] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Incapacidad](
	[IdIncapacidad] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NOT NULL,
	[TipoIncapacidad] [nvarchar](100) NOT NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NOT NULL,
	[Dias] [decimal](10, 2) NOT NULL,
	[Observacion] [nvarchar](250) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdIncapacidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[InventarioMovimiento] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventarioMovimiento](
	[IdMovimiento] [int] IDENTITY(1,1) NOT NULL,
	[IdTipoTarima] [int] NOT NULL,
	[IdUsuarioRegistro] [int] NOT NULL,
	[TipoMovimiento] [nvarchar](30) NOT NULL,
	[Cantidad] [int] NOT NULL,
	[FechaMovimiento] [datetime] NOT NULL,
	[Motivo] [nvarchar](250) NULL,
	[IdProduccion] [int] NULL,
	[IdOrdenCompra] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdMovimiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Licencia] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Licencia](
	[IdLicencia] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NOT NULL,
	[TipoLicencia] [nvarchar](100) NOT NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NOT NULL,
	[Dias] [decimal](10, 2) NOT NULL,
	[ConGoceSalarial] [bit] NOT NULL,
	[Observacion] [nvarchar](250) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdLicencia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[OrdenCompra] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrdenCompra](
	[IdOrdenCompra] [int] IDENTITY(1,1) NOT NULL,
	[NumeroOrden] [nvarchar](50) NOT NULL,
	[IdCliente] [int] NOT NULL,
	[IdVendedor] [int] NOT NULL,
	[FechaOrden] [datetime] NOT NULL,
	[Estado] [nvarchar](50) NOT NULL,
	[Observacion] [nvarchar](250) NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
	[Impuesto] [decimal](18, 2) NOT NULL,
	[Total] [decimal](18, 2) NOT NULL,
	[Activa] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdOrdenCompra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NumeroOrden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[OrdenCompraDetalle] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrdenCompraDetalle](
	[IdOrdenCompraDetalle] [int] IDENTITY(1,1) NOT NULL,
	[IdOrdenCompra] [int] NOT NULL,
	[IdTipoTarima] [int] NOT NULL,
	[Cantidad] [int] NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdOrdenCompraDetalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PagoFactura] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PagoFactura](
	[IdPagoFactura] [int] IDENTITY(1,1) NOT NULL,
	[IdFactura] [int] NOT NULL,
	[FechaPago] [datetime] NOT NULL,
	[Monto] [decimal](18, 2) NOT NULL,
	[FormaPago] [nvarchar](50) NOT NULL,
	[Referencia] [nvarchar](100) NULL,
	[IdUsuarioRegistro] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPagoFactura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[ParametroPlanilla] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ParametroPlanilla](
	[IdParametroPlanilla] [int] IDENTITY(1,1) NOT NULL,
	[NombreParametro] [nvarchar](100) NOT NULL,
	[Valor] [decimal](18, 4) NOT NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NULL,
	[Estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdParametroPlanilla] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NombreParametro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PlanillaDeduccionDetalle] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanillaDeduccionDetalle](
	[IdPlanillaDeduccionDetalle] [int] IDENTITY(1,1) NOT NULL,
	[IdPlanillaDetalle] [int] NOT NULL,
	[IdTipoDeduccion] [int] NOT NULL,
	[Monto] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPlanillaDeduccionDetalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PlanillaDetalle] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanillaDetalle](
	[IdPlanillaDetalle] [int] IDENTITY(1,1) NOT NULL,
	[IdPlanillaPeriodo] [int] NOT NULL,
	[IdEmpleado] [int] NOT NULL,
	[HorasOrdinarias] [decimal](10, 2) NOT NULL,
	[HorasExtra] [decimal](10, 2) NOT NULL,
	[SalarioBase] [decimal](18, 2) NOT NULL,
	[MontoHorasExtra] [decimal](18, 2) NOT NULL,
	[SalarioBruto] [decimal](18, 2) NOT NULL,
	[TotalDeducciones] [decimal](18, 2) NOT NULL,
	[SalarioNeto] [decimal](18, 2) NOT NULL,
	[DeduccionCCSS] [decimal](18, 2) NOT NULL,
	[DeduccionRenta] [decimal](18, 2) NOT NULL,
	[DeduccionesInternas] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPlanillaDetalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PlanillaPeriodo] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanillaPeriodo](
	[IdPlanillaPeriodo] [int] IDENTITY(1,1) NOT NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NOT NULL,
	[TipoPeriodo] [nvarchar](50) NOT NULL,
	[Estado] [nvarchar](50) NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
	[IdUsuarioCreacion] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPlanillaPeriodo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Produccion] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Produccion](
	[IdProduccion] [int] IDENTITY(1,1) NOT NULL,
	[IdTipoTarima] [int] NOT NULL,
	[IdUsuarioRegistro] [int] NOT NULL,
	[FechaProduccion] [date] NOT NULL,
	[Cantidad] [int] NOT NULL,
	[Observacion] [nvarchar](250) NULL,
	[FechaRegistro] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdProduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Puesto] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Puesto](
	[IdPuesto] [int] IDENTITY(1,1) NOT NULL,
	[NombrePuesto] [nvarchar](100) NOT NULL,
	[IdDepartamento] [int] NOT NULL,
	[Estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdPuesto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Rol] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rol](
	[IdRol] [int] IDENTITY(1,1) NOT NULL,
	[NombreRol] [nvarchar](50) NOT NULL,
	[Descripcion] [nvarchar](200) NULL,
	[Estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdRol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NombreRol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SalarioHistorial] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SalarioHistorial](
	[IdHistorial] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NOT NULL,
	[SalarioBase] [decimal](18, 2) NULL,
	[TipoPago] [nvarchar](50) NULL,
	[JornadaLaboral] [nvarchar](50) NULL,
	[FechaCambio] [datetime] NOT NULL,
	[UsuarioResponsable] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdHistorial] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[TipoDeduccion] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoDeduccion](
	[IdTipoDeduccion] [int] IDENTITY(1,1) NOT NULL,
	[NombreDeduccion] [nvarchar](100) NOT NULL,
	[Tipo] [nvarchar](50) NOT NULL,
	[Porcentaje] [decimal](5, 2) NULL,
	[MontoFijo] [decimal](18, 2) NULL,
	[EsLegal] [bit] NOT NULL,
	[Estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdTipoDeduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[TipoTarima] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoTarima](
	[IdTipoTarima] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [nvarchar](50) NOT NULL,
	[Nombre] [nvarchar](150) NOT NULL,
	[Medida] [nvarchar](100) NOT NULL,
	[Descripcion] [nvarchar](250) NULL,
	[PrecioUnitario] [decimal](18, 2) NOT NULL,
	[StockMinimo] [int] NOT NULL,
	[Estado] [bit] NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IdTipoTarima] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Usuario] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario](
	[IdUsuario] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NOT NULL,
	[IdRol] [int] NOT NULL,
	[NombreUsuario] [nvarchar](100) NOT NULL,
	[Correo] [nvarchar](150) NOT NULL,
	[ContrasenaHash] [nvarchar](500) NOT NULL,
	[Estado] [bit] NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
	[UltimoAcceso] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[IdUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[NombreUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Vacacion] Fecha de script: 10/7/2026 19:35:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vacacion](
	[IdVacacion] [int] IDENTITY(1,1) NOT NULL,
	[IdEmpleado] [int] NOT NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NOT NULL,
	[Dias] [decimal](10, 2) NOT NULL,
	[Estado] [nvarchar](50) NOT NULL,
	[Observacion] [nvarchar](250) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdVacacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_AspNetRoleClaims_RoleId] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [RoleNameIndex] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_AspNetUserClaims_UserId] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_AspNetUserLogins_UserId] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_AspNetUserRoles_RoleId] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [EmailIndex] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [UserNameIndex] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Bitacora_Fecha] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_Bitacora_Fecha] ON [dbo].[BitacoraAuditoria]
(
	[FechaAccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Cliente_Nombre] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_Cliente_Nombre] ON [dbo].[Cliente]
(
	[NombreCliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Empleado_Cedula] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_Empleado_Cedula] ON [dbo].[Empleado]
(
	[Cedula] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_Factura_Estado] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_Factura_Estado] ON [dbo].[Factura]
(
	[Estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_Factura_Fecha] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_Factura_Fecha] ON [dbo].[Factura]
(
	[FechaEmision] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_InventarioMovimiento_TipoTarima] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_InventarioMovimiento_TipoTarima] ON [dbo].[InventarioMovimiento]
(
	[IdTipoTarima] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [IX_OrdenCompra_Estado] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_OrdenCompra_Estado] ON [dbo].[OrdenCompra]
(
	[Estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Objeto: Index [IX_OrdenCompra_Fecha] Fecha de script: 10/7/2026 19:35:58 ******/
CREATE NONCLUSTERED INDEX [IX_OrdenCompra_Fecha] ON [dbo].[OrdenCompra]
(
	[FechaOrden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT (N'') FOR [NombreCompleto]
GO
ALTER TABLE [dbo].[BitacoraAuditoria] ADD  DEFAULT (getdate()) FOR [FechaAccion]
GO
ALTER TABLE [dbo].[Cliente] ADD  DEFAULT ((0)) FOR [Exonerado]
GO
ALTER TABLE [dbo].[Cliente] ADD  DEFAULT ((0)) FOR [PorcentajeExoneracion]
GO
ALTER TABLE [dbo].[Cliente] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[Cliente] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[DeduccionInterna] ADD  DEFAULT ((0)) FOR [EsPorcentaje]
GO
ALTER TABLE [dbo].[DeduccionInterna] ADD  DEFAULT ((1)) FOR [Activa]
GO
ALTER TABLE [dbo].[Departamento] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[Empleado] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[Empleado] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Factura] ADD  DEFAULT (getdate()) FOR [FechaEmision]
GO
ALTER TABLE [dbo].[Factura] ADD  DEFAULT ('Emitida') FOR [Estado]
GO
ALTER TABLE [dbo].[Factura] ADD  DEFAULT ((0)) FOR [Exoneracion]
GO
ALTER TABLE [dbo].[Factura] ADD  DEFAULT ((1)) FOR [Activa]
GO
ALTER TABLE [dbo].[HistorialEstadoFactura] ADD  DEFAULT (getdate()) FOR [FechaCambio]
GO
ALTER TABLE [dbo].[HistorialEstadoOrden] ADD  DEFAULT (getdate()) FOR [FechaCambio]
GO
ALTER TABLE [dbo].[InventarioMovimiento] ADD  DEFAULT (getdate()) FOR [FechaMovimiento]
GO
ALTER TABLE [dbo].[Licencia] ADD  DEFAULT ((0)) FOR [ConGoceSalarial]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT (getdate()) FOR [FechaOrden]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT ('Pendiente') FOR [Estado]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT ((0)) FOR [Subtotal]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT ((0)) FOR [Impuesto]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT ((0)) FOR [Total]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT ((1)) FOR [Activa]
GO
ALTER TABLE [dbo].[PagoFactura] ADD  DEFAULT (getdate()) FOR [FechaPago]
GO
ALTER TABLE [dbo].[ParametroPlanilla] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[PlanillaDetalle] ADD  DEFAULT ((0)) FOR [HorasOrdinarias]
GO
ALTER TABLE [dbo].[PlanillaDetalle] ADD  DEFAULT ((0)) FOR [HorasExtra]
GO
ALTER TABLE [dbo].[PlanillaDetalle] ADD  DEFAULT ((0)) FOR [MontoHorasExtra]
GO
ALTER TABLE [dbo].[PlanillaDetalle] ADD  DEFAULT ((0)) FOR [TotalDeducciones]
GO
ALTER TABLE [dbo].[PlanillaDetalle] ADD  DEFAULT ((0)) FOR [DeduccionCCSS]
GO
ALTER TABLE [dbo].[PlanillaDetalle] ADD  DEFAULT ((0)) FOR [DeduccionRenta]
GO
ALTER TABLE [dbo].[PlanillaPeriodo] ADD  DEFAULT ('Borrador') FOR [Estado]
GO
ALTER TABLE [dbo].[PlanillaPeriodo] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Produccion] ADD  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[Puesto] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[Rol] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[SalarioHistorial] ADD  DEFAULT (getdate()) FOR [FechaCambio]
GO
ALTER TABLE [dbo].[TipoDeduccion] ADD  DEFAULT ((0)) FOR [EsLegal]
GO
ALTER TABLE [dbo].[TipoDeduccion] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[TipoTarima] ADD  DEFAULT ((0)) FOR [StockMinimo]
GO
ALTER TABLE [dbo].[TipoTarima] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[TipoTarima] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT ((1)) FOR [Estado]
GO
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Vacacion] ADD  DEFAULT ('Registrada') FOR [Estado]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[BitacoraAuditoria]  WITH CHECK ADD  CONSTRAINT [FK_BitacoraAuditoria_Usuario] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[BitacoraAuditoria] CHECK CONSTRAINT [FK_BitacoraAuditoria_Usuario]
GO
ALTER TABLE [dbo].[Empleado]  WITH CHECK ADD  CONSTRAINT [FK_Empleado_Puesto] FOREIGN KEY([IdPuesto])
REFERENCES [dbo].[Puesto] ([IdPuesto])
GO
ALTER TABLE [dbo].[Empleado] CHECK CONSTRAINT [FK_Empleado_Puesto]
GO
ALTER TABLE [dbo].[EmpleadoDeduccion]  WITH CHECK ADD FOREIGN KEY([IdDeduccion])
REFERENCES [dbo].[DeduccionInterna] ([IdDeduccion])
GO
ALTER TABLE [dbo].[EmpleadoDeduccion]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Factura]  WITH CHECK ADD  CONSTRAINT [FK_Factura_Cliente] FOREIGN KEY([IdCliente])
REFERENCES [dbo].[Cliente] ([IdCliente])
GO
ALTER TABLE [dbo].[Factura] CHECK CONSTRAINT [FK_Factura_Cliente]
GO
ALTER TABLE [dbo].[Factura]  WITH CHECK ADD  CONSTRAINT [FK_Factura_Orden] FOREIGN KEY([IdOrdenCompra])
REFERENCES [dbo].[OrdenCompra] ([IdOrdenCompra])
GO
ALTER TABLE [dbo].[Factura] CHECK CONSTRAINT [FK_Factura_Orden]
GO
ALTER TABLE [dbo].[Factura]  WITH CHECK ADD  CONSTRAINT [FK_Factura_Usuario] FOREIGN KEY([IdUsuarioEmisor])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[Factura] CHECK CONSTRAINT [FK_Factura_Usuario]
GO
ALTER TABLE [dbo].[HistorialEstadoFactura]  WITH CHECK ADD  CONSTRAINT [FK_HistorialEstadoFactura_Factura] FOREIGN KEY([IdFactura])
REFERENCES [dbo].[Factura] ([IdFactura])
GO
ALTER TABLE [dbo].[HistorialEstadoFactura] CHECK CONSTRAINT [FK_HistorialEstadoFactura_Factura]
GO
ALTER TABLE [dbo].[HistorialEstadoFactura]  WITH CHECK ADD  CONSTRAINT [FK_HistorialEstadoFactura_Usuario] FOREIGN KEY([IdUsuarioCambio])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[HistorialEstadoFactura] CHECK CONSTRAINT [FK_HistorialEstadoFactura_Usuario]
GO
ALTER TABLE [dbo].[HistorialEstadoOrden]  WITH CHECK ADD  CONSTRAINT [FK_HistorialEstadoOrden_Orden] FOREIGN KEY([IdOrdenCompra])
REFERENCES [dbo].[OrdenCompra] ([IdOrdenCompra])
GO
ALTER TABLE [dbo].[HistorialEstadoOrden] CHECK CONSTRAINT [FK_HistorialEstadoOrden_Orden]
GO
ALTER TABLE [dbo].[HistorialEstadoOrden]  WITH CHECK ADD  CONSTRAINT [FK_HistorialEstadoOrden_Usuario] FOREIGN KEY([IdUsuarioCambio])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[HistorialEstadoOrden] CHECK CONSTRAINT [FK_HistorialEstadoOrden_Usuario]
GO
ALTER TABLE [dbo].[HistorialSalario]  WITH CHECK ADD  CONSTRAINT [FK_HistorialSalario_Empleado] FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[HistorialSalario] CHECK CONSTRAINT [FK_HistorialSalario_Empleado]
GO
ALTER TABLE [dbo].[HistorialSalario]  WITH CHECK ADD  CONSTRAINT [FK_HistorialSalario_Usuario] FOREIGN KEY([IdUsuarioRegistro])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[HistorialSalario] CHECK CONSTRAINT [FK_HistorialSalario_Usuario]
GO
ALTER TABLE [dbo].[Incapacidad]  WITH CHECK ADD  CONSTRAINT [FK_Incapacidad_Empleado] FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Incapacidad] CHECK CONSTRAINT [FK_Incapacidad_Empleado]
GO
ALTER TABLE [dbo].[InventarioMovimiento]  WITH CHECK ADD  CONSTRAINT [FK_InventarioMovimiento_OrdenCompra] FOREIGN KEY([IdOrdenCompra])
REFERENCES [dbo].[OrdenCompra] ([IdOrdenCompra])
GO
ALTER TABLE [dbo].[InventarioMovimiento] CHECK CONSTRAINT [FK_InventarioMovimiento_OrdenCompra]
GO
ALTER TABLE [dbo].[InventarioMovimiento]  WITH CHECK ADD  CONSTRAINT [FK_InventarioMovimiento_Produccion] FOREIGN KEY([IdProduccion])
REFERENCES [dbo].[Produccion] ([IdProduccion])
GO
ALTER TABLE [dbo].[InventarioMovimiento] CHECK CONSTRAINT [FK_InventarioMovimiento_Produccion]
GO
ALTER TABLE [dbo].[InventarioMovimiento]  WITH CHECK ADD  CONSTRAINT [FK_InventarioMovimiento_TipoTarima] FOREIGN KEY([IdTipoTarima])
REFERENCES [dbo].[TipoTarima] ([IdTipoTarima])
GO
ALTER TABLE [dbo].[InventarioMovimiento] CHECK CONSTRAINT [FK_InventarioMovimiento_TipoTarima]
GO
ALTER TABLE [dbo].[InventarioMovimiento]  WITH CHECK ADD  CONSTRAINT [FK_InventarioMovimiento_Usuario] FOREIGN KEY([IdUsuarioRegistro])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[InventarioMovimiento] CHECK CONSTRAINT [FK_InventarioMovimiento_Usuario]
GO
ALTER TABLE [dbo].[Licencia]  WITH CHECK ADD  CONSTRAINT [FK_Licencia_Empleado] FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Licencia] CHECK CONSTRAINT [FK_Licencia_Empleado]
GO
ALTER TABLE [dbo].[OrdenCompra]  WITH CHECK ADD  CONSTRAINT [FK_OrdenCompra_Cliente] FOREIGN KEY([IdCliente])
REFERENCES [dbo].[Cliente] ([IdCliente])
GO
ALTER TABLE [dbo].[OrdenCompra] CHECK CONSTRAINT [FK_OrdenCompra_Cliente]
GO
ALTER TABLE [dbo].[OrdenCompra]  WITH CHECK ADD  CONSTRAINT [FK_OrdenCompra_Vendedor] FOREIGN KEY([IdVendedor])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[OrdenCompra] CHECK CONSTRAINT [FK_OrdenCompra_Vendedor]
GO
ALTER TABLE [dbo].[OrdenCompraDetalle]  WITH CHECK ADD  CONSTRAINT [FK_OrdenCompraDetalle_Orden] FOREIGN KEY([IdOrdenCompra])
REFERENCES [dbo].[OrdenCompra] ([IdOrdenCompra])
GO
ALTER TABLE [dbo].[OrdenCompraDetalle] CHECK CONSTRAINT [FK_OrdenCompraDetalle_Orden]
GO
ALTER TABLE [dbo].[OrdenCompraDetalle]  WITH CHECK ADD  CONSTRAINT [FK_OrdenCompraDetalle_TipoTarima] FOREIGN KEY([IdTipoTarima])
REFERENCES [dbo].[TipoTarima] ([IdTipoTarima])
GO
ALTER TABLE [dbo].[OrdenCompraDetalle] CHECK CONSTRAINT [FK_OrdenCompraDetalle_TipoTarima]
GO
ALTER TABLE [dbo].[PagoFactura]  WITH CHECK ADD  CONSTRAINT [FK_PagoFactura_Factura] FOREIGN KEY([IdFactura])
REFERENCES [dbo].[Factura] ([IdFactura])
GO
ALTER TABLE [dbo].[PagoFactura] CHECK CONSTRAINT [FK_PagoFactura_Factura]
GO
ALTER TABLE [dbo].[PagoFactura]  WITH CHECK ADD  CONSTRAINT [FK_PagoFactura_Usuario] FOREIGN KEY([IdUsuarioRegistro])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[PagoFactura] CHECK CONSTRAINT [FK_PagoFactura_Usuario]
GO
ALTER TABLE [dbo].[PlanillaDeduccionDetalle]  WITH CHECK ADD  CONSTRAINT [FK_PlanillaDeduccionDetalle_Detalle] FOREIGN KEY([IdPlanillaDetalle])
REFERENCES [dbo].[PlanillaDetalle] ([IdPlanillaDetalle])
GO
ALTER TABLE [dbo].[PlanillaDeduccionDetalle] CHECK CONSTRAINT [FK_PlanillaDeduccionDetalle_Detalle]
GO
ALTER TABLE [dbo].[PlanillaDeduccionDetalle]  WITH CHECK ADD  CONSTRAINT [FK_PlanillaDeduccionDetalle_Tipo] FOREIGN KEY([IdTipoDeduccion])
REFERENCES [dbo].[TipoDeduccion] ([IdTipoDeduccion])
GO
ALTER TABLE [dbo].[PlanillaDeduccionDetalle] CHECK CONSTRAINT [FK_PlanillaDeduccionDetalle_Tipo]
GO
ALTER TABLE [dbo].[PlanillaDetalle]  WITH CHECK ADD  CONSTRAINT [FK_PlanillaDetalle_Empleado] FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[PlanillaDetalle] CHECK CONSTRAINT [FK_PlanillaDetalle_Empleado]
GO
ALTER TABLE [dbo].[PlanillaDetalle]  WITH CHECK ADD  CONSTRAINT [FK_PlanillaDetalle_Periodo] FOREIGN KEY([IdPlanillaPeriodo])
REFERENCES [dbo].[PlanillaPeriodo] ([IdPlanillaPeriodo])
GO
ALTER TABLE [dbo].[PlanillaDetalle] CHECK CONSTRAINT [FK_PlanillaDetalle_Periodo]
GO
ALTER TABLE [dbo].[PlanillaPeriodo]  WITH CHECK ADD  CONSTRAINT [FK_PlanillaPeriodo_Usuario] FOREIGN KEY([IdUsuarioCreacion])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[PlanillaPeriodo] CHECK CONSTRAINT [FK_PlanillaPeriodo_Usuario]
GO
ALTER TABLE [dbo].[Produccion]  WITH CHECK ADD  CONSTRAINT [FK_Produccion_TipoTarima] FOREIGN KEY([IdTipoTarima])
REFERENCES [dbo].[TipoTarima] ([IdTipoTarima])
GO
ALTER TABLE [dbo].[Produccion] CHECK CONSTRAINT [FK_Produccion_TipoTarima]
GO
ALTER TABLE [dbo].[Produccion]  WITH CHECK ADD  CONSTRAINT [FK_Produccion_Usuario] FOREIGN KEY([IdUsuarioRegistro])
REFERENCES [dbo].[Usuario] ([IdUsuario])
GO
ALTER TABLE [dbo].[Produccion] CHECK CONSTRAINT [FK_Produccion_Usuario]
GO
ALTER TABLE [dbo].[Puesto]  WITH CHECK ADD  CONSTRAINT [FK_Puesto_Departamento] FOREIGN KEY([IdDepartamento])
REFERENCES [dbo].[Departamento] ([IdDepartamento])
GO
ALTER TABLE [dbo].[Puesto] CHECK CONSTRAINT [FK_Puesto_Departamento]
GO
ALTER TABLE [dbo].[SalarioHistorial]  WITH CHECK ADD FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Empleado] FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_Empleado]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Rol] FOREIGN KEY([IdRol])
REFERENCES [dbo].[Rol] ([IdRol])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_Rol]
GO
ALTER TABLE [dbo].[Vacacion]  WITH CHECK ADD  CONSTRAINT [FK_Vacacion_Empleado] FOREIGN KEY([IdEmpleado])
REFERENCES [dbo].[Empleado] ([IdEmpleado])
GO
ALTER TABLE [dbo].[Vacacion] CHECK CONSTRAINT [FK_Vacacion_Empleado]
GO
ALTER TABLE [dbo].[Factura]  WITH CHECK ADD  CONSTRAINT [CK_Factura_Estado] CHECK  (([Estado]='Anulada' OR [Estado]='Pagada' OR [Estado]='Pendiente de Pago' OR [Estado]='Emitida'))
GO
ALTER TABLE [dbo].[Factura] CHECK CONSTRAINT [CK_Factura_Estado]
GO
ALTER TABLE [dbo].[InventarioMovimiento]  WITH CHECK ADD CHECK  (([Cantidad]>(0)))
GO
ALTER TABLE [dbo].[InventarioMovimiento]  WITH CHECK ADD  CONSTRAINT [CK_InventarioMovimiento_Tipo] CHECK  (([TipoMovimiento]='AjusteSalida' OR [TipoMovimiento]='AjusteEntrada' OR [TipoMovimiento]='Salida' OR [TipoMovimiento]='Entrada'))
GO
ALTER TABLE [dbo].[InventarioMovimiento] CHECK CONSTRAINT [CK_InventarioMovimiento_Tipo]
GO
ALTER TABLE [dbo].[OrdenCompra]  WITH CHECK ADD  CONSTRAINT [CK_OrdenCompra_Estado] CHECK  (([Estado]='Cancelada' OR [Estado]='Entregada' OR [Estado]='Lista para Entrega' OR [Estado]='En Produccion' OR [Estado]='Pendiente'))
GO
ALTER TABLE [dbo].[OrdenCompra] CHECK CONSTRAINT [CK_OrdenCompra_Estado]
GO
ALTER TABLE [dbo].[OrdenCompraDetalle]  WITH CHECK ADD CHECK  (([Cantidad]>(0)))
GO
ALTER TABLE [dbo].[PagoFactura]  WITH CHECK ADD CHECK  (([Monto]>(0)))
GO
ALTER TABLE [dbo].[PlanillaPeriodo]  WITH CHECK ADD  CONSTRAINT [CK_PlanillaPeriodo_Estado] CHECK  (([Estado]='Pagada' OR [Estado]='Aprobada' OR [Estado]='Revisada' OR [Estado]='Borrador'))
GO
ALTER TABLE [dbo].[PlanillaPeriodo] CHECK CONSTRAINT [CK_PlanillaPeriodo_Estado]
GO
ALTER TABLE [dbo].[Produccion]  WITH CHECK ADD CHECK  (([Cantidad]>(0)))
GO
USE [master]
GO
ALTER DATABASE [PROmaderasDB_NEW] SET  READ_WRITE 
GO

USE [PROmaderasDB_NEW]
GO

-- === AJUSTE: registrar migraciones EF Identity como YA aplicadas ===
-- Evita que Program.cs -> identityContext.Database.Migrate() intente
-- recrear AspNetRoles/AspNetUsers (ya creadas por este script) y crashee.
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES
  ('20260324054853_IdentityInit','8.0.11'),
  ('20260411151437_IdentityUsuariosExtendido','8.0.11');
GO
