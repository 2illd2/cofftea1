using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CoffeeTea.Pages.Orders.Models;

namespace CoffeeTea.Pages.Orders.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OrdersController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /orders - Доступно всем авторизованным пользователям
    // User видит только свои заказы, Admin/Consultant видят все
    [HttpGet("/orders")]
    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");

        try
        {
            var response = await client.GetAsync("/api/orders");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Ошибка при загрузке заказов";
                return View("~/Pages/Orders/Views/Index.cshtml", new List<OrderListItemVm>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var orders = JsonSerializer.Deserialize<List<OrderListItemVm>>(json) ?? new();

            // Определяем, какой view использовать в зависимости от роли
            var role = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            var viewPath = role switch
            {
                "admin" => "~/Pages/Orders/Views/AdminIndex.cshtml",
                "consultant" => "~/Pages/Orders/Views/ConsultantIndex.cshtml",
                _ => "~/Pages/Orders/Views/UserIndex.cshtml"
            };

            return View(viewPath, orders);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка: {ex.Message}";
            return View("~/Pages/Orders/Views/Index.cshtml", new List<OrderListItemVm>());
        }
    }

    // GET: /orders/{id} - Детали заказа
    [HttpGet("/orders/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");

        try
        {
            var response = await client.GetAsync($"/api/orders/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return View("~/Pages/Shared/AccessDenied.cshtml");
            }

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("Заказ не найден");
                }
                ViewBag.Error = "Ошибка при загрузке деталей заказа";
                return View("~/Pages/Orders/Views/Details.cshtml", new OrderDetailVm());
            }

            var json = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<OrderDetailVm>(json);

            if (order == null)
            {
                return NotFound("Заказ не найден");
            }

            // Загрузить список статусов для admin/consultant
            var role = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
            if (role == "admin" || role == "consultant")
            {
                var statusResponse = await client.GetAsync("/api/order-statuses");
                if (statusResponse.IsSuccessStatusCode)
                {
                    var statusJson = await statusResponse.Content.ReadAsStringAsync();
                    var statuses = JsonSerializer.Deserialize<List<OrderStatusVm>>(statusJson) ?? new();
                    ViewBag.OrderStatuses = statuses;
                    ViewBag.CanManageStatus = true;
                }
            }
            else
            {
                ViewBag.CanManageStatus = false;
            }

            return View("~/Pages/Orders/Views/Details.cshtml", order);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Ошибка: {ex.Message}";
            return View("~/Pages/Orders/Views/Details.cshtml", new OrderDetailVm());
        }
    }

    //// Alias routes для обратной совместимости
    //[HttpGet("/admin/orders")]
    //public Task<IActionResult> AdminIndex() => Index();

    //[HttpGet("/admin/orders/{id:int}")]
    //public Task<IActionResult> AdminDetails(int id) => Details(id);

    //[HttpGet("/consultant/orders")]
    //public Task<IActionResult> ConsultantIndex() => Index();

    //[HttpGet("/consultant/orders/{id:int}")]
    //public Task<IActionResult> ConsultantDetails(int id) => Details(id);
}
