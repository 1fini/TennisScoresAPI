using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Enums;
using TennisScores.Domain.Events;
using TennisScores.Infrastructure.Data;
using TennisScores.Infrastructure.Events;
using TennisScores.Infrastructure.Repositories;

namespace TennisScores.Tests.Integration.Repositories;

public class MatchEventRepositoryTests
{
    [Fact]
    public async Task AppendAsync_AssignsMonotonicSequenceAcrossPendingAndSavedBatches()
    {
        await using var context = CreateContext();
        var repository = new MatchEventRepository(context);
        var matchId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var occurredAt = new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc);

        var firstBatch = await repository.AppendAsync(
            matchId,
            new IMatchDomainEvent[]
            {
                new PointWon(
                    player1Id,
                    player1Id,
                    PointType.Ace,
                    SetNumber: 1,
                    GameNumber: 1,
                    occurredAt),
                new ServerChanged(player1Id, player2Id, occurredAt)
            });
        var secondBatch = await repository.AppendAsync(
            matchId,
            new IMatchDomainEvent[]
            {
                new PointWon(
                    player2Id,
                    player2Id,
                    PointType.Winner,
                    SetNumber: 1,
                    GameNumber: 1,
                    occurredAt.AddSeconds(1))
            });
        await context.SaveChangesAsync();

        var storedEvents = await repository.GetByMatchIdAsync(matchId);

        Assert.Equal(new long[] { 1, 2 }, firstBatch.Select(matchEvent => matchEvent.Sequence));
        Assert.Equal(3, Assert.Single(secondBatch).Sequence);
        Assert.Equal(new long[] { 1, 2, 3 }, storedEvents.Select(matchEvent => matchEvent.Sequence));
        Assert.Equal(3, storedEvents.Select(matchEvent => matchEvent.Id).Distinct().Count());
    }

    [Fact]
    public async Task AppendAsync_PersistsStableTypeVersionAndRoundTripsPayload()
    {
        await using var context = CreateContext();
        var repository = new MatchEventRepository(context);
        var matchId = Guid.NewGuid();
        var winnerId = Guid.NewGuid();
        var serverId = Guid.NewGuid();
        var occurredAt = new DateTime(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc);
        var domainEvent = new PointWon(
            winnerId,
            serverId,
            PointType.Ace,
            SetNumber: 2,
            GameNumber: 4,
            occurredAt);

        var storedEvent = Assert.Single(await repository.AppendAsync(
            matchId,
            new IMatchDomainEvent[] { domainEvent }));
        await context.SaveChangesAsync();

        Assert.Equal("point-won", storedEvent.EventType);
        Assert.Equal(1, storedEvent.EventVersion);
        Assert.Equal(matchId, storedEvent.MatchId);
        Assert.Equal(occurredAt, storedEvent.OccurredAt);
        Assert.Equal(domainEvent, Assert.IsType<PointWon>(
            MatchEventSerializer.Deserialize(storedEvent)));
    }

    private static TennisDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TennisDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TennisDbContext(options);
    }
}
