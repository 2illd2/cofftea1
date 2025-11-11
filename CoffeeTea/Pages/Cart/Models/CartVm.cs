namespace CoffeeTea.Pages.Cart.Models;

public class CartVm
{
    public List<CartItemVm> Items { get; set; } = new();
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public class CartItemVm
{
    public int Id { get; set; }            
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Qty { get; set; }
    public decimal Subtotal => Price * Qty;
}

public class AddToCartVm
{
    public int ProductId { get; set; }
    public int Qty { get; set; } = 1;
}
