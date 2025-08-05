using TennisScores.Domain.Repositories;
using TennisScores.API.Services;
using TennisScores.Domain.Entities;
using TennisScores.Infrastructure.Repositories;
using TennisScores.Infrastructure;
using TennisScores.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using TennisScores.Domain;

namespace TennisScores.Tests.Integration.Services;

public class LiveScoreServiceIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly TennisDbContext _context;
    private readonly LiveScoreService _liveScoreService;
    private readonly MatchRepository _matchRepository;
    private readonly MatchFormatRepository _matchFormatRepository;
    private readonly SetRepository _setRepository;
    private readonly UnitOfWork _unitOfWork;

    public LiveScoreServiceIntegrationTests(DatabaseFixture fixture)
    {
        _context = fixture.Context;
        _matchRepository = new MatchRepository(_context);
        _matchFormatRepository = new MatchFormatRepository(_context);
        _setRepository = new SetRepository(_context);
        _unitOfWork = new UnitOfWork(_context);

        _liveScoreService = new LiveScoreService(
            (IMatchRepository)_matchRepository,
            (IMatchFormatRepository)_matchFormatRepository,
            (IUnitOfWork)_unitOfWork,
            (ISetRepository)_setRepository);
    }
    // 🧪 Test d'intégration : simulation d’un match avec super tie-break
    [Fact]
    public async Task AddPointToMatchAsync_Player1WinsGame_GameIsCompleted()
    {
        Console.WriteLine("===============Test Started==========");
        // Arrange
        var carlos = _context.Players.SingleOrDefault(p => p.FirstName == "Carlos")!.Id;
        var jannik = _context.Players.SingleOrDefault(p => p.FirstName == "Jannik")!.Id;
        var match = new Match
        {
            Player1Id = carlos,
            Player2Id = jannik,
            Sets = [],
            Tournament = _context.Tournaments.First(),
            TournamentId = _context.Tournaments.First()!.Id
        };

        _context.Matches.Add(match);
        await _unitOfWork.SaveChangesAsync();

        //Act
        for (int i = 0; i < 4; i++)
        {
            await _liveScoreService.AddPointToMatchAsync(match.Id, carlos);
        }

        //Assert
        var updatedMatch = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        var currentSet = updatedMatch?.Sets.OrderBy(s => s.SetNumber).LastOrDefault();
        var currentGame = currentSet?.Games.OrderByDescending(g => g.Id).FirstOrDefault();

        Assert.NotNull(currentSet);
        Assert.NotNull(currentGame);
        Assert.True(currentGame.IsCompleted, "Game should be completed");
        Assert.False(currentSet.IsCompleted, "Set should not be completed");
        Assert.False(updatedMatch!.IsCompleted, "Match should not be over");
    }
}