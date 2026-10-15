
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests.Controllers;

public class EventsControllerTests : IDisposable
{
    private readonly RaceDayContext _context;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new RaceDayContext(options);
        _controller = new EventsController(_context);

        _context.Users.Add(new User
        {
            UserID = 1,
            FirstName = "Test",
            LastName = "Organiser",
            Email = "organiser@example.com",
            PasswordHash = "test-hash",
            Role = "Organiser"
        });

        _context.SaveChanges();
    }

    private Event CreateTestEvent(int id = 1)
    {
        return new Event
        {
            EventID = id,
            OrganiserID = 1,
            Name = "RaceDay Test Run",
            Description = "Automated test event",
            EventDate = new DateTime(2027, 5, 10),
            Location = "Johannesburg",
            Distance = 10,
            EventType = "Running"
        };
    }

    private EventDTO CreateTestEventDTO()
    {
        return new EventDTO
        {
            OrganiserID = 1,
            Name = "New RaceDay Event",
            Description = "Test description",
            EventDate = new DateTime(2027, 6, 15),
            Location = "Pretoria",
            Distance = 15,
            EventType = "Running"
        };
    }

    [Fact]
    public async Task GetEvents_ReturnsOkWithEvents()
    {
        // Arrange
        _context.Events.Add(CreateTestEvent());
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetEvents();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var events = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(
            okResult.Value);

        Assert.Single(events);
    }

    [Fact]
    public async Task GetEvent_WhenEventExists_ReturnsOk()
    {
        // Arrange
        _context.Events.Add(CreateTestEvent());
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetEvent(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var eventDTO = Assert.IsType<EventDTO>(okResult.Value);

        Assert.Equal(1, eventDTO.EventID);
        Assert.Equal("RaceDay Test Run", eventDTO.Name);
    }

    [Fact]
    public async Task GetEvent_WhenEventDoesNotExist_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetEvent(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateEvent_WithValidDetails_ReturnsCreated()
    {
        // Arrange
        var request = CreateTestEventDTO();

        // Act
        var result = await _controller.CreateEvent(request);

        // Assert
        var createdResult =
            Assert.IsType<CreatedAtActionResult>(result);

        var response = Assert.IsType<EventDTO>(createdResult.Value);

        Assert.Equal("New RaceDay Event", response.Name);
        Assert.Equal(1, response.OrganiserID);
        Assert.Equal(15, response.Distance);
        Assert.Equal(1, await _context.Events.CountAsync());
    }

    [Fact]
    public async Task CreateEvent_WithInvalidDistance_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateTestEventDTO();
        request.Distance = 0;

        // Act
        var result = await _controller.CreateEvent(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(await _context.Events.ToListAsync());
    }

    [Fact]
    public async Task CreateEvent_WithUnknownOrganiser_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateTestEventDTO();
        request.OrganiserID = 999;

        // Act
        var result = await _controller.CreateEvent(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(await _context.Events.ToListAsync());
    }

    [Fact]
    public async Task CreateEvent_WithParticipantAsOrganiser_ReturnsBadRequest()
    {
        // Arrange
        _context.Users.Add(new User
        {
            UserID = 2,
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant@example.com",
            PasswordHash = "test-hash",
            Role = "Participant"
        });

        await _context.SaveChangesAsync();

        var request = CreateTestEventDTO();
        request.OrganiserID = 2;

        // Act
        var result = await _controller.CreateEvent(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateEvent_WhenEventExists_UpdatesDetails()
    {
        // Arrange
        _context.Events.Add(CreateTestEvent());
        await _context.SaveChangesAsync();

        var request = CreateTestEventDTO();
        request.OrganiserID = 999;
        request.Name = "Updated RaceDay Event";

        // Act
        var result = await _controller.UpdateEvent(1, request);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        var updatedEvent = await _context.Events.FindAsync(1);

        Assert.NotNull(updatedEvent);
        Assert.Equal("Updated RaceDay Event", updatedEvent.Name);

        // The controller preserves the existing organiser.
        Assert.Equal(1, updatedEvent.OrganiserID);
    }

    [Fact]
    public async Task UpdateEvent_WhenEventDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = CreateTestEventDTO();

        // Act
        var result = await _controller.UpdateEvent(999, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateEvent_WithInvalidDistance_ReturnsBadRequest()
    {
        // Arrange
        _context.Events.Add(CreateTestEvent());
        await _context.SaveChangesAsync();

        var request = CreateTestEventDTO();
        request.Distance = -1;

        // Act
        var result = await _controller.UpdateEvent(1, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteEvent_WhenEventExistsWithoutEnrolments_ReturnsOk()
    {
        // Arrange
        _context.Events.Add(CreateTestEvent());
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteEvent(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Empty(await _context.Events.ToListAsync());
    }

    [Fact]
    public async Task DeleteEvent_WhenEventDoesNotExist_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteEvent(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
