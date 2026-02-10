using CoffeeTea.Pages.Admin.Audit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CoffeeTea.Pages.Admin.Audit.Controllers;

[Authorize(Roles = "admin")]
public class AdminAuditController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminAuditController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /admin/audit
    [Route("/admin/audit")]
    public async Task<IActionResult> Index([FromQuery] string? tableName, [FromQuery] int page = 1)
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");
        var url = $"/api/admin/audit/logs?page={page}&pageSize=50";

        if (!string.IsNullOrWhiteSpace(tableName))
        {
            url += $"&tableName={Uri.EscapeDataString(tableName)}";
        }

        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Ошибка при загрузке логов аудита";
                return View("~/Pages/Admin/Audit/Views/Index.cshtml", new PagedAuditResult());
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<JsonElement>(json);

            var logs = apiResult.GetProperty("logs").Deserialize<List<AuditLogVm>>() ?? new();
            var total = apiResult.GetProperty("total").GetInt32();

            var model = new PagedAuditResult
            {
                Logs = logs,
                Total = total,
                Page = page,
                PageSize = 50
            };

            ViewBag.TableName = tableName;
            return View("~/Pages/Admin/Audit/Views/Index.cshtml", model);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка: {ex.Message}";
            return View("~/Pages/Admin/Audit/Views/Index.cshtml", new PagedAuditResult());
        }
    }

    // GET: /admin/audit/user-stats/{userId}
    [Route("/admin/audit/user-stats/{userId}")]
    public async Task<IActionResult> UserStats(int userId)
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");

        try
        {
            var response = await client.GetAsync($"/api/admin/audit/user-stats/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Пользователь не найден";
                return View("~/Pages/Admin/Audit/Views/UserStats.cshtml", new UserStatsVm());
            }

            var json = await response.Content.ReadAsStringAsync();
            var stats = JsonSerializer.Deserialize<UserStatsVm>(json);

            return View("~/Pages/Admin/Audit/Views/UserStats.cshtml", stats ?? new UserStatsVm());
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка: {ex.Message}";
            return View("~/Pages/Admin/Audit/Views/UserStats.cshtml", new UserStatsVm());
        }
    }

    // GET: /admin/audit/top-products
    [Route("/admin/audit/top-products")]
    public async Task<IActionResult> TopProducts([FromQuery] int limit = 10)
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");

        try
        {
            var response = await client.GetAsync($"/api/admin/audit/top-products?limit={limit}");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Ошибка при загрузке топ продуктов";
                return View("~/Pages/Admin/Audit/Views/TopProducts.cshtml", new List<TopProductVm>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<TopProductVm>>(json) ?? new();

            return View("~/Pages/Admin/Audit/Views/TopProducts.cshtml", products);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка: {ex.Message}";
            return View("~/Pages/Admin/Audit/Views/TopProducts.cshtml", new List<TopProductVm>());
        }
    }
}
