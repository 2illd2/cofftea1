namespace ApiCoffeeTea.DTO
{

    public record HomeCardDto(
        string Title,
        string Text,
        string ImageUrl,
        string Link,
        string? Badge = null
    );
}
