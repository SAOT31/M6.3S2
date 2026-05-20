using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.ViewModels;

public class ClientViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Document type is required")]
    [Display(Name = "Document Type")]
    public string DocumentType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Document number is required")]
    [StringLength(20)]
    [Display(Name = "Document Number")]
    public string DocumentNumber { get; set; } = string.Empty;

    // [EmailAddress] verifica que tenga formato válido: usuario@dominio.com
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    // [Phone] verifica formato de teléfono básico
    [Phone(ErrorMessage = "Invalid phone number")]
    [StringLength(20)]
    [Display(Name = "Phone")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    // La edad se recibe como string para poder aplicar int.Parse con try-catch (Task 7)
    // Si fuera int directamente, el framework haría la conversión automática y
    // mostraría un error genérico — así tenemos control total del mensaje de error
    [Display(Name = "Age")]
    public string AgeInput { get; set; } = string.Empty;

    // Este se llena en el PageModel después de convertir AgeInput exitosamente
    public int Age { get; set; }

    public static List<string> DocumentTypes => ["CC", "NIT", "CE", "Passport"];
}
