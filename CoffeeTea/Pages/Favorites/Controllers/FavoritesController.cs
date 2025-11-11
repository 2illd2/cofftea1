using CoffeeTea.Pages.Favorites.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace CoffeeTea.Pages.Favorites.Controllers
{
    [Route("favorites")]
    public class FavoritesController : Controller
    {
        private const string SessionKey = "Favorites";

        [HttpGet("")]
        public IActionResult Index()
        {
            var favorites = GetFavorites();
            var vm = new FavoritesVm
            {
                Items = favorites.Select(f => new FavoriteItemVm
                {
                    Id = f.Id,
                    Name = f.Name,
                    Type = f.Type,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl
                }).ToList()
            };
            return View("~/Pages/Favorites/Views/Index.cshtml", vm);
        }

        [HttpPost("toggle")]
        public IActionResult Toggle([FromForm] int id, [FromForm] string name, [FromForm] string type, [FromForm] string price, [FromForm] string imageUrl)
        {
            var favorites = GetFavorites();
            var existing = favorites.FirstOrDefault(f => f.Id == id);

            bool isFavorite;
            if (existing != null)
            {
                favorites.Remove(existing);
                isFavorite = false;
            }
            else
            {
                decimal priceValue = 0m;
                decimal.TryParse(price, NumberStyles.Number, CultureInfo.InvariantCulture, out priceValue);

                favorites.Add(new FavoriteItemVm
                {
                    Id = id,
                    Name = name,
                    Type = type,
                    Price = priceValue,
                    ImageUrl = imageUrl
                });
                isFavorite = true;

            }

            SaveFavorites(favorites);

            return Json(new
            {
                success = true,
                isFavorite,
                count = favorites.Count
            });
        }

        // Удалить один
        [HttpPost("remove")]
        public IActionResult Remove(int id)
        {
            var favorites = GetFavorites();
            favorites.RemoveAll(f => f.Id == id);
            SaveFavorites(favorites);
            return RedirectToAction("Index");
        }

        // Очистить всё
        [HttpPost("clear")]
        public IActionResult Clear()
        {
            SaveFavorites(new List<FavoriteItemVm>());
            return RedirectToAction("Index");
        }

        private List<FavoriteItemVm> GetFavorites()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            return json != null
                ? JsonSerializer.Deserialize<List<FavoriteItemVm>>(json)!
                : new List<FavoriteItemVm>();
        }

        private void SaveFavorites(List<FavoriteItemVm> items)
        {
            var json = JsonSerializer.Serialize(items);
            HttpContext.Session.SetString(SessionKey, json);
        }
    }
}
