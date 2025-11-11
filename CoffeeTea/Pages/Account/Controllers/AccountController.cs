using ApiCoffeeTea.DTO;
using CoffeeTea.Pages.Account.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoffeeTea.Pages.Account.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient _http;
    public AccountController(IHttpClientFactory factory) => _http = factory.CreateClient("CoffeeTeaApi");

    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View("~/Pages/Account/Views/Login.cshtml", new LoginVm());
    }


    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View("~/Pages/Account/Views/Login.cshtml", vm);

        var resp = await _http.PostAsJsonAsync("/api/auth/login", new LoginDto(vm.Email, vm.Password));
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return View("~/Pages/Account/Views/Login.cshtml", vm);
        }

        var envelope = await resp.Content.ReadFromJsonAsync<ApiCoffeeTea.DTO.LoginEnvelope>();
        if (envelope?.user is null || string.IsNullOrWhiteSpace(envelope.token))
        {
            ModelState.AddModelError(string.Empty, "Ошибка авторизации.");
            return View("~/Pages/Account/Views/Login.cshtml", vm);
        }

        await SignInAsync(envelope.user);

        HttpContext.Session.SetString("jwt", envelope.token);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/register")]
    public IActionResult Register() => View("~/Pages/Account/Views/Register.cshtml", new RegisterVm());

    [HttpPost("/register")]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View("~/Pages/Account/Views/Register.cshtml", vm);

        var payload = new RegisterDto(vm.FirstName, vm.LastName, vm.MiddleName, vm.Email, vm.Phone, vm.Password);
        var resp = await _http.PostAsJsonAsync("/api/auth/register", payload);
        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(msg) ? "Ошибка регистрации." : msg);
            return View("~/Pages/Account/Views/Register.cshtml", vm);
        }

        var user = await resp.Content.ReadFromJsonAsync<UserDto>();
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Ошибка регистрации.");
            return View("~/Pages/Account/Views/Register.cshtml", vm);
        }

        await SignInAsync(user);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }

    private async Task SignInAsync(UserDto u)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{u.FirstName} {u.LastName}".Trim()),
            new Claim(ClaimTypes.Email, u.Email),
            new Claim(ClaimTypes.Role, u.Role ?? "customer")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
    [Authorize]
    [HttpGet("/profile")]
    public async Task<IActionResult> Profile()
    {
        var vm = new ProfileVm();

        var resp = await _http.GetAsync("/api/me");
        if (resp.IsSuccessStatusCode)
        {
            var me = await resp.Content.ReadFromJsonAsync<ApiCoffeeTea.DTO.MeDto>();
            if (me != null)
            {
                vm = new ProfileVm
                {
                    Id = me.Id,
                    FirstName = me.FirstName,
                    LastName = me.LastName,
                    MiddleName = me.MiddleName,
                    Email = me.Email,
                    Phone = me.Phone,
                    Role = me.Role,
                    CreatedAt = me.created_at
                };
            }
        }

        return View("~/Pages/Account/Views/Profile.cshtml", vm);
    }


    [Authorize]
    [HttpGet("/account/profile-partial")]
    public async Task<IActionResult> ProfilePartial()
    {
        var resp = await _http.GetAsync("/api/me");
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return PartialView("~/Pages/Account/Views/_ProfileCard.cshtml", new ProfileVm());

        resp.EnsureSuccessStatusCode();
        var me = await resp.Content.ReadFromJsonAsync<ApiCoffeeTea.DTO.MeDto>();
        if (me == null) return PartialView("~/Pages/Account/Views/_ProfileCard.cshtml", new ProfileVm());

        var vm = new ProfileVm
        {
            Id = me.Id,
            FirstName = me.FirstName,
            LastName = me.LastName,
            MiddleName = me.MiddleName,
            Email = me.Email,
            Phone = me.Phone,
            Role = me.Role,
            CreatedAt = me.created_at
        };

        return PartialView("~/Pages/Account/Views/_ProfileCard.cshtml", vm);
    }

    [Authorize, ValidateAntiForgeryToken]
    [HttpPost("/account/profile")]
    public async Task<IActionResult> SaveProfile(ProfileVm vm)
    {
        if (!ModelState.IsValid)
            return View("~/Pages/Account/Views/Profile.cshtml", vm);

        var payload = new ApiCoffeeTea.DTO.MeUpdateDto
        {
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            MiddleName = vm.MiddleName,
            Phone = vm.Phone
        };

        var resp = await _http.PutAsJsonAsync("/api/me", payload);
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Не удалось сохранить профиль.");
            return View("~/Pages/Account/Views/Profile.cshtml", vm);
        }

        TempData["ProfileSaved"] = true;
        return RedirectToAction(nameof(Profile));
    }

}