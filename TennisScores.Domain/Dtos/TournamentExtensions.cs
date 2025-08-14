using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Dtos;
public static class TournamentExtensions
{
    public static TournamentDto ToDto(this Tournament t)
    {
        if (t == null) return null!;

        return new TournamentDto
        {
            Id = t.Id,
            Name = t.Name,
            Location = t.Location,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            Description = t.Description,
            MinRankingFft = t.MinRankingFft,
            MaxRankingFft = t.MaxRankingFft,
            MinAge = t.MinAge,
            MaxAge = t.MaxAge,
            AgeCategory = t.AgeCategory,
            Type = t.Type,
            MatchFormatId = t.MatchFormatId,
            MatchFormat = t.MatchFormat == null ? null! : new MatchFormatDto
            {
                Id = t.MatchFormat.Id,
                Name = t.MatchFormat.Name
            },
            SubType = t.SubType,
            BallLevel = t.BallLevel,
            Surface = t.Surface,
            Condition = t.Condition,
            PrizeMoney = t.PrizeMoney,
            PrizeMoneyCurrency = t.PrizeMoneyCurrency,
            Matches = t.Matches.Select(m => new MatchDto
            {
                Id = m.Id,
                Player1FirstName = m.Player1.FirstName,
                Player1LastName = m.Player1.LastName,
                Player2FirstName = m.Player2.FirstName,
                Player2LastName = m.Player2.LastName,
                BestOfSets = m.BestOfSets,
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                Surface = m.Surface,
                WinnerFirstName = m.Winner?.FirstName,
                WinnerLastName = m.Winner?.LastName
            }).ToList() ?? []
        };
    }
}
