
using System.ComponentModel.DataAnnotations;

namespace ApiCoffeeTea.DTO;

public record MeDto(
    int Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    string? Phone,
    string Role,
    DateTime created_at
);

public class MeUpdateDto
{
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
}

public class ChangePasswordDto
{
    [Required] public string CurrentPassword { get; set; } = "";
    [Required, MinLength(6)] public string NewPassword { get; set; } = "";
}
