namespace CoffeeTea.Pages.Consultant.Models;

public class ArticleListItemVm
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Category { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class ArticleEditVm
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Summary { get; set; }
    public string Content { get; set; } = "";
    public int? CategoryId { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
}

public class CategoryItemVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
