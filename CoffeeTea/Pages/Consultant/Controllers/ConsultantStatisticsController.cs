using CoffeeTea.Pages.Consultant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CoffeeTea.Pages.Consultant.Controllers;

[Authorize(Roles = "consultant,admin")]
public class ConsultantStatisticsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ConsultantStatisticsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("/consultant/statistics")]
    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");
        var model = new ConsultantStatisticsVm();

        try
        {
            // Получить все заказы
            var ordersResponse = await client.GetAsync("/api/orders");
            if (ordersResponse.IsSuccessStatusCode)
            {
                var ordersJson = await ordersResponse.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<OrderStatsDto>>(ordersJson) ?? new();

                model.TotalOrders = orders.Count;
                model.TotalRevenue = orders.Sum(o => o.total);
                model.AverageOrderValue = orders.Any() ? orders.Average(o => o.total) : 0;

                // Группировка по статусам
                model.OrdersByStatus = orders
                    .GroupBy(o => o.Status)
                    .Select(g => new StatusGroup
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(o => o.total)
                    })
                    .ToList();

                // Последние заказы
                model.RecentOrders = orders
                    .OrderByDescending(o => o.created_at)
                    .Take(10)
                    .ToList();
            }

            // Получить топ товары
            var topProductsResponse = await client.GetAsync("/api/admin/top-products");
            if (topProductsResponse.IsSuccessStatusCode)
            {
                var topProductsJson = await topProductsResponse.Content.ReadAsStringAsync();
                model.TopProducts = JsonSerializer.Deserialize<List<TopProductDto>>(topProductsJson) ?? new();
            }

            return View("~/Pages/Consultant/Views/Statistics.cshtml", model);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка при загрузке статистики: {ex.Message}";
            return View("~/Pages/Consultant/Views/Statistics.cshtml", model);
        }
    }
}
