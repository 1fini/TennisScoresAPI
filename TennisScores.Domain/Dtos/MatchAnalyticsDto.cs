namespace TennisScores.Domain.Dtos;

public sealed record MatchAnalyticsDto(
    Guid MatchId,
    bool IsCompleted,
    bool ServiceContextAvailable,
    PlayerMatchAnalyticsDto Player1,
    PlayerMatchAnalyticsDto Player2);

public sealed record PlayerMatchAnalyticsDto(
    Guid PlayerId,
    string FirstName,
    string LastName,
    int TotalPointsWon,
    int Aces,
    int DoubleFaults,
    int Winners,
    int UnforcedErrors,
    int ForcedErrors,
    int? PointsServed,
    int? PointsReturned);
