using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Dtos;

public static class TournamentExtensions
{
    public static TournamentDto ToListDto(this Tournament t)
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
            MatchFormat = t.MatchFormat == null ? null! : t.MatchFormat.ToDto(),
            SubType = t.SubType,
            BallLevel = t.BallLevel,
            Surface = t.Surface,
            Condition = t.Condition,
            PrizeMoney = t.PrizeMoney,
            PrizeMoneyCurrency = t.PrizeMoneyCurrency
        };
    }
    
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
            MatchFormat = t.MatchFormat == null ? null! : t.MatchFormat.ToDto(),
            SubType = t.SubType,
            BallLevel = t.BallLevel,
            Surface = t.Surface,
            Condition = t.Condition,
            PrizeMoney = t.PrizeMoney,
            PrizeMoneyCurrency = t.PrizeMoneyCurrency,
            Matches = t.Matches.Select(m => m.MapToFullDto()).ToList() ?? []
        };
    }

    private static MatchFormatDto ToDto(this MatchFormat format)
        => new()
        {
            Id = format.Id,
            Name = format.Name,
            SetsToWin = format.SetsToWin,
            GamesPerSet = format.GamesPerSet,
            TieBreakEnabled = format.TieBreakEnabled,
            DecidingPointEnabled = format.DecidingPointEnabled,
            SuperTieBreakForFinalSet = format.SuperTieBreakForFinalSet,
            Application = format.Application
        };
}
