using ApiCoffeeTea.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Доступ к HttpContext из handler’а
builder.Services.AddHttpContextAccessor();

// Сессии — для хранения JWT
builder.Services.AddSession();

// Cookie-аутентификация 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/login";
        o.LogoutPath = "/logout";
        o.AccessDeniedPath = "/access-denied";
        o.SlidingExpiration = true;
    });

// Делегирующий хендлер, который подставляет токен в вызовы API
builder.Services.AddTransient<ApiAuthHandler>();

// HttpClient к API + наш handler
builder.Services.AddHttpClient("CoffeeTeaApi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!); 
})
.AddHttpMessageHandler<ApiAuthHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
