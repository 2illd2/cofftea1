using ApiCoffeeTea.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/admin/article-categories")]
[Authorize(Roles = "admin")]
public class AdminArticleCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminArticleCategoriesController(AppDbContext db) => _db = db;

    // LIST (в админке показываем и удалённые, с признаком)
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _db.article_categories
            .OrderBy(c => c.name)
            .Select(c => new { c.id, c.name, c.deleted })
            .ToListAsync();

        return Ok(items);
    }

    // GET
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var c = await _db.article_categories.FirstOrDefaultAsync(x => x.id == id);
        if (c is null) return NotFound();
        return Ok(new { c.id, c.name, c.deleted });
    }

    // CREATE
    public record SaveDto(string Name);
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
        var exists = await _db.article_categories.AnyAsync(x => !x.deleted && x.name.ToLower() == dto.Name.Trim().ToLower());
        if (exists) return Conflict("Такая категория уже есть.");

        var c = new article_category { name = dto.Name.Trim(), deleted = false };
        _db.article_categories.Add(c);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.id }, new { id = c.id });
    }

    // UPDATE
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveDto dto)
    {
        var c = await _db.article_categories.FirstOrDefaultAsync(x => x.id == id);
        if (c is null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");

        var exists = await _db.article_categories.AnyAsync(x =>
            x.id != id && !x.deleted && x.name.ToLower() == dto.Name.Trim().ToLower());
        if (exists) return Conflict("Такая категория уже есть.");

        c.name = dto.Name.Trim();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // SOFT DELETE
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.article_categories.FirstOrDefaultAsync(x => x.id == id);
        if (c is null) return NotFound();
        c.deleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // RESTORE 
    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var c = await _db.article_categories.FirstOrDefaultAsync(x => x.id == id);
        if (c is null) return NotFound();
        c.deleted = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
