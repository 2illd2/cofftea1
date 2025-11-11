// ApiCoffeeTea/DTO/ArticlesDtos.cs
namespace ApiCoffeeTea.DTO;

public record ArticleListItemDto(
    int Id,
    string Title,
    string? Category,
    bool IsPublished,
    DateTime? PublishedAt
);

public class ArticleSaveDto
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Summary { get; set; }
    public string Content { get; set; } = "";
    public int? CategoryId { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
}
