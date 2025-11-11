// CoffeeTea/Pages/Admin/Products/Models/AdminProductListItemVm.cs
namespace CoffeeTea.Pages.Admin.Products.Models;

public record AdminProductListItemVm(
    int Id,
    string Name,
    string Type,
    string Sku,
    decimal Price,
    int Quantity,
    string? ImageUrl,
    bool Deleted
);
