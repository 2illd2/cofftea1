using ApiCoffeeTea.DTO;
using System.ComponentModel.DataAnnotations;

namespace CoffeeTea.Pages.Account.Models;

public class ProfileVm
{
    public int Id { get; set; }

    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    public string? MiddleName { get; set; }

    [Required, EmailAddress] public string Email { get; set; } = "";
    public string? Phone { get; set; }

    public string Role { get; set; } = "customer";
    public DateTime CreatedAt { get; set; }
}

// DTO’шки клиента для вызова API /api/me
public record MeDto(
    int Id, string FirstName, string LastName, string? MiddleName,
    string Email, string? Phone, string Role, DateTime created_at);

public class MeUpdateDto
{
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
}
// что возвращает API при логине
public class LoginEnvelope
{
    public string token { get; set; } = "";
    public UserDto user { get; set; } = default!;
}
