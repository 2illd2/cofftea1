using System.Net.Http.Json;
using CoffeeTea.Pages.Admin.Products.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeTea.Pages.Admin.Products.Controllers;

[Authorize(Roles = "admin")]
public class AdminProductsController : Controller
{
    private readonly HttpClient _http;
    public AdminProductsController(IHttpClientFactory factory) => _http = factory.CreateClient("CoffeeTeaApi");

    [HttpGet("/admin/products")]
    public async Task<IActionResult> Index(bool includeDeleted = true)
    {
        var list = await _http.GetFromJsonAsync<List<AdminProductListItemVm>>(
            $"/api/admin/products?includeDeleted={includeDeleted}") ?? new();

        ViewBag.IncludeDeleted = includeDeleted; // если нужно отрисовать чекбокс
        return View("~/Pages/Admin/Products/Views/Index.cshtml", list);
    }

    [HttpGet("/admin/products/create")]
    public IActionResult Create() =>
        View("~/Pages/Admin/Products/Views/Edit.cshtml", new AdminProductVm());

    [HttpPost("/admin/products/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProductVm vm)
    {
        var ok = await PostUpsert(null, vm);
        if (!ok) return View("~/Pages/Admin/Products/Views/Edit.cshtml", vm);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/products/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _http.GetFromJsonAsync<AdminProductVm>($"/api/admin/products/{id}");
        if (dto == null) return NotFound();
        dto.Id = id;
        return View("~/Pages/Admin/Products/Views/Edit.cshtml", dto);
    }

    [HttpPost("/admin/products/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminProductVm vm)
    {
        var ok = await PostUpsert(id, vm);
        if (!ok) return View("~/Pages/Admin/Products/Views/Edit.cshtml", vm);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/products/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _http.DeleteAsync($"/api/admin/products/{id}");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/products/{id:int}/restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        await _http.PostAsync($"/api/admin/products/{id}/restore", null);
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> PostUpsert(int? id, AdminProductVm vm)
    {
        var payload = new { vm.CategoryId, vm.Name, vm.Type, vm.Sku, vm.Price, vm.Quantity, vm.Description, vm.ImageUrl, vm.RoastLevel, vm.Processing, vm.OriginCountry, vm.OriginRegion, vm.LongDescription, vm.BrewingGuide, vm.FlavorNotes, vm.OriginDetails };

        var resp = id is null
            ? await _http.PostAsJsonAsync("/api/admin/products", payload)
            : await _http.PutAsJsonAsync($"/api/admin/products/{id}", payload);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Ошибка сохранения ({(int)resp.StatusCode}). {body}");
            return false;
        }
        return true;
    }
}
