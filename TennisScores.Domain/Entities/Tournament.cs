using TennisScores.Domain.Enums;

namespace TennisScores.Domain.Entities;

public class Tournament
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Location { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }

    public string? MinRankingFft { get; set; }  // Example : "30/3"
    public string? MaxRankingFft { get; set; }  // Example : "15/1"

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    public AgeCategory? AgeCategory { get; set; }

    public TournamentType Type { get; set; }
    public MatchFormat MatchFormat { get; set; } = default!;
    public int MatchFormatId { get; set; }
    public TournamentSubType SubType { get; set; }
    public BallLevel BallLevel { get; set; }
    public CourtSurface Surface { get; set; }
    public PlayingCondition Condition { get; set; }

    public decimal? PrizeMoney { get; set; }
    public string? PrizeMoneyCurrency { get; set; } // 💶 ex: "EUR", "USD"

    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
