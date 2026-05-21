using Firmeza.Core.Services;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Firmeza.Web.Pages.Import;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ImportParserService  _parser;

    public IndexModel(ApplicationDbContext context, ImportParserService parser)
    {
        _context = context;
        _parser  = parser;
    }

    public List<string> ImportLog       { get; set; } = [];
    public int ProductsInserted { get; set; }
    public int ProductsUpdated  { get; set; }
    public int ClientsInserted  { get; set; }
    public int ClientsUpdated   { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            ImportLog.Add("✗ No file received. Please select a .xlsx file.");
            return Page();
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ImportLog.Add("✗ Only .xlsx files are supported.");
            return Page();
        }

        // EPPlus licencia non-commercial
        ExcelPackage.License.SetNonCommercialOrganization("Firmeza");

        using var stream  = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);

        var sheet = package.Workbook.Worksheets.FirstOrDefault();
        if (sheet is null || sheet.Dimension is null)
        {
            ImportLog.Add("✗ The file has no data or the first sheet is empty.");
            return Page();
        }

        // Leer encabezados de la primera fila
        int colCount = sheet.Dimension.End.Column;
        int rowCount = sheet.Dimension.End.Row;

        var headers = new Dictionary<int, string>(); // colIndex → header name
        for (int c = 1; c <= colCount; c++)
        {
            var h = sheet.Cells[1, c].Text?.Trim();
            if (!string.IsNullOrEmpty(h))
                headers[c] = h;
        }

        ImportLog.Add($"─── File: {file.FileName}  |  Rows: {rowCount - 1}  |  Columns: {headers.Count} ───");

        // Cargar datos existentes para el upsert
        var existingProducts = await _context.Products.ToDictionaryAsync(p => p.Name.ToLower());
        var existingClients  = await _context.Clients.ToDictionaryAsync(c => c.DocumentNumber);

        // Procesar cada fila de datos (desde fila 2)
        for (int r = 2; r <= rowCount; r++)
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (col, header) in headers)
                row[header] = sheet.Cells[r, col].Text?.Trim() ?? string.Empty;

            // Ignorar filas completamente vacías
            if (row.Values.All(string.IsNullOrWhiteSpace)) continue;

            // ── Intentar parsear como Producto ──────────────────────────
            var (product, productError) = _parser.ParseProductRow(row);
            if (product != null)
            {
                var key = product.Name.ToLower();
                if (existingProducts.TryGetValue(key, out var existing))
                {
                    // Actualizar
                    existing.Category    = product.Category;
                    existing.Unit        = product.Unit;
                    existing.Description = product.Description;
                    existing.Price       = product.Price;
                    existing.Stock       = product.Stock;
                    existing.UpdatedAt   = DateTime.UtcNow;
                    ProductsUpdated++;
                    ImportLog.Add($"✓ Row {r}: Product UPDATED → '{product.Name}'");
                }
                else
                {
                    _context.Products.Add(product);
                    existingProducts[key] = product;
                    ProductsInserted++;
                    ImportLog.Add($"✓ Row {r}: Product INSERTED → '{product.Name}'");
                }
            }

            // ── Intentar parsear como Cliente ────────────────────────────
            var (client, clientError) = _parser.ParseClientRow(row);
            if (client != null)
            {
                if (existingClients.TryGetValue(client.DocumentNumber, out var existingClient))
                {
                    // Actualizar
                    existingClient.FirstName  = client.FirstName;
                    existingClient.LastName   = client.LastName;
                    existingClient.Email      = client.Email;
                    existingClient.Phone      = client.Phone;
                    existingClient.Address    = client.Address;
                    existingClient.Age        = client.Age;
                    ClientsUpdated++;
                    ImportLog.Add($"✓ Row {r}: Client UPDATED  → '{client.FullName}' ({client.DocumentNumber})");
                }
                else
                {
                    _context.Clients.Add(client);
                    existingClients[client.DocumentNumber] = client;
                    ClientsInserted++;
                    ImportLog.Add($"✓ Row {r}: Client INSERTED → '{client.FullName}' ({client.DocumentNumber})");
                }
            }

            // Si ambos fallaron y la fila no estaba vacía, registrar aviso
            if (product is null && client is null)
            {
                var info = productError ?? clientError ?? "Could not identify row as product or client.";
                ImportLog.Add($"✗ Row {r}: SKIPPED — {info}");
            }
        }

        await _context.SaveChangesAsync();

        ImportLog.Add($"─── Done: {ProductsInserted} products inserted, {ProductsUpdated} updated | " +
                      $"{ClientsInserted} clients inserted, {ClientsUpdated} updated ───");

        return Page();
    }
}
