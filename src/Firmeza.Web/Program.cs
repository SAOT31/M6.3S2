using Firmeza.Core.Enums;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// WebApplication.CreateBuilder arma el contenedor de servicios (inyección de dependencias)
// y la configuración del servidor web
var builder = WebApplication.CreateBuilder(args);

// ── BASE DE DATOS ──────────────────────────────────────────────────────────────
// Le decimos a EF Core que use PostgreSQL como motor de BD
// La cadena de conexión viene del appsettings.json ("DefaultConnection")
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── IDENTITY (autenticación + roles) ──────────────────────────────────────────
// AddIdentity registra todos los servicios de autenticación:
// UserManager, SignInManager, RoleManager, etc.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Requisitos mínimos de contraseña — ajustados para el taller
    options.Password.RequireDigit           = true;   // debe tener al menos un número
    options.Password.RequiredLength         = 6;      // mínimo 6 caracteres
    options.Password.RequireNonAlphanumeric = false;  // no exige símbolos especiales
    options.Password.RequireUppercase       = false;  // no exige mayúsculas

    // No pedimos confirmar el email para simplificar el flujo del taller
    options.SignIn.RequireConfirmedAccount = false;
})
// Le decimos que guarde los usuarios y roles en nuestra BD (ApplicationDbContext)
.AddEntityFrameworkStores<ApplicationDbContext>()
// Agrega los tokens para reset de contraseña, confirmación de email, etc.
.AddDefaultTokenProviders();

// ── CONFIGURACIÓN DE COOKIES ───────────────────────────────────────────────────
// Cuando alguien intente acceder a una página protegida sin estar logueado,
// el framework lo redirige automáticamente a estas rutas
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Auth/Login";        // redirige aquí si no está logueado
    options.AccessDeniedPath = "/Auth/AccessDenied"; // redirige aquí si no tiene el rol correcto
    options.ExpireTimeSpan   = TimeSpan.FromHours(8); // la sesión dura 8 horas
});

// ── RAZOR PAGES ───────────────────────────────────────────────────────────────
// Registra el servicio de Razor Pages — sin esto el framework no procesa los .cshtml
builder.Services.AddRazorPages();

// Construimos la app con todos los servicios registrados arriba
var app = builder.Build();

// ── SEED DE DATOS INICIALES ────────────────────────────────────────────────────
// Antes de atender peticiones, creamos los roles y el admin por defecto si no existen
await SeedDatabase(app);

// ── MIDDLEWARES (tuberías de procesamiento de peticiones) ─────────────────────
// Solo en producción: manejo de errores y HTTPS estricto
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // fuerza HTTPS en el navegador
}

app.UseHttpsRedirection(); // redirige HTTP → HTTPS
app.UseStaticFiles();      // sirve archivos de wwwroot (CSS, JS, imágenes)
app.UseRouting();          // habilita el sistema de rutas
app.UseAuthentication();   // lee la cookie de sesión y autentica al usuario
app.UseAuthorization();    // verifica si el usuario tiene permiso para la ruta

// Conecta las rutas a las páginas Razor
app.MapRazorPages();

// La ruta raíz "/" redirige directo al dashboard
app.MapGet("/", () => Results.Redirect("/Dashboard"));

app.Run();

// ── MÉTODO DE SEED ─────────────────────────────────────────────────────────────
// Se ejecuta una sola vez al arrancar; crea roles y admin si no existen
static async Task SeedDatabase(WebApplication app)
{
    // CreateScope abre un "alcance" temporal para resolver servicios con ciclo Scoped
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context     = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Aplica cualquier migración pendiente — crea las tablas si no existen
        await context.Database.MigrateAsync();

        // Crea los roles si todavía no están en la tabla AspNetRoles
        string[] roles = [AppRoles.Admin, AppRoles.Customer];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Crea el usuario admin por defecto si no existe
        const string adminEmail = "admin@firmeza.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName       = adminEmail,
                Email          = adminEmail,
                DisplayName    = "Administrador",
                EmailConfirmed = true // marcamos como confirmado para no pedir verificación
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }
    catch (Exception ex)
    {
        // Si la BD no está lista (contenedor aún iniciando), solo logueamos y seguimos
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error durante el seed de la base de datos");
    }
}
