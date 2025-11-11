using CoffeeTea.Pages.Cart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeTea.Pages.Cart.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly HttpClient _http;
    public CartController(IHttpClientFactory factory) => _http = factory.CreateClient("CoffeeTeaApi");

    [HttpGet("/cart")]
    public async Task<IActionResult> Index()
    {
        var dto = await _http.GetFromJsonAsync<ApiCartDto>("/api/cart");
        var vm = new CartVm
        {
            Items = dto?.Items.Select(i => new CartItemVm
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Name = i.Name,
                ImageUrl = i.ImageUrl,
                Price = i.Price,
                Qty = i.Qty
            }).ToList() ?? new(),
            Total = dto?.Total ?? 0,
            Count = dto?.Count ?? 0
        };
        return View("~/Pages/Cart/Views/Index.cshtml", vm);
    }

    [HttpPost("/cart/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToCartVm vm, string? returnUrl = null)
    {
        await _http.PostAsJsonAsync("/api/cart/items", new { productId = vm.ProductId, qty = vm.Qty });
        return Redirect(returnUrl ?? "/cart");
    }

    [HttpPost("/cart/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int qty)
    {
        await _http.PutAsJsonAsync($"/api/cart/items/{id}", new { qty });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/cart/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        await _http.DeleteAsync($"/api/cart/items/{id}");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/cart/clear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _http.DeleteAsync("/api/cart/clear");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/cart/checkout")]
    public async Task<IActionResult> Checkout()
    {
        var addresses = await _http.GetFromJsonAsync<List<AddressVm>>("/api/addresses/mine") ?? new();
        var vm = new CheckoutVm { Addresses = addresses, SelectedAddressId = addresses.FirstOrDefault(a => a.IsDefault)?.Id ?? 0 };
        return View("~/Pages/Cart/Views/Checkout.cshtml", vm);
    }

    [HttpPost("/cart/checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutVm vm)
    {
        object payload = vm.SelectedAddressId > 0
            ? new { AddressId = vm.SelectedAddressId }
            : new
            {
                Address = new
                {
                    line1 = vm.NewAddress.Line1,
                    city = vm.NewAddress.City,
                    postalCode = vm.NewAddress.PostalCode,
                    country = vm.NewAddress.Country,
                    isDefault = vm.NewAddress.IsDefault
                }
            };

        var resp = await _http.PostAsJsonAsync("/api/cart/checkout", payload);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            TempData["CartError"] = $"Не удалось оформить заказ: {(int)resp.StatusCode} {resp.ReasonPhrase}. Ответ: {body}";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Success");
    }
    [HttpGet("/cart/success")]
    public IActionResult Success()
    {
        return View("~/Pages/Cart/Views/Success.cshtml");
    }

    private record ApiCartDto(IReadOnlyList<ApiCartItemDto> Items, decimal Total, int Count);
    private record ApiCartItemDto(int Id, int ProductId, string Name, string? ImageUrl, decimal Price, int Qty, decimal Subtotal);
}
