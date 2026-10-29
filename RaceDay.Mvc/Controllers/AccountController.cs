using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;
    public AccountController(ApiClient api) => _api = api;

    [HttpGet] public IActionResult Login() => View(new LoginVm());

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var res = await _api.Post<UserDto>("api/auth/login", new { vm.Email, vm.Password });
        if (!res.Ok || res.Data is null)
        {
            ModelState.AddModelError("", res.Error ?? "Login failed.");
            return View(vm);
        }
        HttpContext.Session.SetInt32("UserId", res.Data.Id);
        HttpContext.Session.SetString("Role", res.Data.Role);
        HttpContext.Session.SetString("Name", res.Data.FirstName);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet] public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var res = await _api.Post<object>("api/auth/register",
            new { vm.FirstName, vm.LastName, vm.Email, vm.Password, vm.Role, vm.Phone });
        if (!res.Ok)
        {
            ModelState.AddModelError("", res.Error ?? "Registration failed.");
            return View(vm);
        }
        TempData["Success"] = "Account created! Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _api.Post<object>("api/auth/logout");
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
