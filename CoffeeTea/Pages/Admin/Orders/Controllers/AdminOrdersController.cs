using CoffeeTea.Pages.Admin.Orders.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CoffeeTea.Pages.Admin.Orders.Controllers;

[Authorize(Roles = "admin")]
public class AdminOrdersController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminOrdersController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /admin/orders
    [HttpGet("/admin/orders")]
    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");

        try
        {
            var response = await client.GetAsync("/api/orders");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Ошибка при загрузке заказов";
                return View("~/Pages/Admin/Orders/Views/Index.cshtml", new List<AdminOrderListItemVm>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var orders = JsonSerializer.Deserialize<List<AdminOrderListItemVm>>(json) ?? new();

            return View("~/Pages/Admin/Orders/Views/Index.cshtml", orders);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка: {ex.Message}";
            return View("~/Pages/Admin/Orders/Views/Index.cshtml", new List<AdminOrderListItemVm>());
        }
    }

    // GET: /admin/orders/{id}
    [HttpGet("/admin/orders/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");

        try
        {
            var response = await client.GetAsync($"/api/orders/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("Заказ не найден");
                }
                ViewBag.Error = "Ошибка при загрузке деталей заказа";
                return View("~/Pages/Admin/Orders/Views/Details.cshtml", new AdminOrderDetailVm());
            }

            var json = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<AdminOrderDetailVm>(json);

            if (order == null)
            {
                return NotFound("Заказ не найден");
            }

            // Загрузить список статусов для выпадающего списка
            var statusResponse = await client.GetAsync("/api/order-statuses");
            if (statusResponse.IsSuccessStatusCode)
            {
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                var statuses = JsonSerializer.Deserialize<List<OrderStatusVm>>(statusJson) ?? new();
                ViewBag.OrderStatuses = statuses;
            }

            return View("~/Pages/Admin/Orders/Views/Details.cshtml", order);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка: {ex.Message}";
            return View("~/Pages/Admin/Orders/Views/Details.cshtml", new AdminOrderDetailVm());
        }
    }
}
