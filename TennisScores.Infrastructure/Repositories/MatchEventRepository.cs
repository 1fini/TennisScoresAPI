using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Events;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Data;
using TennisScores.Infrastructure.Events;

namespace TennisScores.Infrastructure.Repositories;

public sealed class MatchEventRepository(TennisDbContext context) : IMatchEventRepository
{
    private readonly TennisDbContext _context = context;

    public async Task<IReadOnlyList<MatchEvent>> AppendAsync(
        Guid matchId,
        IReadOnlyCollection<IMatchDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        if (domainEvents.Count == 0)
            return [];

        var persistedSequence = await _context.MatchEvents
            .Where(matchEvent => matchEvent.MatchId == matchId)
            .Select(matchEvent => (long?)matchEvent.Sequence)
            .MaxAsync(cancellationToken) ?? 0;
        var pendingSequence = _context.MatchEvents.Local
            .Where(matchEvent => matchEvent.MatchId == matchId)
            .Select(matchEvent => matchEvent.Sequence)
            .DefaultIfEmpty(0)
            .Max();
        var nextSequence = Math.Max(persistedSequence, pendingSequence) + 1;

        var matchEvents = domainEvents
            .Select((domainEvent, index) => MatchEventSerializer.Serialize(
                matchId,
                nextSequence + index,
                domainEvent))
            .ToList();

        await _context.MatchEvents.AddRangeAsync(matchEvents, cancellationToken);
        return matchEvents;
    }

    public async Task<IReadOnlyList<MatchEvent>> GetByMatchIdAsync(
        Guid matchId,
        CancellationToken cancellationToken = default)
        => await _context.MatchEvents
            .AsNoTracking()
            .Where(matchEvent => matchEvent.MatchId == matchId)
            .OrderBy(matchEvent => matchEvent.Sequence)
            .ToListAsync(cancellationToken);
}
