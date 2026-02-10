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

    [HttpGet]
    public async Task<ActionResult<object>> List([FromQuery] string? q, [FromQuery] bool onlyPublished = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.articles
            .Include(a => a.category)
            .Where(a => !a.deleted);

        if (onlyPublished)
            query = query.Where(a => a.is_published);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.title.ToLower().Contains(q.ToLower()));

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
                a.published_at
            ))
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    // Получить одну
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ArticleSaveDto>> Get(int id)
    {
        var a = await _db.articles.FirstOrDefaultAsync(x => x.id == id && !x.deleted);
        if (a is null) return NotFound();

        return new ArticleSaveDto
        {
            Title = a.title,
            Slug = a.slug,
            Summary = a.summary,
            Content = a.content,
            CoverImageUrl = a.cover_image_url,
            CategoryId = a.category_id,
            IsPublished = a.is_published
        };
    }

    // Создать
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ArticleSaveDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Title is required.");

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? MakeSlug(dto.Title)
            : dto.Slug.Trim();

        var a = new article
        {
            title = dto.Title.Trim(),
            slug = slug,
            summary = dto.Summary,
            content = dto.Content,
            cover_image_url = dto.CoverImageUrl,
            category_id = dto.CategoryId,
            is_published = dto.IsPublished,
            published_at = dto.IsPublished ? DateTime.UtcNow : null,
            deleted = false
        };

        _db.articles.Add(a);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = a.id }, new { id = a.id });
    }

    // Обновить
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ArticleSaveDto dto)
    {
        var a = await _db.articles.FirstOrDefaultAsync(x => x.id == id && !x.deleted);
        if (a is null) return NotFound();

        a.title = dto.Title?.Trim() ?? a.title;
        a.slug = string.IsNullOrWhiteSpace(dto.Slug) ? MakeSlug(a.title) : dto.Slug.Trim();
        a.summary = dto.Summary;
        a.content = dto.Content ?? a.content;
        a.cover_image_url = dto.CoverImageUrl;
        a.category_id = dto.CategoryId;

        if (!a.is_published && dto.IsPublished)
        {
            a.is_published = true;
            a.published_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }
        else if (a.is_published && !dto.IsPublished)
        {
            a.is_published = false;
            a.published_at = null;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Удалить
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.articles.FirstOrDefaultAsync(x => x.id == id && !x.deleted);
        if (a is null) return NotFound();
        a.deleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Список только опубликованных статей (для /articles)
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> PublicList(
        [FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var query = _db.articles
            .Include(a => a.category)
            .Where(a => a.is_published && !a.deleted);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => a.title.ToLower().Contains(q.ToLower()));

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.published_at ?? DateTime.MinValue)
            .ThenByDescending(a => a.id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                id = a.id,
                title = a.title,
                slug = a.slug,
                summary = a.summary,
                coverImageUrl = a.cover_image_url,
                category = a.category != null ? a.category.name : null,
                publishedAt = a.published_at
            })
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    // Детальная статья по slug (для /articles/{slug})
    [HttpGet("by-slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetBySlug(string slug)
    {
        var a = await _db.articles
            .Include(x => x.category)
            .FirstOrDefaultAsync(x => x.slug == slug && x.is_published && !x.deleted);

        if (a is null) return NotFound();

        return Ok(new
        {
            id = a.id,
            title = a.title,
            slug = a.slug,
            summary = a.summary,
            content = a.content,
            coverImageUrl = a.cover_image_url,
            category = a.category != null ? a.category.name : null,
            publishedAt = a.published_at
        });
    }
    private static string MakeSlug(string title)
    {
        // простой слаггер: латиница/цифры/дефис
        var s = title.ToLowerInvariant();
        var arr = s.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(arr).Trim('-');
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return string.IsNullOrWhiteSpace(slug) ? $"post-{Guid.NewGuid():N}" : slug;
    }
}
