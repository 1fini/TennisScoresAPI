using Microsoft.AspNetCore.SignalR;
using Moq;
using TennisScores.API.Hubs;
using TennisScores.API.Services;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Enums;
using TennisScores.Infrastructure;
using TennisScores.Infrastructure.Data;
using TennisScores.Infrastructure.Repositories;
using Match = TennisScores.Domain.Entities.Match;

namespace TennisScores.Tests.Integration.Services;

public class LiveScoreCharacterizationTests : IClassFixture<DatabaseFixture>
{
    private readonly TennisDbContext _context;
    private readonly MatchRepository _matchRepository;
    private readonly UnitOfWork _unitOfWork;
    private readonly LiveScoreService _liveScoreService;

    public LiveScoreCharacterizationTests(DatabaseFixture fixture)
    {
        _context = fixture.Context;

        _matchRepository = new MatchRepository(_context);
        var matchFormatRepository = new MatchFormatRepository(_context);
        var setRepository = new SetRepository(_context);
        var gameRepository = new GameRepository(_context);
        var pointRepository = new PointRepository(_context);
        _unitOfWork = new UnitOfWork(_context);

        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var mockHubContext = new Mock<IHubContext<ScoreHub>>();

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockHubContext.Setup(c => c.Clients).Returns(mockClients.Object);

        _liveScoreService = new LiveScoreService(
            _matchRepository,
            matchFormatRepository,
            _unitOfWork,
            setRepository,
            gameRepository,
            pointRepository,
            mockHubContext.Object);
    }

    [Fact]
    public async Task AddPointToMatchAsync_DecidingPointEnabled_GameEndsOnFourthPointAtThreeAll()
    {
        var (player1, player2) = GetPlayers();
        var match = await CreateMatchAsync(formatId: 3, player1, player2, player1, "No-ad characterization");

        await PlayPointAsync(match.Id, player1);
        await PlayPointAsync(match.Id, player2);
        await PlayPointAsync(match.Id, player1);
        await PlayPointAsync(match.Id, player2);
        await PlayPointAsync(match.Id, player1);
        await PlayPointAsync(match.Id, player2);

        var atThreeAll = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        var gameAtThreeAll = atThreeAll!.Sets.Single().Games.Single(g => !g.IsCompleted);
        Assert.Equal(3, gameAtThreeAll.Points.Count(p => p.WinnerId == player1));
        Assert.Equal(3, gameAtThreeAll.Points.Count(p => p.WinnerId == player2));

        await PlayPointAsync(match.Id, player2);

        var updated = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        var firstSet = updated!.Sets.Single();
        var completedGame = firstSet.Games.Single(g => g.IsCompleted);

        Assert.Equal(player2, completedGame.WinnerId);
        Assert.Equal(3, completedGame.Points.Count(p => p.WinnerId == player1));
        Assert.Equal(4, completedGame.Points.Count(p => p.WinnerId == player2));
        Assert.Single(firstSet.Games.Where(g => !g.IsCompleted));
    }

    [Fact]
    public async Task AddPointToMatchAsync_NormalGameCompletion_SwitchesServerExactlyOnce()
    {
        var (player1, player2) = GetPlayers();
        var match = await CreateMatchAsync(formatId: 2, player1, player2, player1, "Server switch characterization");

        await WinGameAsync(match.Id, player1);

        var afterGame = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        Assert.Equal(player2, afterGame!.ServingPlayerId);

        await PlayPointAsync(match.Id, player2);

        var afterNextPoint = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        Assert.Equal(player2, afterNextPoint!.ServingPlayerId);
    }

    [Fact]
    public async Task AddPointToMatchAsync_SetBoundary_PreservesServerSelectedAfterCompletedGame()
    {
        var (player1, player2) = GetPlayers();
        var match = await CreateMatchAsync(formatId: 2, player1, player2, player1, "Set boundary server characterization");

        for (var i = 0; i < 6; i++)
        {
            await WinGameAsync(match.Id, player1);
        }

        var afterSet = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        var firstSet = afterSet!.Sets.Single(s => s.SetNumber == 1);
        var secondSet = afterSet.Sets.Single(s => s.SetNumber == 2);

        Assert.True(firstSet.IsCompleted);
        Assert.False(secondSet.IsCompleted);
        Assert.Equal(player1, afterSet.ServingPlayerId);

        await PlayPointAsync(match.Id, player2);

        var afterFirstPointOfSecondSet = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        Assert.Equal(player1, afterFirstPointOfSecondSet!.ServingPlayerId);
    }

    [Fact]
    public async Task AddPointToMatchAsync_MatchWinningPoint_DoesNotCreateExtraSetOrGame()
    {
        var (player1, player2) = GetPlayers();
        var match = await CreateMatchAsync(formatId: 2, player1, player2, player1, "Match completion characterization");

        for (var set = 0; set < 2; set++)
        {
            for (var game = 0; game < 6; game++)
            {
                await WinGameAsync(match.Id, player1);
            }
        }

        var completed = await _matchRepository.GetFullMatchByIdAsync(match.Id);

        Assert.True(completed!.IsCompleted);
        Assert.Equal(player1, completed.WinnerId);
        Assert.Equal(2, completed.Sets.Count);
        Assert.All(completed.Sets, set => Assert.True(set.IsCompleted));
        Assert.DoesNotContain(completed.Sets.SelectMany(set => set.Games), game => !game.IsCompleted);

        var setCount = completed.Sets.Count;
        var gameCount = completed.Sets.Sum(set => set.Games.Count);
        var pointCount = completed.Sets.Sum(set => set.Games.Sum(game => game.Points.Count));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _liveScoreService.AddPointToMatchAsync(match.Id, player2, PointType.Winner));

        var afterRejectedPoint = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        Assert.Equal(setCount, afterRejectedPoint!.Sets.Count);
        Assert.Equal(gameCount, afterRejectedPoint.Sets.Sum(set => set.Games.Count));
        Assert.Equal(pointCount, afterRejectedPoint.Sets.Sum(set => set.Games.Sum(game => game.Points.Count)));
    }

    private (Guid Player1, Guid Player2) GetPlayers()
    {
        var player1 = _context.Players.Single(p => p.FirstName == "Carlos").Id;
        var player2 = _context.Players.Single(p => p.FirstName == "Jannik").Id;
        return (player1, player2);
    }

    private async Task<Match> CreateMatchAsync(
        int formatId,
        Guid player1,
        Guid player2,
        Guid servingPlayer,
        string tournamentName)
    {
        var format = _context.MatchFormats.Single(f => f.Id == formatId);
        var tournament = new Tournament
        {
            Name = tournamentName,
            StartDate = DateTime.UtcNow.Date,
            Location = "Test",
            MatchFormat = format,
            MatchFormatId = format.Id
        };

        var match = new Match
        {
            Player1Id = player1,
            Player2Id = player2,
            ServingPlayerId = servingPlayer,
            Sets = [],
            Tournament = tournament,
            TournamentId = tournament.Id
        };

        _context.Tournaments.Add(tournament);
        _context.Matches.Add(match);
        await _unitOfWork.SaveChangesAsync();
        return match;
    }

    private Task PlayPointAsync(Guid matchId, Guid winnerId)
        => _liveScoreService.AddPointToMatchAsync(matchId, winnerId, PointType.Winner);

    private async Task WinGameAsync(Guid matchId, Guid winnerId)
    {
        for (var point = 0; point < 4; point++)
        {
            await PlayPointAsync(matchId, winnerId);
        }
    }
}
