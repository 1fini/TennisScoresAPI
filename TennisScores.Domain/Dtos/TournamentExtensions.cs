using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Dtos;

public static class TournamentExtensions
{
/*     public static TournamentDto ToDto(this Tournament t)
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
            Matches = t.Matches.Select(m => m.MapToMatchDto()).ToList() ?? []
        };
    } */
    
    public static TournamentDto ToDetailedDto(this Tournament t)
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
            Matches = t.Matches.Select(m => m.MapToFullDto()).ToList() ?? []
        };
    }
}
