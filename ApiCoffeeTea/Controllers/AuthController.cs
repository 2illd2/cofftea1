using ApiCoffeeTea.Data;
using ApiCoffeeTea.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db) => _db = db;

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email и пароль обязательны.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var exists = await _db.users.AnyAsync(u => u.email == email && !u.deleted);
        if (exists) return Conflict("Пользователь с таким email уже существует.");

        // Роль по умолчанию
        var role = await _db.roles.FirstOrDefaultAsync(r => r.name == "customer" && !r.deleted)
                   ?? new role { name = "customer", deleted = false };
        if (role.id == 0) _db.roles.Add(role);

        // Создание пользователя
        var user = new user
        {
            first_name = dto.FirstName?.Trim() ?? "",
            last_name = dto.LastName?.Trim() ?? "",
            middle_name = dto.MiddleName?.Trim(),
            email = email,
            phone = dto.Phone?.Trim(),
            role = role,
            password_hash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            created_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),

            deleted = false
        };

        _db.users.Add(user);
        await _db.SaveChangesAsync();

        var result = new UserDto(user.id, user.first_name, user.last_name, user.middle_name, user.email, user.phone, role.name);
        return Created($"/api/users/{user.id}", result);
    }
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login(LoginDto dto, [FromServices] IConfiguration cfg)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.users
            .Include(u => u.role)
            .FirstOrDefaultAsync(u => u.email == email && !u.deleted);

        if (user is null) return Unauthorized("Неверный логин или пароль.");

        var ok = BCrypt.Net.BCrypt.Verify(dto.Password, user.password_hash);
        if (!ok) return Unauthorized("Неверный логин или пароль.");

        var userDto = new UserDto(user.id, user.first_name, user.last_name, user.middle_name, user.email, user.phone, user.role.name);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
        new Claim(ClaimTypes.Email, user.email),
        new Claim(ClaimTypes.Role, user.role.name),
        new Claim(ClaimTypes.Name, $"{user.first_name} {user.last_name}".Trim())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: cfg["Jwt:Issuer"],
            audience: cfg["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return Ok(new LoginEnvelope(token, userDto));
    }

}
