namespace RaceDay.API.DTOs;

public class ResultDTO
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalFinishers { get; set; }
}

