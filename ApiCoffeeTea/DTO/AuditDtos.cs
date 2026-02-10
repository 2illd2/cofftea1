namespace ApiCoffeeTea.DTO;

public record AuditLogDto(
    int Id,
    string TableName,
    string Operation,
    int RowId,
    int? UserId,
    string? UserName,
    string? OldValues,
    string? NewValues,
    DateTime ChangedAt
);

public record UserStatsDto(
    string UserName,
    long TotalOrders,
    decimal TotalSpent,
    decimal AvgOrderValue,
    long ReviewsCount,
    DateTime? LastOrderDate,
    long CartItemsCount,
    DateTime RegistrationDate
);

public record BackupDto(
    string BackupName,
    DateTime? CreatedAt
);

public record TopProductDto(
    int ProductId,
    string ProductName,
    long TotalSold,
    decimal Revenue
);
