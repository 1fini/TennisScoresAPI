using TennisScores.Domain.Repositories;
using TennisScores.API.Services;
using TennisScores.Domain.Entities;

namespace TennisScores.Tests.Integration.Services;

public class LiveScoreServiceIntegrationTests
{
    // 🧪 Test d'intégration : simulation d’un match avec super tie-break
    [Fact]
    public async Task AddPointToMatchAsync_ShouldHandleSuperTiebreakCorrectly()
    {
        // Arrange
        var match = MatchFactory.CreateBestOf3MatchWith2SetsEachWon();
        var matchRepository = new InMemoryMatchRepository(match);
        var unitOfWork = new FakeUnitOfWork();
        var service = new LiveScoreService(unitOfWork, matchRepository);

        // Simuler 10 points gagnés par Player1 dans le 3e set super tiebreak
        for (int i = 0; i < 10; i++)
        {
            await service.AddPointToMatchAsync(match.Id, match.Player1Id);
        }

        // Act
        var updatedMatch = await matchRepository.GetFullMatchByIdAsync(match.Id);

        // Assert
        Assert.Equal(match.Player1Id, updatedMatch!.WinnerId);
        Assert.NotNull(updatedMatch.EndTime);
        Assert.Equal(3, updatedMatch.Sets.Count);
        Assert.True(updatedMatch.Sets.Last().Games.Single().IsTiebreak);
    }

    [Fact]
    public async Task AddPointToMatchAsync_ShouldWinMatchInStraightSets()
    {
        var p1 = new Player { Id = Guid.NewGuid(), FirstName = "A", LastName = "Player1" };
        var p2 = new Player { Id = Guid.NewGuid(), FirstName = "B", LastName = "Player2" };

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Player1Id = p1.Id,
            Player2Id = p2.Id,
            Player1 = p1,
            Player2 = p2,
            BestOfSets = 3
        };

        var matchRepository = new InMemoryMatchRepository(match);
        var unitOfWork = new FakeUnitOfWork();
        var service = new LiveScoreService(unitOfWork, matchRepository);

        // Set 1: Player1 gagne 6-2
        for (int i = 0; i < 6; i++) // 6 jeux pour P1
        {
            for (int p = 0; p < 4; p++) // chaque jeu = 4 points consécutifs
                await service.AddPointToMatchAsync(match.Id, p1.Id);
        }
        for (int i = 0; i < 2; i++) // 2 jeux pour P2
        {
            for (int p = 0; p < 4; p++)
                await service.AddPointToMatchAsync(match.Id, p2.Id);
        }

        // Set 2: Player1 gagne 6-4
        for (int i = 0; i < 6; i++) // 6 jeux pour P1
        {
            for (int p = 0; p < 4; p++)
                await service.AddPointToMatchAsync(match.Id, p1.Id);
        }
        for (int i = 0; i < 4; i++) // 4 jeux pour P2
        {
            for (int p = 0; p < 4; p++)
                await service.AddPointToMatchAsync(match.Id, p2.Id);
        }

        var updatedMatch = await matchRepository.GetFullMatchByIdAsync(match.Id);

        Assert.Equal(p1.Id, updatedMatch!.WinnerId);
        Assert.NotNull(updatedMatch.EndTime);
        Assert.Equal(2, updatedMatch.Sets.Count);
        Assert.Equal(p1.Id, updatedMatch.Sets.FirstOrDefault()!.WinnerId);
        Assert.Equal(p1.Id, updatedMatch.Sets.Skip(1).FirstOrDefault()!.WinnerId);
    }

    [Fact]
    public async Task AddPointToMatchAsync_ShouldHandleTiebreakSetVictory()
    {
        var p1 = new Player { Id = Guid.NewGuid(), FirstName = "A", LastName = "Player1" };
        var p2 = new Player { Id = Guid.NewGuid(), FirstName = "B", LastName = "Player2" };

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Player1Id = p1.Id,
            Player2Id = p2.Id,
            Player1 = p1,
            Player2 = p2,
            BestOfSets = 3
        };

        var repo = new InMemoryMatchRepository(match);
        var uow = new FakeUnitOfWork();
        var service = new LiveScoreService(uow, repo);

        // Set 1 : Player1 gagne 6–3
        for (int i = 0; i < 6; i++)
            for (int p = 0; p < 4; p++)
                await service.AddPointToMatchAsync(match.Id, p1.Id);

        for (int i = 0; i < 3; i++)
            for (int p = 0; p < 4; p++)
                await service.AddPointToMatchAsync(match.Id, p2.Id);

        // Set 2 : 6–6 → tie-break
        for (int i = 0; i < 6; i++)
            for (int p = 0; p < 4; p++)
                await service.AddPointToMatchAsync(match.Id, p1.Id);

        for (int i = 0; i < 6; i++)
            for (int p = 0; p < 4; p++)
                await service.AddPointToMatchAsync(match.Id, p2.Id);

        // Tie-break : Player1 gagne 7–5
        for (int i = 0; i < 7; i++)
            await service.AddPointToMatchAsync(match.Id, p1.Id);

        for (int i = 0; i < 5; i++)
            await service.AddPointToMatchAsync(match.Id, p2.Id);

        var updated = await repo.GetFullMatchByIdAsync(match.Id);

        Assert.Equal(p1.Id, updated!.WinnerId);
        Assert.NotNull(updated.EndTime);
        Assert.Equal(2, updated.Sets.Count);
        Assert.Equal(p1.Id, updated.Sets.FirstOrDefault()!.WinnerId);
        Assert.Equal(p1.Id, updated.Sets.Skip(1).FirstOrDefault()!.WinnerId);

        var tiebreakGame = updated.Sets.Skip(1).FirstOrDefault()!.Games.SingleOrDefault(g => g.IsTiebreak);
        Assert.NotNull(tiebreakGame);
        Assert.Equal(p1.Id, tiebreakGame.WinnerId);
    }

}