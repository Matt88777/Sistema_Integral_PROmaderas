/*
 * Pedidos360 — Herramienta CLI para crear usuarios administrador
 *
 * Genera un PasswordHash 100% compatible con ASP.NET Core Identity v3
 * (PBKDF2-HMAC-SHA256, 350.000 iteraciones) y lo inserta en la BD
 * ejecutando el stored procedure dbo.SP_CrearUsuarioAdminIdentity.
 *
 * Uso:
 *   dotnet run
 *   dotnet run -- --correo admin2@empresa.com --nombre "Ana Gomez" --telefono 88880000
 *   dotnet run -- --server 192.168.1.10 --db Pedidos360DB_Prod
 *
 * Opciones:
 *   --correo    Correo del admin          (default: admin@pedidos360.local)
 *   --nombre    Nombre completo           (default: Administrador Principal)
 *   --telefono  Telefono (opcional)
 *   --server    SQL Server host/IP        (default: localhost)
 *   --db        Nombre de la base de datos (default: Pedidos360DB)
 */

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;

// ─── Parsear argumentos ────────────────────────────────────────────────────
string correo         = GetArg(args, "--correo",   "admin@pedidos360.local");
string nombreCompleto = GetArg(args, "--nombre",   "Administrador Principal");
string? telefono      = GetArgOrNull(args, "--telefono");
string sqlServer      = GetArg(args, "--server",   "localhost");
string baseDeDatos    = GetArg(args, "--db",       "Pedidos360DB");

// ─── Consola ───────────────────────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("  Pedidos360 — Creacion de Usuario Administrador");
Console.WriteLine("═══════════════════════════════════════════════════════");
Console.ResetColor();
Console.WriteLine();
Console.WriteLine($"  Correo       : {correo}");
Console.WriteLine($"  Nombre       : {nombreCompleto}");
Console.WriteLine($"  Servidor SQL : {sqlServer}");
Console.WriteLine($"  Base de datos: {baseDeDatos}");
Console.WriteLine();

// ─── Leer contraseña ───────────────────────────────────────────────────────
string password  = LeerContrasena("  Ingrese la contrasena   : ");
string confirmacion = LeerContrasena("  Confirme la contrasena  : ");
Console.WriteLine();

if (password != confirmacion)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("  [ERROR] Las contrasenas no coinciden.");
    Console.ResetColor();
    return 1;
}

if (password.Length < 8)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("  [ERROR] La contrasena debe tener al menos 8 caracteres.");
    Console.ResetColor();
    return 1;
}

// ─── Generar hash ──────────────────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Yellow;
Console.Write("  Generando PasswordHash Identity v3... ");
Console.ResetColor();

string passwordHash = GenerarIdentityPasswordHash(password);
password     = string.Empty;  // limpiar de memoria
confirmacion = string.Empty;

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("OK");
Console.ResetColor();

// ─── Ejecutar SP ───────────────────────────────────────────────────────────
string connStr = $"Server={sqlServer};Database={baseDeDatos};Integrated Security=True;TrustServerCertificate=True;";

Console.ForegroundColor = ConsoleColor.Yellow;
Console.Write($"  Conectando a {sqlServer}\\{baseDeDatos}... ");
Console.ResetColor();

try
{
    await using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("OK");
    Console.ResetColor();

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("  Ejecutando SP_CrearUsuarioAdminIdentity... ");
    Console.ResetColor();

    await using var cmd = conn.CreateCommand();
    cmd.CommandType = System.Data.CommandType.StoredProcedure;
    cmd.CommandText = "dbo.SP_CrearUsuarioAdminIdentity";

    cmd.Parameters.AddWithValue("@Correo",         correo);
    cmd.Parameters.AddWithValue("@NombreCompleto", nombreCompleto);
    cmd.Parameters.AddWithValue("@PasswordHash",   passwordHash);

    if (telefono is not null)
        cmd.Parameters.AddWithValue("@Telefono", telefono);

    await cmd.ExecuteNonQueryAsync();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("OK");
    Console.WriteLine();
    Console.WriteLine($"  ✔ Administrador '{correo}' creado/actualizado correctamente.");
    Console.ResetColor();
    Console.WriteLine();
    return 0;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine();
    Console.WriteLine($"  [ERROR] {ex.Message}");
    Console.ResetColor();
    return 1;
}

// ─── Helpers ───────────────────────────────────────────────────────────────

/// <summary>
/// Genera un PasswordHash identico al que produce ASP.NET Core Identity
/// PasswordHasher&lt;TUser&gt; version 3:
///   - Algoritmo: PBKDF2-HMAC-SHA256
///   - Iteraciones: 350.000
///   - Sal: 16 bytes aleatorios seguros
///   - Longitud derivada: 32 bytes
/// El resultado en Base64 puede guardarse directamente en AspNetUsers.PasswordHash.
/// </summary>
static string GenerarIdentityPasswordHash(string plainPassword)
{
    const int iterCount  = 350_000;
    const int saltLength = 16;
    const int hashLength = 32;
    const KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA256;

    byte[] salt = RandomNumberGenerator.GetBytes(saltLength);

    byte[] subkey = KeyDerivation.Pbkdf2(
        password:   plainPassword,
        salt:       salt,
        prf:        prf,
        iterationCount: iterCount,
        numBytesRequested: hashLength
    );

    // Formato Identity v3:
    // [0]     : 0x01  (version marker)
    // [1-4]   : PRF como uint32 big-endian  (1 = HMACSHA256)
    // [5-8]   : iteraciones big-endian
    // [9-12]  : longitud del salt big-endian
    // [13...] : salt | subkey
    byte[] output = new byte[1 + sizeof(uint) * 3 + saltLength + hashLength];
    output[0] = 0x01;
    WriteBigEndian(output, 1, (uint)prf);
    WriteBigEndian(output, 5, (uint)iterCount);
    WriteBigEndian(output, 9, (uint)saltLength);
    Buffer.BlockCopy(salt,   0, output, 13,               saltLength);
    Buffer.BlockCopy(subkey, 0, output, 13 + saltLength,  hashLength);

    return Convert.ToBase64String(output);
}

static void WriteBigEndian(byte[] buffer, int offset, uint value)
{
    buffer[offset]     = (byte)(value >> 24);
    buffer[offset + 1] = (byte)(value >> 16);
    buffer[offset + 2] = (byte)(value >> 8);
    buffer[offset + 3] = (byte)(value);
}

static string LeerContrasena(string prompt)
{
    Console.Write(prompt);
    var sb = new StringBuilder();
    while (true)
    {
        ConsoleKeyInfo key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) break;
        if (key.Key == ConsoleKey.Backspace)
        {
            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
        }
        else if (key.KeyChar != '\0')
        {
            sb.Append(key.KeyChar);
        }
    }
    Console.WriteLine();
    return sb.ToString();
}

static string GetArg(string[] args, string flag, string defaultValue)
{
    int idx = Array.IndexOf(args, flag);
    return (idx >= 0 && idx + 1 < args.Length) ? args[idx + 1] : defaultValue;
}

static string? GetArgOrNull(string[] args, string flag)
{
    int idx = Array.IndexOf(args, flag);
    return (idx >= 0 && idx + 1 < args.Length) ? args[idx + 1] : null;
}
