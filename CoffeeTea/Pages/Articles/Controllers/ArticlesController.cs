using CoffeeTea.Pages.Articles.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeTea.Pages.Articles.Controllers
{
    [Route("articles")]
    public class ArticlesController : Controller
    {
        private readonly HttpClient _http;
        public ArticlesController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("CoffeeTeaApi");
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] string? q = null, [FromQuery] int page = 1)
        {
            var url = $"/api/articles/public?q={Uri.EscapeDataString(q ?? "")}&page={page}&pageSize=12";
            PagedResult<ArticlePublicVm>? result = null;

            try
            {
                result = await _http.GetFromJsonAsync<PagedResult<ArticlePublicVm>>(url);
            }
            catch (HttpRequestException) { }

            result ??= new PagedResult<ArticlePublicVm> { Items = new(), Page = page, PageSize = 12 };
            return View("~/Pages/Articles/Views/Index.cshtml", result);
        }


        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var article = await _http.GetFromJsonAsync<ArticlePublicVm>($"/api/articles/by-slug/{slug}");
            if (article == null) return NotFound();

            return View("~/Pages/Articles/Views/Details.cshtml", article);
        }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }
}
