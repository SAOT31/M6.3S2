using Firmeza.Infrastructure.Data;
using Firmeza.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.Web.Pages.Clients;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ClientViewModel Client { get; set; } = new();

    public string? ErrorMessage { get; set; }

    // AgeError es separado del ErrorMessage porque el campo de edad
    // se muestra justo debajo del input, no en la barra de error general
    public string? AgeError { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        // ── TASK 7: VALIDACIÓN DE EDAD CON TRY-CATCH ──────────────────────────
        // AgeInput llega como string desde el formulario
        // int.Parse lanza FormatException si el valor no es un número entero
        int parsedAge;
        try
        {
            parsedAge = int.Parse(Client.AgeInput);

            // Validación de rango adicional — un número es válido pero 999 no tiene sentido
            if (parsedAge < 0 || parsedAge > 120)
                throw new ArgumentOutOfRangeException(nameof(parsedAge), "Age must be between 0 and 120.");
        }
        catch (FormatException)
        {
            // El usuario escribió "veinticinco" o "25.5" — no es un entero
            AgeError = "Age must be a whole number (e.g. 25).";
            return Page(); // vuelve al formulario con el mensaje de error
        }
        catch (ArgumentOutOfRangeException ex)
        {
            AgeError = ex.Message;
            return Page();
        }
        // ── FIN TASK 7 ─────────────────────────────────────────────────────────

        // Verificamos el resto de validaciones del ViewModel (email, required, etc.)
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var client = new Core.Entities.Client
            {
                FirstName      = Client.FirstName,
                LastName       = Client.LastName,
                DocumentType   = Client.DocumentType,
                DocumentNumber = Client.DocumentNumber,
                Email          = Client.Email,
                Phone          = Client.Phone,
                Address        = Client.Address,
                Age            = parsedAge, // usamos el valor ya convertido y validado
                CreatedAt      = DateTime.UtcNow
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Clients/Index");
        }
        catch (Exception ex)
        {
            // Puede fallar si el email o documento ya existen (índice único en la BD)
            ErrorMessage = $"Error saving client: {ex.Message}";
            return Page();
        }
    }
}
