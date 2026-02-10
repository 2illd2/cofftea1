namespace CoffeeTea.Pages.Consultant.Models;

public class ConsultantStatisticsVm
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<StatusGroup> OrdersByStatus { get; set; } = new();
    public List<OrderStatsDto> RecentOrders { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}

public class StatusGroup
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderStatsDto
{
    public int id { get; set; }
    public decimal total { get; set; }
    public string Status { get; set; } = "";
    public string Client { get; set; } = "";
    public DateTime created_at { get; set; }
}

public class TopProductDto
{
    public int product_id { get; set; }
    public string product_name { get; set; } = "";
    public long total_sold { get; set; }
    public decimal revenue { get; set; }
}
