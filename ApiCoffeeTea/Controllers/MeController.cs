// ApiCoffeeTea/Controllers/MeController.cs
using ApiCoffeeTea.Data;
using ApiCoffeeTea.DTO;
using ApiCoffeeTea.Utils;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/me")]
[Authorize] 
public class MeController : ControllerBase
{
    private readonly AppDbContext _db;
    public MeController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<MeDto>> Get()
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var u = await _db.users
            .Include(x => x.role)
            .FirstOrDefaultAsync(x => x.id == uid && !x.deleted);

        if (u is null) return NotFound();

        var roleName = u.role?.name ?? "customer"; 
        return new MeDto(u.id, u.first_name, u.last_name, u.middle_name,
                         u.email, u.phone, roleName, u.created_at);
    }


    // Обновление ФИО/телефона
    [HttpPut]
    [ProducesResponseType(typeof(MeDto), 200)]
    public async Task<ActionResult<MeDto>> Update(MeUpdateDto dto)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var u = await _db.users
            .Include(x => x.role)
            .FirstOrDefaultAsync(x => x.id == uid && !x.deleted);

        if (u is null) return NotFound();

        u.first_name = dto.FirstName.Trim();
        u.last_name = dto.LastName.Trim();
        u.middle_name = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();
        u.phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();

        await _db.SaveChangesAsync();

        return new MeDto(
            u.id, u.first_name, u.last_name, u.middle_name, u.email, u.phone, u.role.name, u.created_at
        );
    }

    // Смена пароля
    [HttpPut("password")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var u = await _db.users.FirstOrDefaultAsync(x => x.id == uid && !x.deleted);
        if (u is null) return NotFound();

        // Проверяем текущий пароль
        var ok = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, u.password_hash);
        if (!ok) return BadRequest("Текущий пароль неверный.");

        // Меняем на новый
        u.password_hash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
