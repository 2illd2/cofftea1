using CoffeeTea.Pages.Consultant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CoffeeTea.Pages.Consultant.Controllers;

[Authorize(Roles = "consultant,admin")]
public class ConsultantController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ConsultantController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /consultant/articles
    [Route("/consultant/articles")]
    public async Task<IActionResult> Articles([FromQuery] string? q, [FromQuery] int page = 1)
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");
        var url = $"/api/articles?q={q}&page={page}&pageSize=20";

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return View("~/Pages/Consultant/Views/Index.cshtml", new PagedResult<ArticleListItemVm>());
        }

        var json = await response.Content.ReadAsStringAsync();
        var apiResult = JsonSerializer.Deserialize<JsonElement>(json);

        var items = apiResult.GetProperty("items").Deserialize<List<ArticleListItemVm>>() ?? new();
        var total = apiResult.GetProperty("total").GetInt32();

        var model = new PagedResult<ArticleListItemVm>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = 20
        };

        return View("~/Pages/Consultant/Views/Index.cshtml", model);
    }

    // GET: /consultant/articles/edit/{id?}
    [Route("/consultant/articles/edit/{id?}")]
    public async Task<IActionResult> Edit(int? id)
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");

        // Load categories
        var categoriesResponse = await client.GetAsync("/api/admin/article-categories");

        // ДОБАВЬТЕ ПРОВЕРКУ ПУСТОГО ОТВЕТА:
        if (!categoriesResponse.IsSuccessStatusCode)
        {
            // Если запрос не удался, используем пустой список
            ViewBag.Categories = new List<CategoryItemVm>();
            TempData["Error"] = "Не удалось загрузить категории";
        }
        else
        {
            var categoriesJson = await categoriesResponse.Content.ReadAsStringAsync();

            // ПРОВЕРЬТЕ, ЧТО ОТВЕТ НЕ ПУСТОЙ
            if (!string.IsNullOrWhiteSpace(categoriesJson))
            {
                try
                {
                    var categories = JsonSerializer.Deserialize<List<CategoryItemVm>>(categoriesJson) ?? new();
                    ViewBag.Categories = categories;
                }
                catch (JsonException)
                {
                    // Если JSON некорректен
                    ViewBag.Categories = new List<CategoryItemVm>();
                    TempData["Error"] = "Ошибка формата данных категорий";
                }
            }
            else
            {
                ViewBag.Categories = new List<CategoryItemVm>();
            }
        }

        // Остальная часть метода...
        if (id.HasValue)
        {
            // Edit existing article
            var response = await client.GetAsync($"/api/articles/{id.Value}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<ArticleEditVm>(json);

            if (model != null)
            {
                model.Id = id.Value;
            }

            return View("~/Pages/Consultant/Views/Edit.cshtml", model);
        }
        else
        {
            // Create new article
            return View("~/Pages/Consultant/Views/Edit.cshtml", new ArticleEditVm());
        }
    }

    // POST: /consultant/articles/save
    [HttpPost]
    [Route("/consultant/articles/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [FromForm] int? id,
        [FromForm] string title,
        [FromForm] string? slug,
        [FromForm] string? summary,
        [FromForm] string content,
        [FromForm] int? categoryId,
        [FromForm] string? coverImageUrl,
        [FromForm] bool isPublished = false)
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");

        var dto = new
        {
            Title = title,
            Slug = slug ?? "",
            Summary = summary,
            Content = content,
            CategoryId = categoryId,
            CoverImageUrl = coverImageUrl,
            IsPublished = isPublished
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(dto),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response;

        if (id.HasValue && id.Value > 0)
        {
            // Update existing
            response = await client.PutAsync($"/api/articles/{id.Value}", jsonContent);
        }
        else
        {
            // Create new
            response = await client.PostAsync("/api/articles", jsonContent);
        }

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Articles");
        }

        TempData["Error"] = "Ошибка при сохранении статьи";
        return RedirectToAction("Edit", new { id });
    }

    // POST: /consultant/articles/delete
    [HttpPost]
    [Route("/consultant/articles/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromForm] int id)
    {
        var client = _httpClientFactory.CreateClient("CoffeeTeaApi");
        var response = await client.DeleteAsync($"/api/articles/{id}");

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Статья удалена";
        }
        else
        {
            TempData["Error"] = "Ошибка при удалении статьи";
        }

        return RedirectToAction("Articles");
    }
}
