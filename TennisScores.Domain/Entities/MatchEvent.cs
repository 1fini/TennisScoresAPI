namespace TennisScores.Domain.Entities;

public class MatchEvent
{
    private MatchEvent()
    {
    }

    public MatchEvent(
        Guid matchId,
        long sequence,
        string eventType,
        int eventVersion,
        string payload,
        DateTime occurredAt)
    {
        MatchId = matchId;
        Sequence = sequence;
        EventType = eventType;
        EventVersion = eventVersion;
        Payload = payload;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid MatchId { get; private set; }
    public Match Match { get; private set; } = null!;
    public long Sequence { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public int EventVersion { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }
}
