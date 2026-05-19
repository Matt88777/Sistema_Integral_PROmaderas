<#
.SYNOPSIS
    Crea un usuario administrador en la base de datos de Pedidos360 usando
    hashing compatible con ASP.NET Core Identity (PBKDF2-HMAC-SHA256).

.DESCRIPTION
    Este script:
      1. Solicita (o recibe como parametro) la contrasena en texto plano.
      2. Genera el PasswordHash usando el mismo algoritmo que usa ASP.NET Identity.
      3. Conecta a SQL Server y ejecuta el SP dbo.SP_CrearUsuarioAdminIdentity.

    IMPORTANTE: El PasswordHash que genera este script es 100% compatible con
    ASP.NET Core Identity PasswordHasher v3 (PBKDF2-HMAC-SHA256, 350000 iteraciones).
    La aplicacion podra autenticar al usuario sin ningun problema.

.PARAMETER Correo
    Correo electronico del administrador. Default: admin@pedidos360.local

.PARAMETER NombreCompleto
    Nombre completo del administrador. Default: Administrador Principal

.PARAMETER Telefono
    Telefono opcional.

.PARAMETER SqlServer
    Nombre o IP del servidor SQL Server. Default: localhost

.PARAMETER BaseDeDatos
    Nombre de la base de datos. Default: Pedidos360DB

.EXAMPLE
    .\Crear-AdminIdentity.ps1

.EXAMPLE
    .\Crear-AdminIdentity.ps1 -Correo "nuevo.admin@miempresa.com" -NombreCompleto "Carlos Mora" -Telefono "88991234"

.EXAMPLE
    .\Crear-AdminIdentity.ps1 -SqlServer "192.168.1.10" -BaseDeDatos "Pedidos360DB_Produccion"
#>
param(
    [string] $Correo        = "admin@pedidos360.local",
    [string] $NombreCompleto = "Administrador Principal",
    [string] $Telefono       = $null,
    [string] $SqlServer      = "localhost",
    [string] $BaseDeDatos    = "Pedidos360DB"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# -----------------------------------------------------------------------
# Funcion: Genera PasswordHash Identity v3 (compatible con ASP.NET Core 3+)
#
# Formato del hash (Base64 del byte array):
#   [0]     = 0x01  (version marker = v3)
#   [1..4]  = PRF   = 1 (HMACSHA256) como int32 big-endian
#   [5..8]  = iteraciones como int32 big-endian
#   [9..12] = longitud del salt como int32 big-endian (16 bytes = 0x10)
#   [13..28]= salt (16 bytes aleatorios)
#   [29..]  = hash PBKDF2 (32 bytes)
# -----------------------------------------------------------------------
function New-IdentityPasswordHash {
    param([string] $PlainPassword)

    # Parametros identicos a PasswordHasherV3 de ASP.NET Core Identity
    [int] $iterCount    = 350000
    [int] $saltSize     = 16   # bytes
    [int] $numBytesHash = 32   # bytes (SHA256 output)
    [int] $prf          = 1    # KeyDerivationPrf.HMACSHA256

    # Generar salt criptograficamente seguro (compatible con Windows PowerShell 5.1)
    $salt = [byte[]]::new($saltSize)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($salt)
    $rng.Dispose()

    # Derivar clave con PBKDF2-HMACSHA256
    $kdf = [System.Security.Cryptography.Rfc2898DeriveBytes]::new(
        $PlainPassword,
        $salt,
        $iterCount,
        [System.Security.Cryptography.HashAlgorithmName]::SHA256
    )
    $derivedKey = $kdf.GetBytes($numBytesHash)
    $kdf.Dispose()

    # Construir el buffer de salida
    # Byte 0: version = 0x01
    # Bytes 1-4:  PRF (big-endian int32)
    # Bytes 5-8:  iteraciones (big-endian int32)
    # Bytes 9-12: longitud del salt (big-endian int32)
    # Bytes 13..13+saltSize-1: salt
    # Bytes siguientes: hash derivado
    $output = [System.IO.MemoryStream]::new()
    $writer = [System.IO.BinaryWriter]::new($output)

    $writer.Write([byte]0x01)  # version marker

    # big-endian int32 helper
    function Write-BigEndianInt32 {
        param($w, [int]$value)
        $b = [System.BitConverter]::GetBytes($value)
        if ([System.BitConverter]::IsLittleEndian) { [System.Array]::Reverse($b) }
        $w.Write($b)
    }

    Write-BigEndianInt32 $writer $prf
    Write-BigEndianInt32 $writer $iterCount
    Write-BigEndianInt32 $writer $saltSize

    $writer.Write($salt)
    $writer.Write($derivedKey)

    $writer.Flush()
    $hashBytes = $output.ToArray()

    return [System.Convert]::ToBase64String($hashBytes)
}

# -----------------------------------------------------------------------
# Solicitar contrasena de forma segura
# -----------------------------------------------------------------------
Write-Host ""
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host "  Pedidos360 - Creacion de Usuario Administrador" -ForegroundColor Cyan
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Correo       : $Correo" -ForegroundColor White
Write-Host "  Nombre       : $NombreCompleto" -ForegroundColor White
Write-Host "  Servidor SQL : $SqlServer" -ForegroundColor White
Write-Host "  Base de datos: $BaseDeDatos" -ForegroundColor White
Write-Host ""

$securePass = Read-Host "Ingrese la contrasena para el administrador" -AsSecureString
$confirm    = Read-Host "Confirme la contrasena                      " -AsSecureString

# Comparar contrasenas
$p1 = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePass))
$p2 = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($confirm))

if ($p1 -ne $p2) {
    Write-Host ""
    Write-Host "  [ERROR] Las contrasenas no coinciden." -ForegroundColor Red
    exit 1
}

if ($p1.Length -lt 8) {
    Write-Host ""
    Write-Host "  [ERROR] La contrasena debe tener al menos 8 caracteres." -ForegroundColor Red
    exit 1
}

# -----------------------------------------------------------------------
# Generar hash compatible con ASP.NET Identity
# -----------------------------------------------------------------------
Write-Host ""
Write-Host "  Generando PasswordHash Identity..." -ForegroundColor Yellow

$passwordHash = New-IdentityPasswordHash -PlainPassword $p1

# Limpiar variables sensibles de memoria
$p1 = $null; $p2 = $null
[System.GC]::Collect()

Write-Host "  Hash generado correctamente." -ForegroundColor Green

# -----------------------------------------------------------------------
# Ejecutar SP en SQL Server
# -----------------------------------------------------------------------
Write-Host ""
Write-Host "  Conectando a SQL Server ($SqlServer \ $BaseDeDatos)..." -ForegroundColor Yellow

$conn = [System.Data.SqlClient.SqlConnection]::new(
    "Server=$SqlServer;Database=$BaseDeDatos;Integrated Security=True;TrustServerCertificate=True;"
)

try {
    $conn.Open()
    Write-Host "  Conexion establecida." -ForegroundColor Green

    # Evita errores con indices filtrados/computed columns al ejecutar DML en Identity
    $sessionCmd = $conn.CreateCommand()
    $sessionCmd.CommandType = [System.Data.CommandType]::Text
    $sessionCmd.CommandText = "SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET CONCAT_NULL_YIELDS_NULL ON; SET ARITHABORT ON; SET NUMERIC_ROUNDABORT OFF;"
    $sessionCmd.ExecuteNonQuery() | Out-Null

    $cmd = $conn.CreateCommand()
    $cmd.CommandType = [System.Data.CommandType]::StoredProcedure
    $cmd.CommandText = "dbo.SP_CrearUsuarioAdminIdentity"

    [void]$cmd.Parameters.AddWithValue("@Correo",         $Correo)
    [void]$cmd.Parameters.AddWithValue("@NombreCompleto", $NombreCompleto)
    [void]$cmd.Parameters.AddWithValue("@PasswordHash",   $passwordHash)

    if ($Telefono) {
        [void]$cmd.Parameters.AddWithValue("@Telefono", $Telefono)
    }

    Write-Host "  Ejecutando SP_CrearUsuarioAdminIdentity..." -ForegroundColor Yellow
    $cmd.ExecuteNonQuery() | Out-Null

    Write-Host ""
    Write-Host "  [OK] Administrador creado/actualizado exitosamente." -ForegroundColor Green
    Write-Host "  Correo: $Correo" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "  [ERROR] $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    $passwordHash = $null
    $conn.Dispose()
}
