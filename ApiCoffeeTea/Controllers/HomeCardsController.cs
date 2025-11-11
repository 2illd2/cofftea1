using ApiCoffeeTea.DTO;
using ApiCoffeeTea.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/home")]
public class HomeCardsController : ControllerBase
{
    private readonly AppDbContext _db;
    public HomeCardsController(AppDbContext db) => _db = db;


    [HttpGet("cards/by-categories")]
    public async Task<ActionResult<IEnumerable<HomeCardDto>>> GetCardsByCategories()
    {
        var targetCategories = new[] { "Обжарка недели", "Топ-чай", "Гайды" };

        var latest = await _db.articles
            .Include(a => a.category)
            .Where(a => !a.deleted && a.is_published && a.category != null && !a.category.deleted
                        && targetCategories.Contains(a.category!.name))
            .OrderByDescending(a => a.published_at ?? DateTime.MinValue)
            .ThenByDescending(a => a.id)
            .AsNoTracking()
            .ToListAsync();

        var picked = latest
            .GroupBy(a => a.category!.name)
            .Select(g => g.First()) // по одной самой новой статье на категорию
            .OrderBy(a => Array.IndexOf(targetCategories, a.category!.name))
            .Select(a => new HomeCardDto(
                Title: a.title,
                Text: a.summary ?? string.Empty,
                ImageUrl: string.IsNullOrWhiteSpace(a.cover_image_url)
                    ? "/img/placeholders/article-600x400.png"
                    : a.cover_image_url!,
                Link: $"/articles/{a.slug}",
                Badge: a.category!.name
            ))
            .ToList();

        return Ok(picked);
    }
}
