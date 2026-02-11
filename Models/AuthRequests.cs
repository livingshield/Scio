using System.ComponentModel.DataAnnotations;

namespace ScioApp.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Uživatelské jméno je povinné")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Heslo je povinné")]
    public string Password { get; set; } = "";
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Jméno je povinné")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Email je povinný")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Uživatelské jméno je povinné")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Heslo je povinné")]
    [MinLength(6, ErrorMessage = "Heslo musí mít aspoň 6 znaků")]
    public string Password { get; set; } = "";
}
