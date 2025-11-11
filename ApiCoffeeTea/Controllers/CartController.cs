using ApiCoffeeTea.Data;
using ApiCoffeeTea.DTO;
using ApiCoffeeTea.Utils; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) => _db = db;

    private static DateTime NowDb() =>
        DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetMyCart()
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var items = await _db.carts
            .AsNoTracking()
            .Include(c => c.product)
            .Where(c => c.user_id == uid && !c.deleted)
            .OrderByDescending(c => c.id)
            .Select(c => new CartItemDto(
                c.id,
                c.product_id,
                c.product.name,
                c.product.image_url,
                c.price,
                c.quantity,
                c.price * c.quantity
            ))
            .ToListAsync();

        var dto = new CartDto(items, items.Sum(i => i.Subtotal), items.Sum(i => i.Qty));
        return dto;
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemDto dto)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();
        if (dto.Qty <= 0) return BadRequest("Количество должно быть > 0.");

        var p = await _db.products.FirstOrDefaultAsync(x => x.id == dto.ProductId && !x.deleted);
        if (p is null) return NotFound("Товар не найден.");

        var requested = Math.Min(dto.Qty, p.quantity);

        var item = await _db.carts.FirstOrDefaultAsync(c =>
            c.user_id == uid && c.product_id == p.id && !c.deleted);

        if (item is null)
        {
            item = new cart
            {
                user_id = uid.Value,
                product_id = p.id,
                quantity = requested,
                price = p.price,
                added_at = NowDb(),
                deleted = false
            };
            _db.carts.Add(item);
        }
        else
        {
            item.quantity = Math.Min(item.quantity + requested, p.quantity);
            item.price = p.price;
        }

        await _db.SaveChangesAsync();
        return await GetMyCart();
    }

    [HttpPut("items/{id:int}")]
    public async Task<ActionResult<CartDto>> UpdateItem(int id, UpdateCartItemDto dto)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();
        if (dto.Qty <= 0) return await RemoveItem(id);

        var item = await _db.carts
            .Include(c => c.product)
            .FirstOrDefaultAsync(c => c.id == id && c.user_id == uid && !c.deleted);

        if (item is null) return NotFound();

        item.quantity = Math.Min(dto.Qty, item.product.quantity);
        item.price = item.product.price;
        await _db.SaveChangesAsync();

        return await GetMyCart();
    }

    [HttpDelete("items/{id:int}")]
    public async Task<ActionResult<CartDto>> RemoveItem(int id)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var item = await _db.carts.FirstOrDefaultAsync(c => c.id == id && c.user_id == uid && !c.deleted);
        if (item is null) return NotFound();

        item.deleted = true;
        await _db.SaveChangesAsync();

        return await GetMyCart();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var items = await _db.carts.Where(c => c.user_id == uid && !c.deleted).ToListAsync();
        foreach (var it in items) it.deleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
    {
        var uid = User.GetUserId();
        if (uid is null) return Unauthorized();

        var cartItems = await _db.carts
            .Include(c => c.product)
            .Where(c => c.user_id == uid && !c.deleted)
            .ToListAsync();

        if (!cartItems.Any())
            return BadRequest("Корзина пуста.");

        if (dto.AddressId is null && dto.Address is null)
            return BadRequest("Address is required.");

        int addressId;
        if (dto.AddressId is int existingId)
        {
            var ok = await _db.addresses.AnyAsync(a => a.id == existingId && a.user_id == uid && !a.deleted);
            if (!ok) return BadRequest("Invalid address.");
            addressId = existingId;
        }
        else
        {
            if (dto.Address!.IsDefault)
            {
                await _db.addresses
                    .Where(a => a.user_id == uid && a.is_default && !a.deleted)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.is_default, false));
            }

            var a = new address
            {
                user_id = uid.Value,
                line1 = dto.Address!.Line1.Trim(),
                city = dto.Address!.City.Trim(),
                postal_code = dto.Address!.PostalCode.Trim(),
                country = dto.Address!.Country.Trim(),
                is_default = dto.Address!.IsDefault,
                deleted = false
            };
            _db.addresses.Add(a);
            await _db.SaveChangesAsync();
            addressId = a.id;
        }

        decimal total = 0;
        foreach (var it in cartItems)
        {
            if (it.product.deleted)
                return BadRequest($"Товар '{it.product.name}' недоступен.");

            var allowed = Math.Min(it.quantity, it.product.quantity);
            if (allowed <= 0)
                return BadRequest($"Товар '{it.product.name}' закончился.");

            it.quantity = allowed;
            it.price = it.product.price;
            total += it.price * it.quantity;
        }

        var status = await _db.order_statuses.OrderBy(s => s.id).FirstOrDefaultAsync();
        var statusId = status?.id ?? 1;

        using var tx = await _db.Database.BeginTransactionAsync();

        var order = new order
        {
            user_id = uid.Value,
            address_id = addressId,
            status_id = statusId,
            total = total,
            payment_status = "pending",
            created_at = NowDb(),
            deleted = false
        };
        _db.orders.Add(order);
        await _db.SaveChangesAsync();

        foreach (var it in cartItems)
        {
            _db.order_items.Add(new order_item
            {
                order_id = order.id,
                product_id = it.product_id,
                qty = it.quantity,
                unit_price = it.price
            });

            // списываем остаток
            it.product.quantity = Math.Max(0, it.product.quantity - it.quantity);

            // чистим корзину
            it.deleted = true;
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Created($"/api/orders/{order.id}", new { order.id, order.address_id, order.total });
    }
}
