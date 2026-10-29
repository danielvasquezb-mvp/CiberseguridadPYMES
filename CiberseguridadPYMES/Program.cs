using CiberseguridadPYMES.Data;
using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar la conexión con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar autenticación y usuarios
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Registrar los controladores y las vistas MVC
builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // La cookie no se renueva automáticamente.
    options.SlidingExpiration = false;

    // Duración máxima de la autenticación.
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
});

var app = builder.Build();

// Configurar el pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Habilitar las páginas de ASP.NET Core Identity
app.MapRazorPages();

// Crear roles y asignar el administrador inicial
using (var scope = app.Services.CreateScope())
{
    var servicios = scope.ServiceProvider;

    var roleManager =
        servicios.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        servicios.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles =
    {
        "Administrador",
        "Responsable del sistema"
    };

    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            var resultadoRol =
                await roleManager.CreateAsync(new IdentityRole(rol));

            if (!resultadoRol.Succeeded)
            {
                throw new Exception(
                    $"No se pudo crear el rol {rol}: " +
                    string.Join(", ",
                        resultadoRol.Errors.Select(e => e.Description)));
            }
        }
    }

    var correoAdmin =
        app.Configuration["AdministradorInicial:Correo"];

    if (!string.IsNullOrWhiteSpace(correoAdmin))
    {
        var administrador =
            await userManager.FindByEmailAsync(correoAdmin.Trim());

        if (administrador != null &&
            !await userManager.IsInRoleAsync(
                administrador, "Administrador"))
        {
            var resultadoAdmin =
                await userManager.AddToRoleAsync(
                    administrador, "Administrador");

            if (!resultadoAdmin.Succeeded)
            {
                throw new Exception(
                    "No se pudo asignar el rol Administrador: " +
                    string.Join(", ",
                        resultadoAdmin.Errors.Select(e => e.Description)));
            }
        }
    }
}

app.Run();
