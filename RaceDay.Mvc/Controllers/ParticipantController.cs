using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[Role("Participant")]
public class ParticipantController : Controller
{
    private readonly ApiClient _api;
    public ParticipantController(ApiClient api) => _api = api;

    public async Task<IActionResult> MyEnrolments()
    {
        var res = await _api.Get<List<EnrolmentDto>>("api/enrolments/my");
        if (!res.Ok) TempData["Error"] = res.Error;
        return View((res.Data ?? new()).OrderBy(e => e.EventDate).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var res = await _api.Delete<object>($"api/enrolments/{id}");
        if (res.Ok) TempData["Success"] = "Enrolment cancelled."; else TempData["Error"] = res.Error;
        return RedirectToAction(nameof(MyEnrolments));
    }

    public async Task<IActionResult> MyResults()
    {
        var res = await _api.Get<List<ResultDto>>("api/results/my");
        if (!res.Ok) TempData["Error"] = res.Error;
        return View((res.Data ?? new()).OrderByDescending(r => r.EventDate).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var res = await _api.Get<UserDto>("api/users/profile");
        if (!res.Ok || res.Data is null) { TempData["Error"] = res.Error; return RedirectToAction("Index", "Home"); }
        var u = res.Data;
        return View(new ProfileVm { FirstName = u.FirstName, LastName = u.LastName, Email = u.Email, Phone = u.Phone, ProfilePictureUrl = u.ProfilePictureUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var upd = await _api.Put<UserDto>("api/users/profile", new { vm.FirstName, vm.LastName, vm.Email, vm.Phone });
        if (!upd.Ok) { ModelState.AddModelError("", upd.Error ?? "Update failed."); return View(vm); }

        if (vm.Picture is { Length: > 0 })
        {
            if (vm.Picture.Length > 5_000_000 || !vm.Picture.ContentType.StartsWith("image/"))
            { ModelState.AddModelError("", "Please upload an image smaller than 5 MB."); return View(vm); }
            var up = await _api.Upload<UserDto>("api/users/profile/picture", vm.Picture);
            if (!up.Ok) { ModelState.AddModelError("", up.Error ?? "Picture upload failed."); return View(vm); }
        }
        HttpContext.Session.SetString("Name", vm.FirstName);
        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }
}
