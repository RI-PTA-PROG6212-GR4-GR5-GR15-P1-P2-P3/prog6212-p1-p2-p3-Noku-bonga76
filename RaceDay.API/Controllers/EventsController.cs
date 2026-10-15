using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[Route("api/events")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EventsController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/events
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .Select(e => new EventDTO
            {
                EventID = e.EventID,
                OrganiserID = e.OrganiserID,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                Distance = e.Distance,
                EventType = e.EventType
            })
            .ToListAsync();

        return Ok(events);
    }

    // GET: api/events/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var eventItem = await _context.Events
            .AsNoTracking()
            .Where(e => e.EventID == id)
            .Select(e => new EventDTO
            {
                EventID = e.EventID,
                OrganiserID = e.OrganiserID,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                Distance = e.Distance,
                EventType = e.EventType
            })
            .FirstOrDefaultAsync();

        if (eventItem == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        return Ok(eventItem);
    }

    // POST: api/events
    [HttpPost]
    public async Task<IActionResult> CreateEvent(
        [FromBody] EventDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.OrganiserID <= 0 ||
            string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Location) ||
            string.IsNullOrWhiteSpace(request.EventType) ||
            request.Distance <= 0)
        {
            return BadRequest(new
            {
                message = "A valid organiser, event name, location, event type and positive distance are required."
            });
        }

        var organiserExists = await _context.Users
            .AnyAsync(u =>
                u.UserID == request.OrganiserID &&
                u.Role == "Organiser");

        if (!organiserExists)
        {
            return BadRequest(new
            {
                message = "The specified organiser does not exist or is not an organiser."
            });
        }

        var eventItem = new Event
        {
            OrganiserID = request.OrganiserID,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            Distance = request.Distance,
            EventType = request.EventType.Trim()
        };

        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        var response = new EventDTO
        {
            EventID = eventItem.EventID,
            OrganiserID = eventItem.OrganiserID,
            Name = eventItem.Name,
            Description = eventItem.Description,
            EventDate = eventItem.EventDate,
            Location = eventItem.Location,
            Distance = eventItem.Distance,
            EventType = eventItem.EventType
        };

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = eventItem.EventID },
            response);
    }

    // PUT: api/events/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] EventDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (eventItem == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Location) ||
            string.IsNullOrWhiteSpace(request.EventType) ||
            request.Distance <= 0)
        {
            return BadRequest(new
            {
                message = "Event name, location, event type and a positive distance are required."
            });
        }

        // Keep the existing organiser assigned to the event.
        // Organiser identity and authorization must be enforced
        // when authentication is configured.

        eventItem.Name = request.Name.Trim();
        eventItem.Description = request.Description?.Trim();
        eventItem.EventDate = request.EventDate;
        eventItem.Location = request.Location.Trim();
        eventItem.Distance = request.Distance;
        eventItem.EventType = request.EventType.Trim();

        await _context.SaveChangesAsync();

        var response = new EventDTO
        {
            EventID = eventItem.EventID,
            OrganiserID = eventItem.OrganiserID,
            Name = eventItem.Name,
            Description = eventItem.Description,
            EventDate = eventItem.EventDate,
            Location = eventItem.Location,
            Distance = eventItem.Distance,
            EventType = eventItem.EventType
        };

        return Ok(new
        {
            message = "Event updated successfully.",
            eventDetails = response
        });
    }

    // DELETE: api/events/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (eventItem == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        var hasEnrolments = await _context.Enrolments
            .AnyAsync(e => e.EventID == id);

        if (hasEnrolments)
        {
            return Conflict(new
            {
                message = "This event cannot be deleted because it has enrolments."
            });
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Event deleted successfully."
        });
    }
}
