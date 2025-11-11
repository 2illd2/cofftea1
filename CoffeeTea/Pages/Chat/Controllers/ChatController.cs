using CoffeeTea.Pages.Chat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeTea.Pages.Chat.Controllers
{
    [Authorize]
    [Route("chat")]
    public class ChatController : Controller
    {
        private readonly HttpClient _http;
        public ChatController(IHttpClientFactory f) => _http = f.CreateClient("CoffeeTeaApi");

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ChatThreadVm vm = new();
            try
            {
                var resp = await _http.GetAsync("/api/chat/thread");
                if (resp.IsSuccessStatusCode)
                    vm = (await resp.Content.ReadFromJsonAsync<ChatThreadVm>()) ?? new();
                else
                    TempData["ChatError"] = $"API error: {(int)resp.StatusCode}";
            }
            catch (Exception ex)
            {
                TempData["ChatError"] = "Не удалось открыть чат.";
            }
            return View("~/Pages/Chat/Views/Index.cshtml", vm);
        }

        public record SendMessageDto(int ThreadId, string Text);

        [HttpPost("send")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send([FromForm] int threadId, [FromForm] string text)
        {
            var resp = await _http.PostAsJsonAsync("/api/chat/send",
                new SendMessageDto(threadId, text));

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["ChatError"] = $"Не удалось отправить сообщение: {(int)resp.StatusCode} {(string.IsNullOrWhiteSpace(body) ? "" : $"— {body}")}";
            }
            return RedirectToAction("Index");
        }
    }
}
