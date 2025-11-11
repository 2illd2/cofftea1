using ApiCoffeeTea.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/article-categories")]
[Authorize(Roles = "consultant,admin")]
public class ArticleCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ArticleCategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<object>> List()
    {
        var items = await _db.article_categories
            .Where(c => !c.deleted)
            .OrderBy(c => c.name)
            .Select(c => new { c.id, c.name })
            .ToListAsync();

        return Ok(items);
    }
}
