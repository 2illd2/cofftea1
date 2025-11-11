using System.Net.Http.Headers;

public class ApiAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _ctx;
    public ApiAuthHandler(IHttpContextAccessor ctx) => _ctx = ctx;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var http = _ctx.HttpContext;

        var token =
            http?.Session.GetString("jwt")
            ?? http?.User?.FindFirst("Jwt")?.Value
            ?? http?.Request.Cookies["jwt"];

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, ct);
    }
}
