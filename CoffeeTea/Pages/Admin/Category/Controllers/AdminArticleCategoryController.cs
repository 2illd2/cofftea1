using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeTea.Pages.Admin.Category.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("admin/article-categories")]
    public class AdminArticleCategoryController : Controller
    {
        private readonly HttpClient _http;
        public AdminArticleCategoryController(IHttpClientFactory f) => _http = f.CreateClient("CoffeeTeaApi");

        public record Vm(int Id, string Name, bool Deleted);
        public record SaveDto(string Name);

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var items = await _http.GetFromJsonAsync<List<Vm>>("/api/admin/article-categories") ?? new();
            return View("~/Pages/Admin/Category/Views/Index.cshtml", items);
        }

        [HttpGet("edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            Vm? model = null;
            if (id is not null)
                model = await _http.GetFromJsonAsync<Vm>($"/api/admin/article-categories/{id}");
            return View("~/Pages/Admin/Category/Views/Edit.cshtml", model ?? new Vm(0, "", false));
        }

        [HttpPost("save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromForm] int id, [FromForm] string name)
        {
            HttpResponseMessage resp = id == 0
                ? await _http.PostAsJsonAsync("/api/admin/article-categories", new SaveDto(name))
                : await _http.PutAsJsonAsync($"/api/admin/article-categories/{id}", new SaveDto(name));

            TempData[resp.IsSuccessStatusCode ? "Ok" : "Error"] =
                resp.IsSuccessStatusCode ? "Сохранено" : await resp.Content.ReadAsStringAsync();

            return RedirectToAction("Index");
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] int id)
        {
            var resp = await _http.DeleteAsync($"/api/admin/article-categories/{id}");
            TempData[resp.IsSuccessStatusCode ? "Ok" : "Error"] = resp.IsSuccessStatusCode ? "Удалено" : "Ошибка удаления";
            return RedirectToAction("Index");
        }

        [HttpPost("restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore([FromForm] int id)
        {
            var resp = await _http.PostAsync($"/api/admin/article-categories/{id}/restore", null);
            TempData[resp.IsSuccessStatusCode ? "Ok" : "Error"] = resp.IsSuccessStatusCode ? "Восстановлено" : "Ошибка";
            return RedirectToAction("Index");
        }
    }
}
