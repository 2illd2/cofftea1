namespace CoffeeTea.Pages.Orders.Models;

public class OrderListItemVm
{
    public int id { get; set; }
    public decimal total { get; set; }
    public string Status { get; set; } = "";
    public string Client { get; set; } = "";
    public DateTime created_at { get; set; }
}

public class OrderDetailVm
{
    public int id { get; set; }
    public decimal total { get; set; }
    public string status { get; set; } = "";
    public int statusId { get; set; }
    public string client { get; set; } = "";
    public string clientEmail { get; set; } = "";
    public string address { get; set; } = "";
    public string paymentStatus { get; set; } = "";
    public DateTime createdAt { get; set; }
    public List<OrderItemVm> items { get; set; } = new();
}

public class OrderItemVm
{
    public int productId { get; set; }
    public string productName { get; set; } = "";
    public int qty { get; set; }
    public decimal unitPrice { get; set; }
}

public class OrderStatusVm
{
    public int id { get; set; }
    public string name { get; set; } = "";
}
