using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeTea.Pages.Admin.Controllers;

[Authorize(Roles = "admin")]
public class AdminHomeController : Controller
{
    // GET: /admin
    [HttpGet("/admin")]
    public IActionResult Index()
    {
        return View("~/Pages/Admin/Views/Index.cshtml");
    }
}
