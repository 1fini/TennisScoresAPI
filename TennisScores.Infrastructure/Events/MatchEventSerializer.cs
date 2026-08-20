using System.Text.Json;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Events;

namespace TennisScores.Infrastructure.Events;

public static class MatchEventSerializer
{
    public const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static MatchEvent Serialize(
        Guid matchId,
        long sequence,
        IMatchDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return new MatchEvent(
            matchId,
            sequence,
            GetEventType(domainEvent),
            CurrentVersion,
            JsonSerializer.Serialize(
                domainEvent,
                domainEvent.GetType(),
                SerializerOptions),
            domainEvent.OccurredAt);
    }

    public static IMatchDomainEvent Deserialize(MatchEvent matchEvent)
    {
        ArgumentNullException.ThrowIfNull(matchEvent);

        if (matchEvent.EventVersion != CurrentVersion)
        {
            throw new NotSupportedException(
                $"Unsupported match event version {matchEvent.EventVersion} for '{matchEvent.EventType}'.");
        }

        return matchEvent.EventType switch
        {
            "point-won" => Deserialize<PointWon>(matchEvent),
            "game-won" => Deserialize<GameWon>(matchEvent),
            "set-won" => Deserialize<SetWon>(matchEvent),
            "match-won" => Deserialize<MatchWon>(matchEvent),
            "server-changed" => Deserialize<ServerChanged>(matchEvent),
            _ => throw new NotSupportedException(
                $"Unsupported match event type '{matchEvent.EventType}'.")
        };
    }

    private static string GetEventType(IMatchDomainEvent domainEvent)
        => domainEvent switch
        {
            PointWon => "point-won",
            GameWon => "game-won",
            SetWon => "set-won",
            MatchWon => "match-won",
            ServerChanged => "server-changed",
            _ => throw new NotSupportedException(
                $"Unsupported match event type '{domainEvent.GetType().Name}'.")
        };

    private static TEvent Deserialize<TEvent>(MatchEvent matchEvent)
        where TEvent : IMatchDomainEvent
        => JsonSerializer.Deserialize<TEvent>(matchEvent.Payload, SerializerOptions) ??
            throw new InvalidOperationException(
                $"Match event '{matchEvent.Id}' has an invalid payload.");
}
