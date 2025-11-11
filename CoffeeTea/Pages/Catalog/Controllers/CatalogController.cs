using CoffeeTea.Pages.Catalog.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeTea.Pages.Catalog.Controllers;

public class CatalogController : Controller
{
    private readonly HttpClient _http;
    public CatalogController(IHttpClientFactory factory) => _http = factory.CreateClient("CoffeeTeaApi");


    private record PagedResultDto<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
    private record ListItemDto(
        int Id, string Name, string Type, string Sku, decimal Price, int Quantity,
        string? ImageUrl, string? RoastLevel, string? Processing, string? OriginCountry, string? OriginRegion
    );

    [HttpGet("/catalog")]
    public async Task<IActionResult> Index([FromQuery] CatalogFilterVm filter)
    {
        var url =
            $"/api/catalog?categoryId={filter.CategoryId}" +
            $"&q={Uri.EscapeDataString(filter.Q ?? string.Empty)}" +
            $"&type={Uri.EscapeDataString(filter.Type ?? string.Empty)}" +
            $"&minPrice={filter.MinPrice}" +
            $"&maxPrice={filter.MaxPrice}" +
            $"&sort={Uri.EscapeDataString(filter.SortBy ?? string.Empty)}" +
            $"&page={filter.Page}&pageSize={filter.PageSize}";

        var resp = await _http.GetFromJsonAsync<PagedResultDto<ListItemDto>>(url);

        var vm = new CatalogPageVm
        {
            Filter = filter,
            Items = resp?.Items?.Select(x => new CatalogItemVm
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Sku = x.Sku,
                Price = x.Price,
                Quantity = x.Quantity,
                ImageUrl = x.ImageUrl,
                RoastLevel = x.RoastLevel,
                Processing = x.Processing,
                OriginCountry = x.OriginCountry,
                OriginRegion = x.OriginRegion
            }).ToList() ?? new(),
            Total = resp?.Total ?? 0
        };

        return View("~/Pages/Catalog/Views/Index.cshtml", vm);
    }

    [HttpGet("/catalog/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var p = await _http.GetFromJsonAsync<ProductDetailVm>($"/api/products/{id}");
        if (p is null) return NotFound();
        return View("~/Pages/Catalog/Views/Details.cshtml", p);
    }
}
