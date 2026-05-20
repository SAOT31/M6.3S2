using Firmeza.Infrastructure.Data;
using Firmeza.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Pages.Dashboard;

// [Authorize(Roles = "Admin")] hace que el framework bloquee la página si el usuario
// no está logueado o no tiene el rol Admin — redirige a /Auth/Login automáticamente
[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    // Inyectamos el contexto de BD para poder hacer consultas
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // Esta propiedad se llena en OnGetAsync y la vista la lee con @Model.Stats
    public DashboardViewModel Stats { get; set; } = new();

    // OnGetAsync se ejecuta cuando alguien entra al dashboard (petición GET)
    public async Task OnGetAsync()
    {
        // CountAsync() hace un SELECT COUNT(*) — más eficiente que traer todos los registros
        Stats.TotalProducts = await _context.Products.CountAsync();
        Stats.TotalClients  = await _context.Clients.CountAsync();
        Stats.TotalSales    = await _context.Sales.CountAsync();

        // SumAsync suma el campo Total solo de ventas completadas
        // El (decimal?) es para que no falle si no hay ventas todavía (null → 0)
        Stats.TotalRevenue = await _context.Sales
            .Where(s => s.Status == "Completed")
            .SumAsync(s => (decimal?)s.Total) ?? 0;

        // Include() hace un JOIN con la tabla de clientes para traer el nombre
        // Select() transforma el resultado en el DTO que necesita la vista
        Stats.RecentSales = await _context.Sales
            .Include(s => s.Client)
            .OrderByDescending(s => s.SaleDate) // las más recientes primero
            .Take(5)                             // solo las últimas 5
            .Select(s => new RecentSaleItem
            {
                SaleId     = s.Id,
                ClientName = s.Client.FirstName + " " + s.Client.LastName,
                Total      = s.Total,
                Status     = s.Status,
                Date       = s.SaleDate
            })
            .ToListAsync();

        // Productos con stock crítico (menos de 10 unidades)
        Stats.LowStockProducts = await _context.Products
            .Where(p => p.Stock < 10)
            .OrderBy(p => p.Stock) // los más críticos primero
            .Select(p => new LowStockItem
            {
                ProductName = p.Name,
                Stock       = p.Stock,
                Unit        = p.Unit
            })
            .ToListAsync();
    }
}
