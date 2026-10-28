using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Mvc.Filters;

/// <summary>Unauthenticated -> Login. Wrong role -> own home page.</summary>
public class RoleAttribute : ActionFilterAttribute
{
    private readonly string _role;
    public RoleAttribute(string role) => _role = role;

    public override void OnActionExecuting(ActionExecutingContext ctx)
    {
        var role = ctx.HttpContext.Session.GetString("Role");
        if (string.IsNullOrEmpty(role))
            ctx.Result = new RedirectToActionResult("Login", "Account", null);
        else if (role != _role)
            ctx.Result = new RedirectToActionResult("Index", "Home", null);
    }
}
