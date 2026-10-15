

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly IPasswordService _passwordService;

    public AuthController(
        RaceDayContext context,
        IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest(new
            {
                message = "First name, last name, email, password and role are required."
            });
        }

        if (request.Role != "Participant" &&
            request.Role != "Organiser")
        {
            return BadRequest(new
            {
                message = "Role must be Participant or Organiser."
            });
        }

        var email = request.Email.Trim();

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = _passwordService.HashPassword(
                request.Password),
            Role = request.Role,
            Phone = request.Phone?.Trim()
        };

        _context.Users.Add(user);
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

        return CreatedAtAction(
            nameof(Register),
            new { id = user.UserID },
            response);
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Email and password are required."
            });
        }

        var email = request.Email.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null ||
            !_passwordService.VerifyPassword(
                request.Password,
                user.PasswordHash))
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
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

        return Ok(new
        {
            message = "Login successful.",
            user = response
        });
    }

    // POST: api/auth/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new
        {
            message = "Logout endpoint reached successfully."
        });
    }
}
