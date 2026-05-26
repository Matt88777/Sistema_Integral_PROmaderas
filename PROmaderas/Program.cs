using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;                        // <-- AGREGA ESTO
using PROmaderas.AccesoADatos;                              // <-- YA ESTÁ
using PROmaderas.AccesoADatos.Seguridad;
using PROmaderas.AccesoADatos.Categorias;
using PROmaderas.AccesoADatos.Productos;
using PROmaderas.AccesoADatos.Clientes;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.LogicaDeNegocio.Categorias;
using PROmaderas.LogicaDeNegocio.Productos;
using PROmaderas.LogicaDeNegocio.Clientes;
using QuestPDF.Infrastructure;
using PROmaderas.UI.Services;
using PROmaderas.AccesoADatos.Empleados;



{

}


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

//

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

builder.Services.AddScoped<IClienteRepositorio, PROmaderas.AccesoADatos.Clientes.EmpleadoRepositorio>();

builder.Services.AddScoped<IClienteLogica, EmpleadosLogica>();

builder.Services.AddScoped<PROmaderas.AccesoADatos.Empleados.EmpleadoRepositorio>();


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

