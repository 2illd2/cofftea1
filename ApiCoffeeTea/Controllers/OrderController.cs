using ApiCoffeeTea.Data;
using ApiCoffeeTea.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrderController(AppDbContext db) => _db = db;

    // Получить все заказы
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var uid = User.GetUserId();
        var role = User.FindFirst("Role")?.Value;

        if (role == "customer")
        {
            var myOrders = await _db.orders
                .Include(o => o.status)
                .Include(o => o.address)
                .Where(o => o.user_id == uid)
                .Select(o => new
                {
                    o.id,
                    o.total,
                    Status = o.status.name,
                    Address = o.address.line1,
                    o.created_at
                })
                .ToListAsync();

            return Ok(myOrders);
        }

        // Консультант или админ — видит все заказы
        var allOrders = await _db.orders
     .Include(o => o.status)            
     .Include(o => o.user)
     .Select(o => new
     {
         o.id,
         o.total,
         Status = o.status.name,       
         Client = o.user.first_name + " " + o.user.last_name,
         o.created_at
     })
     .ToListAsync();


        return Ok(allOrders);
    }

    // Получить детали заказа
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var uid = User.GetUserId();
        var role = User.FindFirst("Role")?.Value;

        var order = await _db.orders
            .Include(o => o.status)
            .Include(o => o.user)
            .Include(o => o.address)
            .Include(o => o.order_items)
                .ThenInclude(oi => oi.product)
            .Where(o => o.id == id && !o.deleted)
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound();

        // Обычные пользователи могут видеть только свои заказы
        if (role == "user" && order.user_id != uid)
            return Forbid();

        var result = new
        {
            id = order.id,
            total = order.total,
            status = order.status.name,
            statusId = order.status_id,
            client = order.user.first_name + " " + order.user.last_name,
            clientEmail = order.user.email,
            address = $"{order.address.line1}, {order.address.city}, {order.address.postal_code}",
            paymentStatus = order.payment_status,
            createdAt = order.created_at,
            items = order.order_items.Select(oi => new
            {
                productId = oi.product_id,
                productName = oi.product.name,
                qty = oi.qty,
                unitPrice = oi.unit_price
            }).ToList()
        };

        return Ok(result);
    }

    // Изменить статус
    [HttpPut("{id:int}/status/{statusId:int}")]
    [Authorize(Roles = "admin,consultant")]
    public async Task<IActionResult> ChangeStatus(int id, int statusId)
    {
        var order = await _db.orders
            .Include(o => o.status)
            .FirstOrDefaultAsync(o => o.id == id);

        if (order == null)
            return NotFound();

        var st = await _db.order_statuses.FindAsync(statusId);
        if (st == null)
            return BadRequest("Неверный статус");

        order.status_id = statusId;
        await _db.SaveChangesAsync();

        return Ok(new { order.id, NewStatus = st.name });
    }
}
