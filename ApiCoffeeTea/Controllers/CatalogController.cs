using ApiCoffeeTea.Data;
using ApiCoffeeTea.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly AppDbContext _db;
    public CatalogController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> Get(
        [FromQuery] int? categoryId,
        [FromQuery] string? q,
        [FromQuery] string? type,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = _db.products
            .AsNoTracking()
            .Where(p => !p.deleted);

        if (categoryId is not null) query = query.Where(p => p.category_id == categoryId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.name.ToLower().Contains(term) ||
                (p.description != null && p.description.ToLower().Contains(term)) ||
                p.sku.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(p => p.type == type);
        if (minPrice is not null) query = query.Where(p => p.price >= minPrice);
        if (maxPrice is not null) query = query.Where(p => p.price <= maxPrice);

        var total = await query.CountAsync();
        query = (sort?.ToLowerInvariant()) switch
        {
            "priceasc" => query.OrderBy(p => p.price),
            "pricedesc" => query.OrderByDescending(p => p.price),
            "newest" => query.OrderByDescending(p => p.id),
            _ => query.OrderByDescending(p => p.id)  
        };
        var items = await query
            .OrderByDescending(p => p.id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto(
                p.id, p.name, p.type, p.sku, p.price, p.quantity,
                p.image_url, p.roast_level, p.processing, p.origin_country, p.origin_region
            ))
            .ToListAsync();

        return new PagedResult<ProductListItemDto>(items, total, page, pageSize);
    }

    // страница товара
    [HttpGet("/api/products/{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetOne(int id)
    {
        var p = await _db.products
            .Include(x => x.product_detail)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.id == id && !x.deleted);

        if (p is null) return NotFound();

        return new ProductDetailDto(
            p.id, p.name, p.type, p.sku, p.price, p.quantity,
            p.image_url, p.description, p.roast_level, p.processing, p.origin_country, p.origin_region,
            p.product_detail?.long_description, p.product_detail?.brewing_guide,
            p.product_detail?.flavor_notes, p.product_detail?.origin_details
        );
    }
}
