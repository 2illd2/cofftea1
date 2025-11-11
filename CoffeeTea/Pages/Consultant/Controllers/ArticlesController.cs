
using ApiCoffeeTea.Data;
using ApiCoffeeTea.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/articles")]
[Authorize(Roles = "consultant,admin")]
public class ArticlesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ArticlesController(AppDbContext db) => _db = db;

    // список (внутренний, можно видеть черновики)
    [HttpGet]
    public async Task<ActionResult<object>> List(
        [FromQuery] string? q,
        [FromQuery] bool onlyPublished = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.articles
            .Include(a => a.category)
            .Where(a => !a.deleted);

        if (onlyPublished) query = query.Where(a => a.is_published);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.ToLower();
            query = query.Where(a => a.title.ToLower().Contains(term) ||
                                     (a.summary != null && a.summary.ToLower().Contains(term)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.published_at ?? DateTime.MinValue)
            .ThenByDescending(a => a.id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ArticleListItemDto(
                a.id,
                a.title,
                a.category != null ? a.category.name : "Без категории",
                a.is_published,
                a.published_at))
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    // получить одну для формы
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        var a = await _db.articles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.id == id && !x.deleted);
        if (a is null) return NotFound();

        return Ok(new
        {
            Title = a.title,
            Slug = a.slug,
            Summary = a.summary,
            Content = a.content,
            CategoryId = a.category_id,
            CoverImageUrl = a.cover_image_url,
            IsPublished = a.is_published
        });
    }

    // создать
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ArticleSaveDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Title is required");

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? MakeSlug(dto.Title) : dto.Slug.Trim();

        // проверка уникальности slug
        var slugExists = await _db.articles.AnyAsync(x => x.slug == slug && !x.deleted);
        if (slugExists) return Conflict("Slug already exists");

        var nowLocal = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified); // TIMESTAMP (без TZ)

        var a = new article
        {
            title = dto.Title.Trim(),
            slug = slug,
            summary = dto.Summary,
            content = dto.Content,
            category_id = dto.CategoryId,
            cover_image_url = dto.CoverImageUrl,
            is_published = dto.IsPublished,
            published_at = dto.IsPublished ? nowLocal : null,
            deleted = false
        };

        _db.articles.Add(a);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = a.id }, new { id = a.id });
    }

    // обновить
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ArticleSaveDto dto)
    {
        var a = await _db.articles.FirstOrDefaultAsync(x => x.id == id && !x.deleted);
        if (a is null) return NotFound();

        a.title = dto.Title?.Trim() ?? a.title;

        var newSlug = string.IsNullOrWhiteSpace(dto.Slug) ? MakeSlug(a.title) : dto.Slug.Trim();
        if (!string.Equals(a.slug, newSlug, StringComparison.Ordinal))
        {
            var slugBusy = await _db.articles.AnyAsync(x => x.id != id && x.slug == newSlug && !x.deleted);
            if (slugBusy) return Conflict("Slug already exists");
            a.slug = newSlug;
        }

        a.summary = dto.Summary;
        a.content = dto.Content ?? a.content;
        a.category_id = dto.CategoryId;
        a.cover_image_url = dto.CoverImageUrl;

        // публикация/снятие публикации (TIMESTAMP без TZ)
        var nowLocal = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        if (!a.is_published && dto.IsPublished)
        {
            a.is_published = true;
            a.published_at = nowLocal;
        }
        else if (a.is_published && !dto.IsPublished)
        {
            a.is_published = false;
            a.published_at = null;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // удалить (soft)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.articles.FirstOrDefaultAsync(x => x.id == id && !x.deleted);
        if (a is null) return NotFound();
        a.deleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static string MakeSlug(string title)
    {
        var s = title.ToLowerInvariant();
        var arr = s.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(arr).Trim('-');
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return string.IsNullOrWhiteSpace(slug) ? $"post-{Guid.NewGuid():N}" : slug;
    }
}
