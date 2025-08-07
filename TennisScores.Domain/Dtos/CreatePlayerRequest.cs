using TennisScores.Domain.Enums;

namespace TennisScores.Domain.Dtos;

public class CreatePlayerRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Nationality { get; set; }
    public int? Age { get; set; }
    public FftRanking? FftRanking { get; set; }
}
