using ApiCoffeeTea.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoffeeTea.Pages.Home.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _http;
        public HomeController(IHttpClientFactory factory)
            => _http = factory.CreateClient("CoffeeTeaApi");

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var token = HttpContext.Request.Cookies["auth_token"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        _http.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }
                }

                var vm = await _http.GetFromJsonAsync<List<HomeCardDto>>("/api/home/cards/by-categories") ?? new List<HomeCardDto>();

                if (User.Identity.IsAuthenticated)
                {
                    ViewData["UserName"] = User.Identity.Name;
                    ViewData["UserEmail"] = User.FindFirst(ClaimTypes.Email)?.Value;
                    ViewData["IsAuthenticated"] = true;
                }
                else
                {
                    ViewData["IsAuthenticated"] = false;
                }

                return View("~/Pages/Home/Views/_Home.cshtml", vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading home data: {ex.Message}");

                ViewData["IsAuthenticated"] = User.Identity.IsAuthenticated;
                return View("~/Pages/Home/Views/_Home.cshtml", new List<HomeCardDto>());
            }
        }
    }
}