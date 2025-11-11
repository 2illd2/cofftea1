
namespace ApiCoffeeTea.DTO;

public record RegisterDto(
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    string? Phone,
    string Password
);

public record LoginDto(string Email, string Password);

public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string Email,
    string? Phone,
    string Role
);
public record LoginEnvelope(string token, UserDto user);
