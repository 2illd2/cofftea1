using ApiCoffeeTea.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/order-statuses")]
[Authorize]
public class OrderStatusController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrderStatusController(AppDbContext db) => _db = db;

    // GET: api/order-statuses
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var statuses = await _db.order_statuses
            .Select(s => new
            {
                id = s.id,
                name = s.name
            })
            .ToListAsync();

        return Ok(statuses);
    }
}
