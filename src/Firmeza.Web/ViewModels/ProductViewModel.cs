using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.ViewModels;

// Un ViewModel es un objeto diseñado exclusivamente para la vista
// Tiene validaciones y formato, pero NO tiene lógica de negocio
// La entidad Product (en Core) no tiene validaciones — esas van aquí
public class ProductViewModel
{
    // Se usa en Edit para saber cuál producto actualizar (no se muestra al usuario)
    public int Id { get; set; }

    // [Required] → el campo no puede estar vacío
    // [StringLength] → máximo de caracteres
    // [Display] → etiqueta que muestra Razor con asp-for (en vez del nombre de la propiedad)
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(150, ErrorMessage = "Max 150 characters")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unit is required")]
    [Display(Name = "Unit")]
    public string Unit { get; set; } = string.Empty;

    // [Range] valida que el valor esté entre los límites definidos
    [Required]
    [Range(0.01, 99999999, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    [Display(Name = "Stock")]
    public int Stock { get; set; }

    // Listas estáticas para los <select> del formulario
    // static → no necesitan crear una instancia del ViewModel para acceder a ellas
    public static List<string> Categories =>
        ["Cement", "Steel Bar", "Paint", "Sand", "Brick", "Tile", "Other"];

    public static List<string> Units =>
        ["unit", "kg", "m²", "m³", "bag", "liter", "box"];
}
