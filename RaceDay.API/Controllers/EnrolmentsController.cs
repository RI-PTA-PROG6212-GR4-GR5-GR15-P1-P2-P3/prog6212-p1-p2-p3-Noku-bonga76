
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EnrolmentsController(RaceDayContext context)
    {
        _context = context;
    }

    // POST: api/enrolments
    [HttpPost("api/enrolments")]
    public async Task<IActionResult> CreateEnrolment(
        [FromBody] EnrolmentDTO request)
    {
        var participantExists = await _context.Users
            .AnyAsync(u =>
                u.UserID == request.ParticipantID &&
                u.Role == "Participant");

        if (!participantExists)
        {
            return BadRequest(new
            {
                message = "The specified participant does not exist."
            });
        }

        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == request.EventID);

        if (!eventExists)
        {
            return BadRequest(new { message = "Event not found." });
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(c =>
                c.CategoryID == request.CategoryID &&
                c.EventID == request.EventID);

        if (category == null)
        {
            return BadRequest(new
            {
                message = "The category does not belong to this event."
            });
        }

        var alreadyEnrolled = await _context.Enrolments
            .AnyAsync(e =>
                e.ParticipantID == request.ParticipantID &&
                e.EventID == request.EventID);

        if (alreadyEnrolled)
        {
            return Conflict(new
            {
                message = "The participant is already enrolled in this event."
            });
        }

        var enrolment = new Enrolment
        {
            ParticipantID = request.ParticipantID,
            EventID = request.EventID,
            CategoryID = request.CategoryID,
            EnrolmentDate = DateTime.Now,
            Status = "Confirmed"
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        var response = new EnrolmentDTO
        {
            EnrolmentID = enrolment.EnrolmentID,
            ParticipantID = enrolment.ParticipantID,
            EventID = enrolment.EventID,
            CategoryID = enrolment.CategoryID,
            EnrolmentDate = enrolment.EnrolmentDate,
            Status = enrolment.Status
        };

        return Created(
            $"/api/enrolments/{enrolment.EnrolmentID}",
            response);
    }

    // GET: api/enrolments/my?participantId=1
    [HttpGet("api/enrolments/my")]
    public async Task<IActionResult> GetMyEnrolments(
        [FromQuery] int participantId)
    {
        if (participantId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid participant ID is required."
            });
        }

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.ParticipantID == participantId)
            .Select(e => new EnrolmentDTO
            {
                EnrolmentID = e.EnrolmentID,
                ParticipantID = e.ParticipantID,
                EventID = e.EventID,
                CategoryID = e.CategoryID,
                EnrolmentDate = e.EnrolmentDate,
                Status = e.Status
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // GET: api/events/1/enrolments
    [HttpGet("api/events/{eventId:int}/enrolments")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == eventId);

        if (!eventExists)
        {
            return NotFound(new { message = "Event not found." });
        }

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.EventID == eventId)
            .Select(e => new EnrolmentDTO
            {
                EnrolmentID = e.EnrolmentID,
                ParticipantID = e.ParticipantID,
                EventID = e.EventID,
                CategoryID = e.CategoryID,
                EnrolmentDate = e.EnrolmentDate,
                Status = e.Status
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // DELETE: api/enrolments/1
    [HttpDelete("api/enrolments/{id:int}")]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var enrolment = await _context.Enrolments
            .FirstOrDefaultAsync(e => e.EnrolmentID == id);

        if (enrolment == null)
        {
            return NotFound(new
            {
                message = "Enrolment not found."
            });
        }

        var hasResult = await _context.Results
            .AnyAsync(r => r.EnrolmentID == id);

        if (hasResult)
        {
            return Conflict(new
            {
                message = "This enrolment cannot be cancelled because a result has been recorded."
            });
        }

        _context.Enrolments.Remove(enrolment);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Enrolment cancelled successfully."
        });
    }
}
