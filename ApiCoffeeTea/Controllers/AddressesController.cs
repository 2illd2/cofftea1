using ApiCoffeeTea.Data;
using ApiCoffeeTea.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly AppDbContext _db;
    public AddressesController(AppDbContext db) => _db = db;

    private int? GetUserId()
    {
        var claim = User.FindFirst("UserId")
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub"); 

        if (claim != null && int.TryParse(claim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<AddressDto>>> Mine()
    {
        var uid = GetUserId();
        if (uid == null)
            return Unauthorized("User ID not found in token");

        return await _db.addresses
            .Where(a => a.user_id == uid && !a.deleted)
            .OrderByDescending(a => a.is_default)
            .ThenBy(a => a.id)
            .Select(a => new AddressDto(a.id, a.line1, a.city, a.postal_code, a.country, a.is_default))
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(AddressCreateDto dto)
    {
        var uid = GetUserId();
        if (uid == null)
            return Unauthorized("User ID not found in token");

        // Валидация данных
        if (string.IsNullOrWhiteSpace(dto.Line1) || string.IsNullOrWhiteSpace(dto.City))
            return BadRequest("Line1 and City are required");

        // Если ставим новый адрес по умолчанию — снимем флаг с остальных
        if (dto.IsDefault)
        {
            await _db.addresses
                .Where(a => a.user_id == uid && a.is_default && !a.deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.is_default, false));
        }

        var a = new address
        {
            user_id = uid.Value,
            line1 = dto.Line1.Trim(),
            city = dto.City.Trim(),
            postal_code = dto.PostalCode?.Trim() ?? "",
            country = dto.Country?.Trim() ?? "Россия",
            is_default = dto.IsDefault,
            deleted = false
        };

        _db.addresses.Add(a);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Mine), new AddressDto(a.id, a.line1, a.city, a.postal_code, a.country, a.is_default));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AddressCreateDto dto)
    {
        var uid = GetUserId();
        if (uid == null)
            return Unauthorized("User ID not found in token");

        var address = await _db.addresses
            .FirstOrDefaultAsync(a => a.id == id && a.user_id == uid && !a.deleted);

        if (address == null)
            return NotFound("Address not found");

        // Если ставим адрес по умолчанию — снимем флаг с остальных
        if (dto.IsDefault)
        {
            await _db.addresses
                .Where(a => a.user_id == uid && a.id != id && a.is_default && !a.deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.is_default, false));
        }

        address.line1 = dto.Line1.Trim();
        address.city = dto.City.Trim();
        address.postal_code = dto.PostalCode?.Trim() ?? "";
        address.country = dto.Country?.Trim() ?? "Россия";
        address.is_default = dto.IsDefault;

        await _db.SaveChangesAsync();

        return Ok(new AddressDto(address.id, address.line1, address.city, address.postal_code, address.country, address.is_default));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var uid = GetUserId();
        if (uid == null)
            return Unauthorized("User ID not found in token");

        var address = await _db.addresses
            .FirstOrDefaultAsync(a => a.id == id && a.user_id == uid && !a.deleted);

        if (address == null)
            return NotFound("Address not found");

        address.deleted = true;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}