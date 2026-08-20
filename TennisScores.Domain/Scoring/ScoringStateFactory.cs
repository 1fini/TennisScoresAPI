using System.Collections.Immutable;
using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Scoring;

public static class ScoringStateFactory
{
    public static ScoringState FromMatch(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        var format = match.Tournament?.MatchFormat ??
            throw new ArgumentException("Match Format not found", nameof(match));

        return new ScoringState(
            match.Player1Id,
            match.Player2Id,
            match.ServingPlayerId,
            match.IsCompleted,
            match.WinnerId,
            match.EndTime,
            new ScoringFormat(
                format.SetsToWin,
                format.GamesPerSet,
                format.TieBreakEnabled,
                format.DecidingPointEnabled,
                format.SuperTieBreakForFinalSet,
                format.TieBreakPoints,
                format.SuperTieBreakPoints),
            match.Sets
                .OrderBy(set => set.SetNumber)
                .Select(set => new ScoringSet(
                    set.SetNumber,
                    set.IsCompleted,
                    set.WinnerId,
                    set.Games
                        .OrderBy(game => game.GameNumber)
                        .Select(game => new ScoringGame(
                            game.GameNumber,
                            game.IsTiebreak,
                            game.IsCompleted,
                            game.WinnerId,
                            game.Points
                                .OrderBy(point => point.Timestamp)
                                .ThenBy(point => point.Id)
                                .Select(point => new ScoredPoint(
                                    point.WinnerId ?? Guid.Empty,
                                    point.PointType,
                                    point.Timestamp))
                                .ToImmutableArray()))
                        .ToImmutableArray()))
                .ToImmutableArray());
    }
}
