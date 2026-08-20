using System.Collections.Immutable;
using TennisScores.Domain.Enums;
using TennisScores.Domain.Events;

namespace TennisScores.Domain.Scoring;

public sealed record ScoringFormat(
    int SetsToWin,
    int GamesPerSet,
    bool TieBreakEnabled,
    bool DecidingPointEnabled,
    bool SuperTieBreakForFinalSet,
    int TieBreakPoints,
    int SuperTieBreakPoints)
{
    public int BestOfSets => (SetsToWin * 2) - 1;
}

public sealed record ScoredPoint(
    Guid WinnerId,
    PointType PointType,
    DateTime OccurredAt);

public sealed record ScoringGame(
    int GameNumber,
    bool IsTiebreak,
    bool IsCompleted,
    Guid? WinnerId,
    ImmutableArray<ScoredPoint> Points);

public sealed record ScoringSet(
    int SetNumber,
    bool IsCompleted,
    Guid? WinnerId,
    ImmutableArray<ScoringGame> Games);

public sealed record ScoringState(
    Guid Player1Id,
    Guid Player2Id,
    Guid ServingPlayerId,
    bool IsCompleted,
    Guid? WinnerId,
    DateTime? EndTime,
    ScoringFormat Format,
    ImmutableArray<ScoringSet> Sets);

public sealed record AwardPoint(
    Guid WinnerId,
    PointType PointType,
    DateTime OccurredAt);

public sealed record ScoringResult(
    ScoringState State,
    int AppliedSetNumber,
    int AppliedGameNumber,
    ImmutableArray<IMatchDomainEvent> Events);
