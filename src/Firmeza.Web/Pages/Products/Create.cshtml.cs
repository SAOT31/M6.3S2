using Firmeza.Infrastructure.Data;
using Firmeza.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.Web.Pages.Products;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // ProductViewModel tiene las validaciones ([Required], [Range], etc.)
    // Al hacer POST, ASP.NET llena este objeto con los datos del formulario
    [BindProperty]
    public ProductViewModel Product { get; set; } = new();

    public string? ErrorMessage { get; set; }

    // GET: muestra el formulario vacío
    public void OnGet() { }

    // POST: recibe los datos del formulario y guarda el producto
    public async Task<IActionResult> OnPostAsync()
    {
        // Si alguna validación del ViewModel falla, vuelve al formulario con los errores
        if (!ModelState.IsValid)
            return Page();

        try
        {
            // Convertimos el ViewModel (datos del form) a la entidad real que va a la BD
            // Nunca guardamos el ViewModel directamente — eso rompería la separación de capas
            var product = new Core.Entities.Product
            {
                Name        = Product.Name,
                Description = Product.Description,
                Category    = Product.Category,
                Unit        = Product.Unit,
                Price       = Product.Price,
                Stock       = Product.Stock,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow
            };

            // Add() marca la entidad para ser insertada
            _context.Products.Add(product);

            // SaveChangesAsync() ejecuta el INSERT en la BD
            await _context.SaveChangesAsync();

            // RedirectToPage redirige al listado de productos después de guardar
            return RedirectToPage("/Products/Index");
        }
        catch (Exception ex)
        {
            // Si algo falla (BD no disponible, restricción única, etc.)
            // mostramos el error sin que la app se caiga
            ErrorMessage = $"Error saving product: {ex.Message}";
            return Page();
        }
    }
}
