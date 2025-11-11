namespace ApiCoffeeTea.DTO;

public record CartItemDto(
    int Id,             
    int ProductId,
    string Name,
    string? ImageUrl,
    decimal Price,
    int Qty,
    decimal Subtotal
);

public record CartDto(
    IReadOnlyList<CartItemDto> Items,
    decimal Total,
    int Count
);

public record AddCartItemDto(int ProductId, int Qty);
public record UpdateCartItemDto(int Qty);


public class CheckoutDto
{
    public int? AddressId { get; set; }
    public AddressCreateDto? Address { get; set; }
}
