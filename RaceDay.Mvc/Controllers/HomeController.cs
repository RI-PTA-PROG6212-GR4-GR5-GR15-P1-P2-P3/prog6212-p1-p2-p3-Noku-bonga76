using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ApiClient _api;
    public HomeController(ApiClient api) => _api = api;

    // Role-aware landing: Participant -> events list, Organiser -> dashboard, anonymous -> login.
    public async Task<IActionResult> Index(string? type, string? search, DateTime? from)
    {
        var role = HttpContext.Session.GetString("Role");
        if (role is null) return RedirectToAction("Login", "Account");
        if (role == "Organiser") return RedirectToAction("Dashboard", "Organiser");

        var res = await _api.Get<List<EventDto>>("api/events");
        if (!res.Ok) TempData["Error"] = res.Error;

        var events = (res.Data ?? new()).Where(e => e.EventDate.Date >= DateTime.Today);
        if (!string.IsNullOrWhiteSpace(type)) events = events.Where(e => e.EventType == type);
        if (!string.IsNullOrWhiteSpace(search))
            events = events.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                                    || e.Location.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (from.HasValue) events = events.Where(e => e.EventDate.Date >= from.Value.Date);

        ViewBag.Type = type; ViewBag.Search = search; ViewBag.From = from?.ToString("yyyy-MM-dd");
        return View(events.OrderBy(e => e.EventDate).ToList());
    }

    [Role("Participant")]
    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.Get<EventDto>($"api/events/{id}");
        if (!ev.Ok || ev.Data is null) { TempData["Error"] = ev.Error; return RedirectToAction(nameof(Index)); }
        var cats = await _api.Get<List<CategoryDto>>($"api/events/{id}/categories");
        var mine = await _api.Get<List<EnrolmentDto>>("api/enrolments/my");
        return View(new EventDetailVm
        {
            Event = ev.Data,
            Categories = cats.Data ?? new(),
            AlreadyEnrolled = mine.Data?.Any(e => e.EventId == id && e.Status != "Cancelled") ?? false
        });
    }

    [HttpPost, Role("Participant")]
    public async Task<IActionResult> Enrol(int id, int categoryId)
    {
        if (categoryId == 0) { TempData["Error"] = "Please select a category."; return RedirectToAction(nameof(Details), new { id }); }
        var res = await _api.Post<EnrolmentDto>("api/enrolments", new { EventID = id, CategoryID = categoryId });
        if (res.Ok) { TempData["Success"] = "You're in! Enrolment recorded."; return RedirectToAction("MyEnrolments", "Participant"); }
        TempData["Error"] = res.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    public IActionResult Error() => View();
}
