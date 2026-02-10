namespace CoffeeTea.Pages.Admin.Audit.Models;

public class AuditLogVm
{
    public int Id { get; set; }
    public string TableName { get; set; } = "";
    public string Operation { get; set; } = "";
    public int RowId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class UserStatsVm
{
    public string UserName { get; set; } = "";
    public long TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AvgOrderValue { get; set; }
    public long ReviewsCount { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public long CartItemsCount { get; set; }
    public DateTime RegistrationDate { get; set; }
}

public class TopProductVm
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public long TotalSold { get; set; }
    public decimal Revenue { get; set; }
}

public class PagedAuditResult
{
    public List<AuditLogVm> Logs { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
