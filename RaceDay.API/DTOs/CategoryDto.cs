namespace RaceDay.API.DTOs;

public class CategoryDTO
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}