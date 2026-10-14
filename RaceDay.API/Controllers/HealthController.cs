
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly RaceDayContext _context;

    public HealthController(RaceDayContext context)
    {
        _context = context;
    }

    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase()
    {
        bool connected = await _context.Database.CanConnectAsync();

        if (!connected)
        {
            return StatusCode(503, new
            {
                message = "Database connection failed."
            });
        }

        return Ok(new
        {
            message = "RaceDay API connected to SQL Server successfully."
        });
    }
}