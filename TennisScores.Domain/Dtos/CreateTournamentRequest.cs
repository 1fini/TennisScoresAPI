using TennisScores.Domain.Enums;

namespace TennisScores.Domain.Dtos;

public class CreateTournamentRequest
{
    public string Name { get; set; } = default!;
    public string Location { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }

    public string? MinRankingFft { get; set; }
    public string? MaxRankingFft { get; set; }

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    public AgeCategory? AgeCategory { get; set; }
    public TournamentType Type { get; set; }
    public TournamentSubType SubType { get; set; }
    public BallLevel BallLevel { get; set; }
    public CourtSurface Surface { get; set; }
    public PlayingCondition Condition { get; set; }

    public decimal? PrizeMoney { get; set; }
    public string? PrizeMoneyCurrency { get; set; }

    public int MatchFormatId { get; set; }
}
