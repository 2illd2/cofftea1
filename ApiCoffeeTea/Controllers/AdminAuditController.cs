using ApiCoffeeTea.Data;
using ApiCoffeeTea.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/admin/audit")]
[Authorize(Roles = "admin")]
public class AdminAuditController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public AdminAuditController(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    // Получить логи аудита с пагинацией
    [HttpGet("logs")]
    public async Task<ActionResult<object>> GetAuditLogs(
        [FromQuery] string? tableName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _db.audit_logs.Include(a => a.user).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tableName))
        {
            query = query.Where(a => a.table_name == tableName);
        }

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.changed_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto(
                a.id,
                a.table_name,
                a.operation,
                a.row_id,
                a.user_id,
                a.user != null ? $"{a.user.first_name} {a.user.last_name}" : null,
                a.old_values,
                a.new_values,
                a.changed_at
            ))
            .ToListAsync();

        return Ok(new { logs, total, page, pageSize });
    }

    // Получить статистику пользователя
    [HttpGet("user-stats/{userId}")]
    public async Task<ActionResult<UserStatsDto>> GetUserStats(int userId)
    {
        var connString = _cfg.GetConnectionString("DefaultConnection");
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM sp_GetUserStats(@p_user_id)", conn);
        cmd.Parameters.AddWithValue("p_user_id", userId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var stats = new UserStatsDto(
                reader.GetString(0),                    // user_name
                reader.GetInt64(1),                     // total_orders
                reader.GetDecimal(2),                   // total_spent
                reader.GetDecimal(3),                   // avg_order_value
                reader.GetInt64(4),                     // reviews_count
                reader.IsDBNull(5) ? null : reader.GetDateTime(5),  // last_order_date
                reader.GetInt64(6),                     // cart_items_count
                reader.GetDateTime(7)                   // registration_date
            );

            return Ok(stats);
        }

        return NotFound("Пользователь не найден");
    }

    // Получить топ товаров
    [HttpGet("top-products")]
    public async Task<ActionResult<List<TopProductDto>>> GetTopProducts([FromQuery] int limit = 10)
    {
        var connString = _cfg.GetConnectionString("DefaultConnection");
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT * FROM sp_GetTopProducts(@p_limit)", conn);
        cmd.Parameters.AddWithValue("p_limit", limit);

        var products = new List<TopProductDto>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new TopProductDto(
                reader.GetInt32(0),     // product_id
                reader.GetString(1),    // product_name
                reader.GetInt64(2),     // total_sold
                reader.GetDecimal(3)    // revenue
            ));
        }

        return Ok(products);
    }

    // Очистка старых корзин
    [HttpPost("clean-old-carts")]
    public async Task<ActionResult<object>> CleanOldCarts([FromQuery] int daysOld = 30)
    {
        var connString = _cfg.GetConnectionString("DefaultConnection");
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT sp_CleanOldCarts(@p_days_old)", conn);
        cmd.Parameters.AddWithValue("p_days_old", daysOld);

        var deletedCount = (int?)await cmd.ExecuteScalarAsync() ?? 0;

        return Ok(new { deletedCount, message = $"Удалено {deletedCount} корзин(ы)" });
    }
}
