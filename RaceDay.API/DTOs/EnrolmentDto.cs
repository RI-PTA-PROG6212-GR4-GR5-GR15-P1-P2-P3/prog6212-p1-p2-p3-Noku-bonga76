namespace RaceDay.API.DTOs;

public class EnrolmentDTO
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}