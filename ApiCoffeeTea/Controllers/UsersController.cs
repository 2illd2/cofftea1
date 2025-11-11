using ApiCoffeeTea.Data;
using ApiCoffeeTea.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var list = await _db.users
            .Include(u => u.role)
            .Where(u => !u.deleted)
            .OrderByDescending(u => u.id)
            .Select(u => new UserDto(u.id, u.first_name, u.last_name, u.middle_name, u.email, u.phone, u.role.name))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetOne(int id)
    {
        var u = await _db.users.Include(x => x.role).FirstOrDefaultAsync(x => x.id == id && !x.deleted);
        if (u is null) return NotFound();
        return new UserDto(u.id, u.first_name, u.last_name, u.middle_name, u.email, u.phone, u.role.name);
    }

    // Создание админом
    public record CreateUserDto(string FirstName, string LastName, string? MiddleName, string Email, string? Phone, string Password, string Role);
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _db.users.AnyAsync(x => x.email == email && !x.deleted))
            return Conflict("Email уже занят.");

        var role = await _db.roles.FirstOrDefaultAsync(r => r.name == dto.Role && !r.deleted);
        if (role is null) return BadRequest("Роль не найдена.");

        var u = new user
        {
            first_name = dto.FirstName,
            last_name = dto.LastName,
            middle_name = dto.MiddleName,
            email = email,
            phone = dto.Phone,
            role_id = role.id,
            password_hash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            created_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            deleted = false
        };
        _db.users.Add(u);
        await _db.SaveChangesAsync();

        return Created($"/api/users/{u.id}", new UserDto(u.id, u.first_name, u.last_name, u.middle_name, u.email, u.phone, role.name));
    }

    public record UpdateUserDto(string FirstName, string LastName, string? MiddleName, string? Phone, string? Role, string? NewPassword);
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, UpdateUserDto dto)
    {
        var u = await _db.users.Include(x => x.role).FirstOrDefaultAsync(x => x.id == id && !x.deleted);
        if (u is null) return NotFound();

        u.first_name = dto.FirstName;
        u.last_name = dto.LastName;
        u.middle_name = dto.MiddleName;
        u.phone = dto.Phone;

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var role = await _db.roles.FirstOrDefaultAsync(r => r.name == dto.Role && !r.deleted);
            if (role is null) return BadRequest("Роль не найдена.");
            u.role_id = role.id;
        }

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            u.password_hash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _db.SaveChangesAsync();

        var roleName = (await _db.roles.FindAsync(u.role_id))?.name ?? "customer";
        return new UserDto(u.id, u.first_name, u.last_name, u.middle_name, u.email, u.phone, roleName);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _db.users.FirstOrDefaultAsync(x => x.id == id && !x.deleted);
        if (u is null) return NotFound();
        u.deleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
