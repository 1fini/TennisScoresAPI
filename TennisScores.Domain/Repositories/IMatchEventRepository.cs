using TennisScores.Domain.Entities;
using TennisScores.Domain.Events;

namespace TennisScores.Domain.Repositories;

public interface IMatchEventRepository
{
    Task<IReadOnlyList<MatchEvent>> AppendAsync(
        Guid matchId,
        IReadOnlyCollection<IMatchDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchEvent>> GetByMatchIdAsync(
        Guid matchId,
        CancellationToken cancellationToken = default);
}
