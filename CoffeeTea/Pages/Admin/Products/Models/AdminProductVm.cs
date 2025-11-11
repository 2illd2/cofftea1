namespace CoffeeTea.Pages.Admin.Products.Models;

public class AdminProductVm
{
    public int? Id { get; set; }
    public int? CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "coffee";
    public string Sku { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? RoastLevel { get; set; }
    public string? Processing { get; set; }
    public string? OriginCountry { get; set; }
    public string? OriginRegion { get; set; }
    public string? LongDescription { get; set; }
    public string? BrewingGuide { get; set; }
    public string? FlavorNotes { get; set; }
    public string? OriginDetails { get; set; }
    public bool Deleted { get; set; }
}
