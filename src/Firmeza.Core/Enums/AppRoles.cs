namespace Firmeza.Core.Enums;

// Identity maneja los roles como strings, no como enum
// Si usáramos enum tendríamos que convertirlo a string cada vez que llamamos
// a roleManager.CreateAsync() o userManager.AddToRoleAsync()
// Con constantes string se usa directamente: AppRoles.Admin
public static class AppRoles
{
    // "static" en la clase significa que no se puede instanciar (new AppRoles() daría error)
    // "const" significa que el valor se incrusta en tiempo de compilación, es inmutable

    public const string Admin    = "Admin";
    public const string Customer = "Customer";
}
