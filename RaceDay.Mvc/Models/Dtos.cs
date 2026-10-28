using System.ComponentModel.DataAnnotations;
namespace RaceDay.Mvc.Models;

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? Phone { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public double Distance { get; set; }
    public string EventType { get; set; } = "Run";
    public string? BannerUrl { get; set; }
    public int? OrganiserId { get; set; }
    public int EnrolmentCount { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
}

public class EnrolmentDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public string Status { get; set; } = "Pending";
}

public class ResultDto
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public string ParticipantName { get; set; } = "";
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = "";
    public string FinishTime { get; set; } = "";
    public int FinishingPosition { get; set; }
    public int TotalFinishers { get; set; }
}

public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}

public class RegisterVm
{
    [Required, Display(Name = "First name")] public string FirstName { get; set; } = "";
    [Required, Display(Name = "Last name")] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    public string? Phone { get; set; }
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";
    [Required] public string Role { get; set; } = "Participant";
}

public class EventFormVm
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = "";
    [Required] public string Description { get; set; } = "";
    [Required, DataType(DataType.Date), Display(Name = "Event date")] public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);
    [Required] public string Location { get; set; } = "";
    [Required, Range(0.1, 500), Display(Name = "Distance (km)")] public double Distance { get; set; }
    [Required, Display(Name = "Event type")] public string EventType { get; set; } = "Run";
    public string? BannerUrl { get; set; }
    [Display(Name = "Banner image")] public IFormFile? Banner { get; set; }
}

public class ProfileVm
{
    [Required, Display(Name = "First name")] public string FirstName { get; set; } = "";
    [Required, Display(Name = "Last name")] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? ProfilePictureUrl { get; set; }
    [Display(Name = "Profile picture")] public IFormFile? Picture { get; set; }
}

public class EventDetailVm { public EventDto Event { get; set; } = new(); public List<CategoryDto> Categories { get; set; } = new(); public bool AlreadyEnrolled { get; set; } }
public class DashboardVm { public List<EventDto> Events { get; set; } = new(); }
public class CategoriesVm { public EventDto Event { get; set; } = new(); public List<CategoryDto> Categories { get; set; } = new(); }
public class ResultsVm { public EventDto Event { get; set; } = new(); public List<EnrolmentDto> Enrolments { get; set; } = new(); public List<ResultDto> Results { get; set; } = new(); }
public class EnrolmentsVm { public EventDto Event { get; set; } = new(); public List<EnrolmentDto> Enrolments { get; set; } = new(); }
