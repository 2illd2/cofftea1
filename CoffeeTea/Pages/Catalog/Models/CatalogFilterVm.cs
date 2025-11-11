namespace CoffeeTea.Pages.Catalog.Models;

public class CatalogFilterVm
{
    public int? CategoryId { get; set; }
    public string? Q { get; set; }
    public string? Type { get; set; }
    public string SortBy { get; set; } = "popularity";
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class CatalogItemVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Sku { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? RoastLevel { get; set; }
    public string? Processing { get; set; }
    public string? OriginCountry { get; set; }
    public string? OriginRegion { get; set; }
    public bool IsFavorite { get; set; }
}

public class CatalogPageVm
{
    public CatalogFilterVm Filter { get; set; } = new();
    public List<CatalogItemVm> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page => Filter.Page;
    public int PageSize => Filter.PageSize;
    public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));
}

public class ProductDetailVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Sku { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public string? RoastLevel { get; set; }
    public string? Processing { get; set; }
    public string? OriginCountry { get; set; }
    public string? OriginRegion { get; set; }
    public string? LongDescription { get; set; }
    public string? BrewingGuide { get; set; }
    public string? FlavorNotes { get; set; }
    public string? OriginDetails { get; set; }
}
