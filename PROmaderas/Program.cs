using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PROmaderas.AccesoADatos;
using PROmaderas.AccesoADatos.Seguridad;
using PROmaderas.AccesoADatos.Categorias;
using PROmaderas.AccesoADatos.Productos;
using PROmaderas.AccesoADatos.Clientes;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.LogicaDeNegocio.Categorias;
using PROmaderas.LogicaDeNegocio.Productos;
using PROmaderas.LogicaDeNegocio.Clientes;
using PROmaderas.LogicaDeNegocio.Empleados;
using PROmaderas.LogicaDeNegocio.Puestos;
using PROmaderas.AccesoADatos.Puestos;
using PROmaderas.AccesoADatos.Facturacion;
using PROmaderas.LogicaDeNegocio.Facturacion;
using QuestPDF.Infrastructure;
using PROmaderas.UI.Services;
using PROmaderas.AccesoADatos.Dashboard;
using PROmaderas.LogicaDeNegocio.Dashboard;
using PROmaderas.AccesoADatos.Produccion;
using PROmaderas.LogicaDeNegocio.Produccion;
using PROmaderas.AccesoADatos.Empleados;
using PROmaderas.AccesoADatos.Licencias;
using PROmaderas.LogicaDeNegocio.Licencias;
using PROmaderas.AccesoADatos.Aguinaldo;
using PROmaderas.LogicaDeNegocio.Aguinaldo;
using PROmaderas.AccesoADatos.HistorialPagos;
using PROmaderas.LogicaDeNegocio.HistorialPagos;
using PROmaderas.AccesoADatos.Reportes;
using PROmaderas.LogicaDeNegocio.Reportes;
using PROmaderas.AccesoADatos.Planilla;
using PROmaderas.LogicaDeNegocio.Planilla;
using PROmaderas.AccesoADatos.Deducciones;
using PROmaderas.LogicaDeNegocio.Deducciones;
using PROmaderas.AccesoADatos.Parametros;
using PROmaderas.LogicaDeNegocio.Parametros;
using PROmaderas.AccesoADatos.Vacaciones;
using PROmaderas.LogicaDeNegocio.Vacaciones;
using PROmaderas.AccesoADatos.Liquidaciones;
using PROmaderas.LogicaDeNegocio.Liquidaciones;
using PROmaderas.AccesoADatos.PolizaINS;
using PROmaderas.LogicaDeNegocio.PolizaINS;
using PROmaderas.AccesoADatos.Incapacidad;
using PROmaderas.LogicaDeNegocio.Incapacidad;







var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<PROmaderas.UI.Filters.GlobalExceptionFilter>();
});

// === CONFIGURA EL CONTEXTO DE IDENTITY ===
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// === CONFIGURA IDENTITY (LOGIN Y ROLES EN LA BD) ===
builder.Services.AddIdentity<UsuarioIdentity, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization();

// Tu contexto original para tus repositorios personalizados (lo puedes dejar)
builder.Services.AddDbContext<Contexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
builder.Services.AddScoped<ICategoriaLogica, CategoriaLogica>();
builder.Services.AddScoped<IProductoLogica, ProductoLogica>();
builder.Services.AddScoped<PROmaderas.AccesoADatos.Empleados.EmpleadoRepositorio>();
builder.Services.AddSingleton<IMailStore, InMemoryMailStore>();
builder.Services.AddScoped<IReportesLogica, ReportesLogica>();
builder.Services.AddScoped<IReportesRepositorio, ReportesRepositorio>();
builder.Services.AddScoped<IReportesExportLogica, ReportesExportLogica>();


builder.Services.AddScoped<IClienteRepositorio, PROmaderas.AccesoADatos.Clientes.EmpleadoRepositorio>();

builder.Services.AddScoped<IClienteLogica, EmpleadosLogica>();

builder.Services.AddScoped<PROmaderas.AccesoADatos.Empleados.EmpleadoRepositorio>();

builder.Services.AddScoped<IEmpleadoRepositorio, PROmaderas.AccesoADatos.Empleados.EmpleadoRepositorio>();
builder.Services.AddScoped<IEmpleadoLogica, EmpleadoLogica>();
builder.Services.AddScoped<IPuestoRepositorio, PROmaderas.AccesoADatos.Puestos.PuestoRepositorio>();
builder.Services.AddScoped<IPuestoLogica, PuestoLogica>();


builder.Services.AddScoped<PROmaderas.Abstracciones.AccesoADatos.IPlanillaRepositorio,
    PROmaderas.AccesoADatos.Planilla.PlanillaRepositorio>();
builder.Services.AddScoped<PROmaderas.Abstracciones.LogicaDeNegocio.IPlanillaLogica,
    PROmaderas.LogicaDeNegocio.Planilla.PlanillaLogica>();

builder.Services.AddScoped<IAguinaldoRepositorio, AguinaldoRepositorio>();
builder.Services.AddScoped<IAguinaldoLogica, AguinaldoLogica>();

builder.Services.AddScoped<ILicenciaRepositorio, PROmaderas.AccesoADatos.Licencias.LicenciaRepositorio>();

builder.Services.AddScoped<IFacturacionRepositorio, 
    PROmaderas.AccesoADatos.Facturacion.FacturacionRepositorio>();
builder.Services.AddScoped<IFacturacionLogica, FacturacionLogica>();
builder.Services.AddScoped<IDashboardRepositorio, DashboardRepositorio>();
builder.Services.AddScoped<IDashboardLogica, DashboardLogica>();

builder.Services.AddScoped<IHistorialPagosRepositorio, HistorialPagosRepositorio>();
builder.Services.AddScoped<IHistorialPagosLogica, HistorialPagosLogica>();

builder.Services.AddScoped<IReportesRepositorio, ReportesRepositorio>();
builder.Services.AddScoped<IReportesExportLogica, ReportesExportLogica>();


builder.Services.AddScoped<IProduccionRepositorio, ProduccionRepositorio>();
builder.Services.AddScoped<IProduccionLogica, ProduccionLogica>();


builder.Services.AddScoped<IDeduccionInternaRepositorio, DeduccionInternaRepositorio>();
builder.Services.AddScoped<IDeduccionInternaLogica, DeduccionInternaLogica>();

// PLA-HU-019: parámetros de planilla versionados por vigencia.
builder.Services.AddScoped<IParametroPlanillaRepositorio, ParametroPlanillaRepositorio>();
builder.Services.AddScoped<IParametroPlanillaLogica, ParametroPlanillaLogica>();

builder.Services.AddScoped<IPlanillaRepositorio, PlanillaRepositorio>();
builder.Services.AddScoped<IPlanillaLogica, PlanillaLogica>();
builder.Services.AddScoped<ILicenciaRepositorio, LicenciaRepositorio>();
builder.Services.AddScoped<ILicenciaLogica, LicenciaLogica>();

// PLA-HU-012: vacaciones.
builder.Services.AddScoped<IVacacionRepositorio, VacacionRepositorio>();
builder.Services.AddScoped<IVacacionLogica, VacacionLogica>();

// PLA-HU-018: pólizas del INS.
builder.Services.AddScoped<IPolizaINSRepositorio, PolizaINSRepositorio>();
builder.Services.AddScoped<IPolizaINSLogica, PolizaINSLogica>();

// PLA-HU-014: registro y cálculo de incapacidades.
builder.Services.AddScoped<IIncapacidadRepositorio, IncapacidadRepositorio>();
builder.Services.AddScoped<IIncapacidadLogica, IncapacidadLogica>();

// PLA-HU-017: liquidacion.
builder.Services.AddScoped<ILiquidacionRepositorio, LiquidacionRepositorio>();
builder.Services.AddScoped<ILiquidacionLogica, LiquidacionLogica>();





QuestPDF.Settings.License = LicenseType.Community;
var app = builder.Build();

// === ASEGURAR QUE LA BASE DE DATOS DE IDENTITY ESTÉ CREADA ===
using (var scope = app.Services.CreateScope())
{
    var identityContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    identityContext.Database.Migrate();
}

// Sprint 0 PROMADERAS: el seed antiguo (Pedidos360) creaba/poblaba tablas
// Categoria, Producto, Cliente y la columna Cliente.UsuarioIdentityId. La BD
// nueva PROmaderasDB_NEW ya viene poblada con seed propio (roles,
// departamentos, puestos, empleado admin, tipos de tarima base) desde el
// script SQL, así que estas llamadas quedan deshabilitadas:
//
// DbSeeder.EnsureClienteIdentitySchema(contexto);
// DbSeeder.EnsureCategoriaSchema(contexto);
// DbSeeder.EnsureProductoSchema(contexto);
// DbSeeder.SeedCategorias(contexto);
// DbSeeder.SeedClientes(contexto);
// DbSeeder.SeedProductos(contexto);

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UsuarioIdentity>>();

    await IdentitySeeder.SeedRolesAsync(roleManager);
    await IdentitySeeder.SeedUsuarioAdministradorAsync(userManager, builder.Configuration);
    await IdentitySeeder.SeedUsuarioVendedorAsync(userManager, builder.Configuration);
    await IdentitySeeder.SeedUsuarioGerenteAsync(userManager);
    await IdentitySeeder.SeedUsuarioContadorAsync(userManager);
    await IdentitySeeder.SeedUsuarioOperadorAsync(userManager);
    await IdentitySeeder.SeedUsuarioGenericoAsync(userManager);
    await IdentitySeeder.SeedUsuarioAborbonAsync(userManager);
    await IdentitySeeder.SeedUsuarioDanielaAsync(userManager);
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "login",
    pattern: "",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.Run();