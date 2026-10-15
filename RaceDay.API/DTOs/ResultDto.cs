namespace RaceDay.API.DTOs;

public class ResultDTO
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
}
