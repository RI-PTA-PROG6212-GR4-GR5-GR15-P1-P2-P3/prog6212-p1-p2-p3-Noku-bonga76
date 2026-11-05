using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Services;

public static class RaceDaySessionExtensions
{
    private const string UserIdKey = "UserID";
    private const string RoleKey = "Role";

    private static bool HasSession(HttpContext? ctx) => ctx?.Features.Get<ISessionFeature>() is not null;

    public static void StartSession(this HttpContext? ctx, int userId, string role)
    {
        if (!HasSession(ctx)) return;
        ctx!.Session.SetInt32(UserIdKey, userId);
        ctx.Session.SetString(RoleKey, role);
    }

    public static void EndSession(this HttpContext? ctx)
    {
        if (HasSession(ctx)) ctx!.Session.Clear();
    }

    public static int? GetSessionUserId(this HttpContext? ctx) =>
        HasSession(ctx) ? ctx!.Session.GetInt32(UserIdKey) : null;

    public static string? GetSessionRole(this HttpContext? ctx) =>
        HasSession(ctx) ? ctx!.Session.GetString(RoleKey) : null;

    /// <summary>Returns null when allowed, otherwise a 401 (not logged in) or 403 (wrong role).</summary>
    public static IActionResult? RequireRole(this ControllerBase controller, params string[] roles)
    {
        if (controller.HttpContext is null) return null;

        if (controller.HttpContext.GetSessionUserId() is null)
            return controller.Unauthorized(new { message = "Please log in to continue." });

        var role = controller.HttpContext.GetSessionRole();
        if (role is null || !roles.Contains(role))
            return controller.StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You do not have permission to do that." });

        return null;
    }
}

