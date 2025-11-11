namespace ApiCoffeeTea.DTO;

public record ProductListItemDto(
    int Id,
    string Name,
    string Type,
    string Sku,
    decimal Price,
    int Quantity,
    string? ImageUrl,
    string? RoastLevel,
    string? Processing,
    string? OriginCountry,
    string? OriginRegion
);

public record ProductDetailDto(
    int Id,
    string Name,
    string Type,
    string Sku,
    decimal Price,
    int Quantity,
    string? ImageUrl,
    string? Description,
    string? RoastLevel,
    string? Processing,
    string? OriginCountry,
    string? OriginRegion,
    string? LongDescription,
    string? BrewingGuide,
    string? FlavorNotes,
    string? OriginDetails
);

public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
