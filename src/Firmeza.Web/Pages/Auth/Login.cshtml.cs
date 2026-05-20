using Firmeza.Core.Enums;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.Pages.Auth;

public class LoginModel : PageModel
{
    // SignInManager maneja el inicio de sesión (verifica usuario + contraseña + crea la cookie)
    private readonly SignInManager<ApplicationUser> _signInManager;

    // UserManager permite consultar datos del usuario: roles, email, etc.
    private readonly UserManager<ApplicationUser>  _userManager;

    // Los servicios se inyectan automáticamente por el contenedor de dependencias
    public LoginModel(SignInManager<ApplicationUser> signInManager,
                      UserManager<ApplicationUser>  userManager)
    {
        _signInManager = signInManager;
        _userManager   = userManager;
    }

    // [BindProperty] hace que ASP.NET llene esta propiedad automáticamente con los datos del form
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    // Mensaje de error que se muestra en la vista si algo falla
    public string? ErrorMessage { get; set; }

    // OnGet se ejecuta cuando el usuario llega a la página por primera vez (GET)
    public void OnGet() { }

    // OnPostAsync se ejecuta cuando el usuario envía el formulario (POST)
    public async Task<IActionResult> OnPostAsync()
    {
        // Si los campos no pasan las validaciones ([Required], [EmailAddress]), vuelve a la página
        if (!ModelState.IsValid)
            return Page();

        // Intenta autenticar al usuario con email y contraseña
        // lockoutOnFailure: false → no bloquea la cuenta después de varios intentos fallidos
        var result = await _signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        // Aunque las credenciales sean correctas, un cliente NO puede acceder al panel Razor
        // Buscamos al usuario y verificamos su rol
        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user != null && await _userManager.IsInRoleAsync(user, AppRoles.Customer))
        {
            // Cerramos la sesión que acabamos de abrir para que no quede logueado
            await _signInManager.SignOutAsync();
            ErrorMessage = "Access denied. This panel is for administrators only.";
            return Page();
        }

        // Todo ok — redirige al dashboard
        return RedirectToPage("/Dashboard/Index");
    }
}

// Clase interna que representa los campos del formulario de login
// Se usa junto con [BindProperty] para recibir los datos del form
public class LoginInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    // DataType.Password indica a Razor que renderice el campo como <input type="password">
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // "Recuérdame" — si es true la cookie dura más tiempo (definido en Program.cs)
    public bool RememberMe { get; set; }
}
