using RaceDay.API.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/results")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context)
    {
        _context = context;
    }

    // POST: api/results
    [HttpPost]
    public async Task<IActionResult> CreateResult(
        [FromBody] ResultDTO request)
    {
        var denied = this.RequireRole("Organiser");
        if (denied != null) return denied;
        if (request.EnrolmentID <= 0 ||
            request.FinishTime < TimeSpan.Zero ||
            request.FinishTime >= TimeSpan.FromDays(1) ||
            request.FinishingPosition <= 0)
        {
            return BadRequest(new
            {
                message = "A valid enrolment, finish time and positive finishing position are required."
            });
        }

        var enrolment = await _context.Enrolments
            .FirstOrDefaultAsync(e =>
                e.EnrolmentID == request.EnrolmentID);

        if (enrolment == null)
        {
            return BadRequest(new
            {
                message = "The specified enrolment does not exist."
            });
        }

        var resultExists = await _context.Results
            .AnyAsync(r => r.EnrolmentID == request.EnrolmentID);

        if (resultExists)
        {
            return Conflict(new
            {
                message = "A result already exists for this enrolment."
            });
        }

        var result = new Result
        {
            EnrolmentID = request.EnrolmentID,
            FinishTime = request.FinishTime,
            FinishingPosition = request.FinishingPosition
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        var response = new ResultDTO
        {
            ResultID = result.ResultID,
            EnrolmentID = result.EnrolmentID,
            FinishTime = result.FinishTime,
            FinishingPosition = result.FinishingPosition
        };

        return Created(
            $"/api/results/{result.ResultID}",
            response);
    }

    // PUT: api/results
    [HttpPut]
    public async Task<IActionResult> UpdateResult(
        [FromBody] ResultDTO request)
    {
        var denied = this.RequireRole("Organiser");
        if (denied != null) return denied;
        if (request.ResultID <= 0 ||
            request.EnrolmentID <= 0 ||
            request.FinishTime < TimeSpan.Zero ||
            request.FinishTime >= TimeSpan.FromDays(1) ||
            request.FinishingPosition <= 0)
        {
            return BadRequest(new
            {
                message = "Valid result, enrolment, finish time and finishing position are required."
            });
        }

        var result = await _context.Results
            .FirstOrDefaultAsync(r => r.ResultID == request.ResultID);

        if (result == null)
        {
            return NotFound(new { message = "Result not found." });
        }

        var enrolmentExists = await _context.Enrolments
            .AnyAsync(e => e.EnrolmentID == request.EnrolmentID);

        if (!enrolmentExists)
        {
            return BadRequest(new
            {
                message = "The specified enrolment does not exist."
            });
        }

        var enrolmentHasOtherResult = await _context.Results
            .AnyAsync(r =>
                r.EnrolmentID == request.EnrolmentID &&
                r.ResultID != request.ResultID);

        if (enrolmentHasOtherResult)
        {
            return Conflict(new
            {
                message = "Another result already exists for this enrolment."
            });
        }

        result.EnrolmentID = request.EnrolmentID;
        result.FinishTime = request.FinishTime;
        result.FinishingPosition = request.FinishingPosition;

        await _context.SaveChangesAsync();

        return Ok(new ResultDTO
        {
            ResultID = result.ResultID,
            EnrolmentID = result.EnrolmentID,
            FinishTime = result.FinishTime,
            FinishingPosition = result.FinishingPosition
        });
    }

    // GET: api/results/my?participantId=1
    [HttpGet("my")]
    public async Task<IActionResult> GetMyResults(
        [FromQuery] int participantId)
    {
        var denied = this.RequireRole("Participant");
        if (denied != null) return denied;
        if (HttpContext.GetSessionUserId() is int sessionUserId)
        {
            participantId = sessionUserId;
        }
        if (participantId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid participant ID is required."
            });
        }

        var results = await (
            from result in _context.Results.AsNoTracking()
            join enrolment in _context.Enrolments.AsNoTracking()
                on result.EnrolmentID equals enrolment.EnrolmentID
            where enrolment.ParticipantID == participantId
            select new ResultDTO
            {
                ResultID = result.ResultID,
                EnrolmentID = result.EnrolmentID,
                FinishTime = result.FinishTime,
                FinishingPosition = result.FinishingPosition,
                ParticipantName = _context.Users.Where(u => u.UserID == enrolment.ParticipantID).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault() ?? "",
                EventName = _context.Events.Where(ev => ev.EventID == enrolment.EventID).Select(ev => ev.Name).FirstOrDefault() ?? "",
                EventDate = _context.Events.Where(ev => ev.EventID == enrolment.EventID).Select(ev => ev.EventDate).FirstOrDefault(),
                CategoryName = _context.Categories.Where(cat => cat.CategoryID == enrolment.CategoryID).Select(cat => cat.CategoryName).FirstOrDefault() ?? "",
                TotalFinishers = _context.Results.Count(r2 => _context.Enrolments.Any(en2 => en2.EnrolmentID == r2.EnrolmentID && en2.EventID == enrolment.EventID))
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/events/1/results
    [HttpGet("/api/events/{eventId:int}/results")]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        var denied = this.RequireRole("Organiser");
        if (denied != null) return denied;
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == eventId);

        if (!eventExists)
        {
            return NotFound(new { message = "Event not found." });
        }

        var results = await (
            from result in _context.Results.AsNoTracking()
            join enrolment in _context.Enrolments.AsNoTracking()
                on result.EnrolmentID equals enrolment.EnrolmentID
            where enrolment.EventID == eventId
            orderby result.FinishingPosition
            select new ResultDTO
            {
                ResultID = result.ResultID,
                EnrolmentID = result.EnrolmentID,
                FinishTime = result.FinishTime,
                FinishingPosition = result.FinishingPosition,
                ParticipantName = _context.Users.Where(u => u.UserID == enrolment.ParticipantID).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault() ?? "",
                EventName = _context.Events.Where(ev => ev.EventID == enrolment.EventID).Select(ev => ev.Name).FirstOrDefault() ?? "",
                EventDate = _context.Events.Where(ev => ev.EventID == enrolment.EventID).Select(ev => ev.EventDate).FirstOrDefault(),
                CategoryName = _context.Categories.Where(cat => cat.CategoryID == enrolment.CategoryID).Select(cat => cat.CategoryName).FirstOrDefault() ?? "",
                TotalFinishers = _context.Results.Count(r2 => _context.Enrolments.Any(en2 => en2.EnrolmentID == r2.EnrolmentID && en2.EventID == enrolment.EventID))
            })
            .ToListAsync();

        return Ok(results);
    }
}






