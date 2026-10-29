using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

public record ApiResult<T>(bool Ok, T? Data, string? Error, HttpStatusCode Status);

/// <summary>
/// Single gateway to the Part 2 API. Forwards the API's session cookie (kept in the MVC session)
/// on every call so the API's server-side session + role checks keep working.
/// </summary>
public class ApiClient
{
    private const string CookieKey = "ApiCookie";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;

    public ApiClient(HttpClient http, IHttpContextAccessor ctx) { _http = http; _ctx = ctx; }

    private ISession Session => _ctx.HttpContext!.Session;

    public Task<ApiResult<T>> Get<T>(string url) => Send<T>(HttpMethod.Get, url, null);
    public Task<ApiResult<T>> Post<T>(string url, object? body = null) => Send<T>(HttpMethod.Post, url, body is null ? null : Body(body));
    public Task<ApiResult<T>> Put<T>(string url, object? body = null) => Send<T>(HttpMethod.Put, url, body is null ? null : Body(body));
    public Task<ApiResult<T>> Delete<T>(string url) => Send<T>(HttpMethod.Delete, url, null);

    /// <summary>Sends a file to the API, which stores it in Azure Blob Storage.</summary>
    public Task<ApiResult<T>> Upload<T>(string url, IFormFile file, string field = "file")
    {
        var content = new MultipartFormDataContent();
        var part = new StreamContent(file.OpenReadStream());
        part.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(part, field, file.FileName);
        return Send<T>(HttpMethod.Post, url, content);
    }

    private static HttpContent Body(object o) =>
        new StringContent(JsonSerializer.Serialize(o, Json), Encoding.UTF8, "application/json");

    private async Task<ApiResult<T>> Send<T>(HttpMethod method, string url, HttpContent? content)
    {
        try
        {
            var req = new HttpRequestMessage(method, url) { Content = content };
            var cookie = Session.GetString(CookieKey);
            if (!string.IsNullOrEmpty(cookie)) req.Headers.Add("Cookie", cookie);

            var res = await _http.SendAsync(req);
            StoreCookies(res);

            var text = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
            {
                T? data = default;
                if (!string.IsNullOrWhiteSpace(text) && typeof(T) != typeof(object))
                    data = JsonSerializer.Deserialize<T>(text, Json);
                return new(true, data, null, res.StatusCode);
            }
            return new(false, default, FriendlyError(res.StatusCode, text), res.StatusCode);
        }
        catch (Exception)
        {
            return new(false, default, "We couldn't reach the RaceDay service. Please try again shortly.", HttpStatusCode.ServiceUnavailable);
        }
    }

    private void StoreCookies(HttpResponseMessage res)
    {
        if (!res.Headers.TryGetValues("Set-Cookie", out var values)) return;
        var jar = (Session.GetString(CookieKey) ?? "")
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.Split('=', 2)).Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => p[1]);
        foreach (var sc in values)
        {
            var pair = sc.Split(';')[0].Split('=', 2);
            if (pair.Length == 2) jar[pair[0].Trim()] = pair[1];
        }
        Session.SetString(CookieKey, string.Join("; ", jar.Select(kv => $"{kv.Key}={kv.Value}")));
    }

    private static string FriendlyError(HttpStatusCode code, string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            foreach (var key in new[] { "message", "detail", "title" })
                if (doc.RootElement.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.String && p.GetString()!.Length < 200)
                    return p.GetString()!;
        }
        catch { /* body not JSON */ }

        return code switch
        {
            HttpStatusCode.Unauthorized => "Invalid credentials or your session has expired.",
            HttpStatusCode.Forbidden => "You don't have permission to do that.",
            HttpStatusCode.NotFound => "We couldn't find what you were looking for.",
            HttpStatusCode.Conflict => "That conflicts with existing data (for example, you're already enrolled).",
            HttpStatusCode.BadRequest => "Some of the information provided is invalid.",
            _ => "Something went wrong. Please try again."
        };
    }
}
