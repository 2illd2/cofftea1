using ApiCoffeeTea.Data;
using ApiCoffeeTea.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "admin")]
public class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminProductListItem>>> GetAll([FromQuery] bool includeDeleted = false)
    {
        var q = _db.products.AsNoTracking();
        if (!includeDeleted) q = q.Where(p => !p.deleted);

        var list = await q.OrderByDescending(p => p.id)
            .Select(p => new AdminProductListItem(p.id, p.name, p.type, p.sku, p.price, p.quantity, p.image_url, p.deleted))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminProductUpsertDto>> GetOne(int id)
    {
        var p = await _db.products.Include(x => x.product_detail).FirstOrDefaultAsync(x => x.id == id);
        if (p is null) return NotFound();

        return new AdminProductUpsertDto
        {
            CategoryId = p.category_id,
            Name = p.name,
            Type = p.type,
            Sku = p.sku,
            Price = p.price,
            Quantity = p.quantity,
            Description = p.description,
            ImageUrl = p.image_url,
            RoastLevel = p.roast_level,
            Processing = p.processing,
            OriginCountry = p.origin_country,
            OriginRegion = p.origin_region,
            LongDescription = p.product_detail?.long_description,
            BrewingGuide = p.product_detail?.brewing_guide,
            FlavorNotes = p.product_detail?.flavor_notes,
            OriginDetails = p.product_detail?.origin_details
        };
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(AdminProductUpsertDto dto)
    {
        var p = new product
        {
            category_id = dto.CategoryId,
            name = dto.Name.Trim(),
            type = dto.Type.Trim(),
            sku = dto.Sku.Trim(),
            price = dto.Price,
            quantity = dto.Quantity,
            description = dto.Description,
            image_url = dto.ImageUrl,
            roast_level = dto.RoastLevel,
            processing = dto.Processing,
            origin_country = dto.OriginCountry,
            origin_region = dto.OriginRegion,
            deleted = false
        };
        _db.products.Add(p);
        await _db.SaveChangesAsync();

        if (dto.LongDescription != null || dto.BrewingGuide != null ||
            dto.FlavorNotes != null || dto.OriginDetails != null)
        {
            var d = new product_detail
            {
                product_id = p.id,
                long_description = dto.LongDescription,
                brewing_guide = dto.BrewingGuide,
                flavor_notes = dto.FlavorNotes,
                origin_details = dto.OriginDetails,
                deleted = false
            };
            _db.product_details.Add(d);
            await _db.SaveChangesAsync();
        }

        return Created($"/api/admin/products/{p.id}", p.id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AdminProductUpsertDto dto)
    {
        var p = await _db.products.Include(x => x.product_detail).FirstOrDefaultAsync(x => x.id == id);
        if (p is null) return NotFound();

        p.category_id = dto.CategoryId;
        p.name = dto.Name.Trim();
        p.type = dto.Type.Trim();
        p.sku = dto.Sku.Trim();
        p.price = dto.Price;
        p.quantity = dto.Quantity;
        p.description = dto.Description;
        p.image_url = dto.ImageUrl;
        p.roast_level = dto.RoastLevel;
        p.processing = dto.Processing;
        p.origin_country = dto.OriginCountry;
        p.origin_region = dto.OriginRegion;

        if (p.product_detail is null)
        {
            if (dto.LongDescription != null || dto.BrewingGuide != null ||
                dto.FlavorNotes != null || dto.OriginDetails != null)
            {
                p.product_detail = new product_detail
                {
                    product_id = p.id,
                    long_description = dto.LongDescription,
                    brewing_guide = dto.BrewingGuide,
                    flavor_notes = dto.FlavorNotes,
                    origin_details = dto.OriginDetails,
                    deleted = false
                };
            }
        }
        else
        {
            p.product_detail.long_description = dto.LongDescription;
            p.product_detail.brewing_guide = dto.BrewingGuide;
            p.product_detail.flavor_notes = dto.FlavorNotes;
            p.product_detail.origin_details = dto.OriginDetails;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.products.FirstOrDefaultAsync(x => x.id == id);
        if (p is null) return NotFound();
        p.deleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var p = await _db.products.FirstOrDefaultAsync(x => x.id == id);
        if (p is null) return NotFound();
        p.deleted = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
