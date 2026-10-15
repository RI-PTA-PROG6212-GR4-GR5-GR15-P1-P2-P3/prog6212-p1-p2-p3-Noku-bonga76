
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public CategoriesController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/events/1/categories
    [HttpGet("api/events/{eventId:int}/categories")]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == eventId);

        if (!eventExists)
        {
            return NotFound(new { message = "Event not found." });
        }

        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.EventID == eventId)
            .Select(c => new CategoryDTO
            {
                CategoryID = c.CategoryID,
                EventID = c.EventID,
                CategoryName = c.CategoryName,
                Description = c.Description
            })
            .ToListAsync();

        return Ok(categories);
    }

    // POST: api/events/1/categories
    [HttpPost("api/events/{eventId:int}/categories")]
    public async Task<IActionResult> CreateCategory(
        int eventId,
        [FromBody] CategoryDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        var eventExists = await _context.Events
            .AnyAsync(e => e.EventID == eventId);

        if (!eventExists)
        {
            return NotFound(new { message = "Event not found." });
        }

        var category = new Category
        {
            EventID = eventId,
            CategoryName = request.CategoryName.Trim(),
            Description = request.Description?.Trim()
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var response = new CategoryDTO
        {
            CategoryID = category.CategoryID,
            EventID = category.EventID,
            CategoryName = category.CategoryName,
            Description = category.Description
        };

        return Created(
            $"/api/categories/{category.CategoryID}",
            response);
    }

    // PUT: api/categories/1
    [HttpPut("api/categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] CategoryDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        category.CategoryName = request.CategoryName.Trim();
        category.Description = request.Description?.Trim();

        await _context.SaveChangesAsync();

        return Ok(new CategoryDTO
        {
            CategoryID = category.CategoryID,
            EventID = category.EventID,
            CategoryName = category.CategoryName,
            Description = category.Description
        });
    }

    // DELETE: api/categories/1
    [HttpDelete("api/categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryID == id);

        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        var hasEnrolments = await _context.Enrolments
            .AnyAsync(e => e.CategoryID == id);

        if (hasEnrolments)
        {
            return Conflict(new
            {
                message = "Cannot delete a category that has enrolments."
            });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Category deleted successfully." });
    }
}
