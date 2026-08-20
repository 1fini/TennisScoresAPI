using TennisScores.Domain.Enums;

namespace TennisScores.Domain.Events;

public interface IMatchDomainEvent
{
    DateTime OccurredAt { get; }
}

public sealed record PointWon(
    Guid WinnerId,
    Guid ServingPlayerId,
    PointType PointType,
    int SetNumber,
    int GameNumber,
    DateTime OccurredAt) : IMatchDomainEvent;

public sealed record PointUndone(
    Guid PointId,
    Guid WinnerId,
    PointType PointType,
    int SetNumber,
    int GameNumber,
    DateTime OriginalOccurredAt,
    DateTime OccurredAt) : IMatchDomainEvent;

public sealed record GameWon(
    Guid WinnerId,
    int SetNumber,
    int GameNumber,
    DateTime OccurredAt) : IMatchDomainEvent;

public sealed record SetWon(
    Guid WinnerId,
    int SetNumber,
    DateTime OccurredAt) : IMatchDomainEvent;

public sealed record MatchWon(
    Guid WinnerId,
    DateTime OccurredAt) : IMatchDomainEvent;

public sealed record ServerChanged(
    Guid PreviousServerId,
    Guid NewServerId,
    DateTime OccurredAt) : IMatchDomainEvent;
