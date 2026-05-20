namespace Firmeza.Core.Entities;

public class Client
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    // Tipo de documento: CC (cédula), NIT (empresa), CE (extranjería)
    public string DocumentType { get; set; } = string.Empty;

    // Número de documento — tiene índice único en la BD, no puede repetirse
    public string DocumentNumber { get; set; } = string.Empty;

    // Email también es único en la BD
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    // La edad se guarda como int pero se recibe como texto en el formulario
    // La conversión se hace con int.Parse() + try-catch (Task 7 del taller)
    public int Age { get; set; }

    // Se asigna automáticamente al crear el cliente
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Lista de ventas asociadas — EF la llena cuando haces Include(c => c.Sales)
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    // Propiedad calculada — no se guarda en BD, solo se usa en las vistas
    // Al escribir @c.FullName en Razor ya tienes el nombre completo listo
    public string FullName => $"{FirstName} {LastName}";
}
