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

    // Изменить статус
    [HttpPut("{id:int}/status/{statusId:int}")]
    [Authorize(Roles = "Admin,Consultant")]
    public async Task<IActionResult> ChangeStatus(int id, int statusId)
    {
        var order = await _db.orders
    .Include(o => o.status)           
    .FirstOrDefaultAsync(o => o.id == id);

        var st = await _db.order_statuses.FindAsync(statusId);
        order.status_id = statusId;
        await _db.SaveChangesAsync();

        return Ok(new { order.id, NewStatus = st!.name });
    }
}
