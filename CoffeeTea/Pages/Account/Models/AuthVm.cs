
using System.ComponentModel.DataAnnotations;

namespace CoffeeTea.Pages.Account.Models;

public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}

public class RegisterVm
{
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    public string? MiddleName { get; set; }
    [Required, EmailAddress] public string Email { get; set; } = "";
    public string? Phone { get; set; }
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}
