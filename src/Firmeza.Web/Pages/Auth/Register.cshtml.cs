using Firmeza.Core.Enums;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RegisterModel(UserManager<ApplicationUser>  userManager,
                         SignInManager<ApplicationUser> signInManager)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            // Creamos el objeto usuario con los datos del formulario
            // UserName y Email deben ser iguales — Identity los usa para el login
            var user = new ApplicationUser
            {
                UserName       = Input.Email,
                Email          = Input.Email,
                DisplayName    = $"{Input.FirstName} {Input.LastName}",
                EmailConfirmed = true // saltamos la verificación de email para el taller
            };

            // CreateAsync crea el usuario en la BD con la contraseña hasheada (nunca en texto plano)
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                // Identity devuelve errores claros: email duplicado, contraseña débil, etc.
                ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return Page();
            }

            // Todo registro público crea un Cliente — nunca un Admin
            // Así protegemos el panel aunque alguien intente registrarse con email de admin
            await _userManager.AddToRoleAsync(user, AppRoles.Customer);

            // TempData guarda un mensaje que sobrevive UNA redirección
            // Lo leemos en Login.cshtml para mostrar el mensaje de éxito
            TempData["SuccessMessage"] = "Account created successfully. Please sign in.";
            return RedirectToPage("/Auth/Login");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Registration error: {ex.Message}";
            return Page();
        }
    }
}

public class RegisterInput
{
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    // StringLength define mínimo y máximo — el error se muestra automáticamente en la vista
    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    // Compare hace que este campo deba coincidir con la propiedad "Password"
    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
