using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[Role("Organiser")]
public class OrganiserController : Controller
{
    private readonly ApiClient _api;
    public OrganiserController(ApiClient api) => _api = api;

    private static readonly string[] AllowedImages = { "image/jpeg", "image/png", "image/webp" };

    // The plan has no "my events" endpoint: load all events, keep this organiser's, and count enrolments per event.
    private async Task<List<EventDto>> LoadMyEvents()
    {
        var res = await _api.Get<List<EventDto>>("api/events");
        if (!res.Ok) TempData["Error"] = res.Error;
        var uid = HttpContext.Session.GetInt32("UserId");
        var events = (res.Data ?? new()).Where(e => e.OrganiserId is null || e.OrganiserId == uid).ToList();
        foreach (var e in events)
        {
            var en = await _api.Get<List<EnrolmentDto>>($"api/events/{e.Id}/enrolments");
            e.EnrolmentCount = en.Data?.Count(x => x.Status != "Cancelled") ?? 0;
        }
        return events.OrderBy(e => e.EventDate).ToList();
    }

    // ---------- Dashboard ----------
    public async Task<IActionResult> Dashboard()
    {
        return View(new DashboardVm { Events = await LoadMyEvents() });
    }

    // ---------- Events ----------
    public async Task<IActionResult> Events()
    {
        return View(await LoadMyEvents());
    }

    [HttpGet] public IActionResult CreateEvent() => View("EventForm", new EventFormVm());

    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventFormVm vm)
    {
        if (!ModelState.IsValid || !BannerOk(vm)) return View("EventForm", vm);
        var res = await _api.Post<EventDto>("api/events", Payload(vm));
        if (!res.Ok || res.Data is null) { ModelState.AddModelError("", res.Error ?? "Could not create event."); return View("EventForm", vm); }
        await UploadBanner(res.Data.Id, vm);
        TempData["Success"] = "Event created.";
        return RedirectToAction(nameof(Events));
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var res = await _api.Get<EventDto>($"api/events/{id}");
        if (!res.Ok || res.Data is null) { TempData["Error"] = res.Error; return RedirectToAction(nameof(Events)); }
        var e = res.Data;
        return View("EventForm", new EventFormVm { Id = e.Id, Name = e.Name, Description = e.Description, EventDate = e.EventDate, Location = e.Location, Distance = e.Distance, EventType = e.EventType, BannerUrl = e.BannerUrl });
    }

    [HttpPost]
    public async Task<IActionResult> EditEvent(EventFormVm vm)
    {
        if (!ModelState.IsValid || !BannerOk(vm)) return View("EventForm", vm);
        var res = await _api.Put<EventDto>($"api/events/{vm.Id}", Payload(vm));
        if (!res.Ok) { ModelState.AddModelError("", res.Error ?? "Could not update event."); return View("EventForm", vm); }
        await UploadBanner(vm.Id, vm);
        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Events));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var res = await _api.Delete<object>($"api/events/{id}");
        if (res.Ok) TempData["Success"] = "Event deleted."; else TempData["Error"] = res.Error;
        return RedirectToAction(nameof(Events));
    }

    private static object Payload(EventFormVm v) => new { v.Name, v.Description, v.EventDate, v.Location, v.Distance, v.EventType };

    private bool BannerOk(EventFormVm vm)
    {
        if (vm.Banner is null) return true;
        if (vm.Banner.Length > 5_000_000 || !AllowedImages.Contains(vm.Banner.ContentType))
        { ModelState.AddModelError("Banner", "Banner must be a JPG, PNG or WEBP under 5 MB."); return false; }
        return true;
    }

    private async Task UploadBanner(int eventId, EventFormVm vm)
    {
        if (vm.Banner is not { Length: > 0 }) return;
        var up = await _api.Upload<EventDto>($"api/events/{eventId}/banner", vm.Banner);
        if (!up.Ok) TempData["Error"] = "Event saved, but the banner upload failed: " + up.Error;
    }

    // ---------- Categories ----------
    public async Task<IActionResult> Categories(int id)
    {
        var ev = await _api.Get<EventDto>($"api/events/{id}");
        if (!ev.Ok || ev.Data is null) { TempData["Error"] = ev.Error; return RedirectToAction(nameof(Events)); }
        var cats = await _api.Get<List<CategoryDto>>($"api/events/{id}/categories");
        return View(new CategoriesVm { Event = ev.Data, Categories = cats.Data ?? new() });
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory(int id, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) TempData["Error"] = "Category name is required.";
        else
        {
            var res = await _api.Post<CategoryDto>($"api/events/{id}/categories", new { CategoryName = name.Trim(), Description = description?.Trim() });
            if (res.Ok) TempData["Success"] = "Category added."; else TempData["Error"] = res.Error;
        }
        return RedirectToAction(nameof(Categories), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id, int categoryId)
    {
        var res = await _api.Delete<object>($"api/categories/{categoryId}");
        if (res.Ok) TempData["Success"] = "Category removed."; else TempData["Error"] = res.Error;
        return RedirectToAction(nameof(Categories), new { id });
    }

    // ---------- Enrolments ----------
    public async Task<IActionResult> Enrolments(int id)
    {
        var ev = await _api.Get<EventDto>($"api/events/{id}");
        if (!ev.Ok || ev.Data is null) { TempData["Error"] = ev.Error; return RedirectToAction(nameof(Events)); }
        var en = await _api.Get<List<EnrolmentDto>>($"api/events/{id}/enrolments");
        return View(new EnrolmentsVm { Event = ev.Data, Enrolments = en.Data ?? new() });
    }

    // ---------- Results ----------
    public async Task<IActionResult> Results(int id)
    {
        var ev = await _api.Get<EventDto>($"api/events/{id}");
        if (!ev.Ok || ev.Data is null) { TempData["Error"] = ev.Error; return RedirectToAction(nameof(Events)); }
        var en = await _api.Get<List<EnrolmentDto>>($"api/events/{id}/enrolments");
        var rs = await _api.Get<List<ResultDto>>($"api/events/{id}/results");
        return View(new ResultsVm { Event = ev.Data, Enrolments = (en.Data ?? new()).Where(e => e.Status != "Cancelled").ToList(), Results = rs.Data ?? new() });
    }

    [HttpPost]
    public async Task<IActionResult> SaveResult(int id, int enrolmentId, int? resultId, string finishTime, int position)
    {
        if (!TimeSpan.TryParse(finishTime, out _) || position < 1)
        { TempData["Error"] = "Enter a valid finish time (hh:mm:ss) and a position of 1 or more."; return RedirectToAction(nameof(Results), new { id }); }

        var res = resultId is > 0
            ? await _api.Put<ResultDto>($"api/results/{resultId}", new { FinishTime = finishTime, FinishingPosition = position })
            : await _api.Post<ResultDto>("api/results", new { EnrolmentID = enrolmentId, FinishTime = finishTime, FinishingPosition = position });
        if (res.Ok) TempData["Success"] = "Result saved."; else TempData["Error"] = res.Error;
        return RedirectToAction(nameof(Results), new { id });
    }
}
