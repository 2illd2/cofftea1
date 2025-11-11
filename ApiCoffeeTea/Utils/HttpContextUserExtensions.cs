
using System.Security.Claims;

namespace ApiCoffeeTea.Utils;

public static class HttpContextUserExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : null;
    }
}
