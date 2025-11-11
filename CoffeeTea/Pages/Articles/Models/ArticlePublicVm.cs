namespace CoffeeTea.Pages.Articles.Models
{
    public class ArticlePublicVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? Summary { get; set; }
        public string Content { get; set; } = "";
        public string? CoverImageUrl { get; set; }
        public string? Category { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}
