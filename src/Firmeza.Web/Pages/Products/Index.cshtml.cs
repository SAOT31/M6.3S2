using Firmeza.Core.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Products;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lista de productos que se muestra en la tabla — la vista la lee con @Model.Products
    public List<Product> Products { get; set; } = [];

    // Guardamos los valores de búsqueda para que el formulario los muestre al volver a cargar
    public string? Search   { get; set; }
    public string? Category { get; set; }

    // Los parámetros search y category vienen del query string de la URL
    // Ejemplo: /Products?search=cemento&category=Cement
    public async Task OnGetAsync(string? search, string? category)
    {
        Search   = search;
        Category = category;

        // AsQueryable() permite ir armando la consulta de forma condicional
        // sin ejecutarla hasta el ToListAsync() final
        var query = _context.Products.AsQueryable();

        // Solo agrega el filtro si el usuario escribió algo en el buscador
        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

        // Solo filtra por categoría si el usuario eligió una
        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        // OrderBy + ToListAsync ejecuta el SELECT final con todos los filtros aplicados
        Products = await query
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
