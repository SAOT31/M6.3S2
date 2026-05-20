using Firmeza.Core.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Clients;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Client> Clients { get; set; } = [];
    public string? Search { get; set; }

    public async Task OnGetAsync(string? search)
    {
        Search = search;

        var query = _context.Clients.AsQueryable();

        // La búsqueda cubre tanto el nombre completo como el número de documento
        // Así el usuario puede buscar "Juan" o "1234567890" y encuentra al cliente
        if (!string.IsNullOrEmpty(search))
        {
            var lower = search.ToLower();
            query = query.Where(c =>
                // concatena FirstName + espacio + LastName para buscar en el nombre completo
                (c.FirstName + " " + c.LastName).ToLower().Contains(lower) ||
                c.DocumentNumber.ToLower().Contains(lower));
        }

        Clients = await query
            .OrderBy(c => c.FirstName)
            .ToListAsync();
    }
}
