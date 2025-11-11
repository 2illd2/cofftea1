namespace CoffeeTea.Pages.Favorites.Models
{

    public class FavoritesVm
    {
        public List<FavoriteItemVm> Items { get; set; } = new();
        public int TotalCount => Items.Count;
    }


    public class FavoriteItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }
}
