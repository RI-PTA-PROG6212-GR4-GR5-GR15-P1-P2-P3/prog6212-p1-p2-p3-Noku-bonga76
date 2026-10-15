
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly RaceDayContext _context;

    public UsersController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/users/profile?userId=1
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(
        [FromQuery] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid user ID is required."
            });
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User profile not found."
            });
        }

        var response = new UserProfileDTO
        {
            UserID = user.UserID,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Phone = user.Phone
        };

        return Ok(response);
    }

    // PUT: api/users/profile?userId=1
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromQuery] int userId,
        [FromBody] UserProfileDTO request)
    {
        if (userId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid user ID is required."
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new
            {
                message = "First name, last name and email are required."
            });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User profile not found."
            });
        }

        var email = request.Email.Trim();

        var emailExists = await _context.Users.AnyAsync(
            u => u.Email == email && u.UserID != userId);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "Another account already uses this email."
            });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        user.Phone = request.Phone?.Trim();

        // The user cannot change their role through profile updates.

        await _context.SaveChangesAsync();

        var response = new UserProfileDTO
        {
            UserID = user.UserID,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Phone = user.Phone
        };

        return Ok(new
        {
            message = "Profile updated successfully.",
            user = response
        });
    }
}
