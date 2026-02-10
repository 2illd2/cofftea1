using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeTea.Pages.Admin.Settings.Controllers;

[Authorize(Roles = "admin")]
public class AdminSettingsController : Controller
{
    private readonly IConfiguration _configuration;

    public AdminSettingsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // GET: /admin/settings
    [HttpGet("/admin/settings")]
    public IActionResult Index()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
        ViewBag.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        return View("~/Pages/Admin/Settings/Views/Index.cshtml");
    }
}
