namespace TennisScores.Domain.Dtos;

public class MatchDto
{
    public Guid Id { get; set; }
    public required string Player1FirstName { get; set; }
    public required string Player1LastName { get; set; }
    public required string Player2FirstName { get; set; }
    public required string Player2LastName { get; set; }
    public int BestOfSets { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Surface { get; set; }
    public string? WinnerFirstName { get; set; }
    public string? WinnerLastName { get; set; }
}
