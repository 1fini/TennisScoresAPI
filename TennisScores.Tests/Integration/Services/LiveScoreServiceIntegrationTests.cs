using TennisScores.Domain.Repositories;
using TennisScores.API.Services;
using TennisScores.Domain.Entities;
using TennisScores.Infrastructure.Repositories;
using TennisScores.Infrastructure;
using TennisScores.Infrastructure.Data;
using TennisScores.Domain;

namespace TennisScores.Tests.Integration.Services;

public class LiveScoreServiceIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly TennisDbContext _context;
    private readonly LiveScoreService _liveScoreService;
    private readonly MatchRepository _matchRepository;
    private readonly MatchFormatRepository _matchFormatRepository;
    private readonly GameRepository _gameRepository;
    private readonly PointRepository _pointRepository;
    private readonly SetRepository _setRepository;
    private readonly UnitOfWork _unitOfWork;

    public LiveScoreServiceIntegrationTests(DatabaseFixture fixture)
    {
        _context = fixture.Context;
        _matchRepository = new MatchRepository(_context);
        _matchFormatRepository = new MatchFormatRepository(_context);
        _setRepository = new SetRepository(_context);
        _gameRepository = new GameRepository(_context);
        _pointRepository = new PointRepository(_context);
        _unitOfWork = new UnitOfWork(_context);

        _liveScoreService = new LiveScoreService(
            _matchRepository,
            _matchFormatRepository,
            _unitOfWork,
            _setRepository,
            _gameRepository,
            _pointRepository);
    }
    // 🧪 Test d'intégration : simulation d’un match avec super tie-break
    [Fact]
    public async Task AddPointToMatchAsync_Player1WinsGame_GameIsCompleted()
    {
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
        // We look for the last completed game since the methode AddPointToMatchAsync ensures
        // that a new game is created if the current one is completed.
        var previousCompletedGame = currentSet?.Games.SingleOrDefault(g => g.IsCompleted && g.GameNumber == currentSet.Games.Count - 1);

        Assert.NotNull(currentSet);
        Assert.NotNull(previousCompletedGame);
        Assert.True(previousCompletedGame!.IsCompleted, "Game should be completed");
        Assert.Equal(1, currentSet.Games.Count(g => g.IsCompleted && g.WinnerId == carlos));
        Assert.Equal(0, currentSet.Games.Count(g => g.IsCompleted && g.WinnerId == jannik));
        Assert.Single(updatedMatch!.Sets);
        Assert.Equal(carlos, previousCompletedGame.WinnerId);
        Assert.Equal(carlos, previousCompletedGame.Winner?.Id);
        Assert.False(currentSet.IsCompleted, "Set should not be completed");
        Assert.False(updatedMatch!.IsCompleted, "Match should not be over");
    }

    // 🧪 Test d'intégration : simulation d’un match avec tie-break
    [Fact]
    public async Task AddPointToMatchAsync_TieBreakTriggeredAt6_6_GameIsTieBreak()
    {
        // Arrange
        var player1Id = _context.Players.Single(p => p.FirstName == "Carlos").Id;
        var player2Id = _context.Players.Single(p => p.FirstName == "Jannik").Id;
        var tournament = _context.Tournaments.First(t => t.MatchFormat.TieBreakEnabled && t.MatchFormat.GamesPerSet == 6);

        var match = new Match
        {
            Player1Id = player1Id,
            Player2Id = player2Id,
            Tournament = tournament,
            TournamentId = tournament.Id,
            Sets = []
        };

        _context.Matches.Add(match);
        await _unitOfWork.SaveChangesAsync();

        // Simulate 12 games : 6–6
        for (int i = 0; i < 6; i++)
        {
            for (int p = 0; p < 4; p++)
                await _liveScoreService.AddPointToMatchAsync(match.Id, player1Id); // 6 games Carlos

            for (int p = 0; p < 4; p++)
                await _liveScoreService.AddPointToMatchAsync(match.Id, player2Id); // 6 games Jannik
        }

        // One point in tie-break for player 1
        await _liveScoreService.AddPointToMatchAsync(match.Id, player1Id);

        // Assert
        var updatedMatch = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        // Could be a method in _gameRepository
        var firstSetGamesWonByPlayer1 = updatedMatch!.Sets.First(s => s.SetNumber == 1).Games.Count(g => g.WinnerId == player1Id);
        var firstSetGamesWonByPlayer2 = updatedMatch!.Sets.First(s => s.SetNumber == 1).Games.Count(g => g.WinnerId == player2Id);
        var currentSet = updatedMatch!.Sets.Single();
        var currentGame = currentSet.Games.Last();

        Assert.True(currentGame.IsTiebreak, "The game should be a tie-break");
        Assert.Equal(6, firstSetGamesWonByPlayer1);
        Assert.Equal(6, firstSetGamesWonByPlayer2);
        Assert.Equal(1, currentSet.SetNumber);
        Assert.Equal(1, currentSet.Games.Count(g => g.IsTiebreak));
        // Points in the tie-break
        Assert.Equal(1, currentGame.Points.Count(p => p.WinnerId == player1Id));
        Assert.Equal(0, currentGame.Points.Count(p => p.WinnerId == player2Id));
        Assert.False(currentSet.IsCompleted, "Set should not be completed");
        Assert.False(updatedMatch.IsCompleted, "Match should not be over");
    }

    // 🧪 Test d'intégration : simulation d’un match avec tie-break dans le second set
    // 2 sets (6–4, 7–6)
    [Fact]
    public async Task AddPointToMatchAsync_TwoSetMatch_TiebreakEnds_12_10_MatchIsCompleted()
    {
        // Arrange
        var player1 = _context.Players.Single(p => p.FirstName == "Carlos").Id;
        var player2 = _context.Players.Single(p => p.FirstName == "Jannik").Id;

        var match = new Match
        {
            Player1Id = player1,
            Player2Id = player2,
            Sets = [],
            Tournament = _context.Tournaments.First(t => t.MatchFormat.SetsToWin == 2 && t.MatchFormat.GamesPerSet == 6),
            TournamentId = _context.Tournaments.First().Id
        };

        _context.Matches.Add(match);
        await _unitOfWork.SaveChangesAsync();

        // Act

        // --- Set 1 : 6–4 Carlos ---
        for (int i = 0; i < 6; i++) await WinGameAsync(match.Id, player1);
        for (int i = 0; i < 4; i++) await WinGameAsync(match.Id, player2);

        // --- Set 2 : 6–6 ---
        for (int i = 0; i < 6; i++) await WinGameAsync(match.Id, player1);
        for (int i = 0; i < 6; i++) await WinGameAsync(match.Id, player2);

        // --- Tie-break 12–10 Carlos ---
        for (int i = 0; i < 10; i++) // 10–10
        {
            var scorer = i % 2 == 0 ? player1 : player2;
            await _liveScoreService.AddPointToMatchAsync(match.Id, scorer);
        }

        // Carlos marque les 2 derniers points : 12–10
        await _liveScoreService.AddPointToMatchAsync(match.Id, player1);
        await _liveScoreService.AddPointToMatchAsync(match.Id, player1);

        // Assert
        var updatedMatch = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        Assert.NotNull(updatedMatch);
        Assert.True(updatedMatch!.IsCompleted, "Match should be completed");
        Assert.Equal(player1, updatedMatch.WinnerId);

        Assert.Equal(2, updatedMatch.Sets.Count);

        var secondSet = updatedMatch.Sets.OrderBy(s => s.SetNumber).Last();
        var tiebreakGame = secondSet.Games.Last();

        Assert.True(tiebreakGame.IsTiebreak, "Last game should be a tiebreak");
        Assert.True(tiebreakGame.IsCompleted, "Tiebreak game should be completed");
        Assert.Equal(player1, tiebreakGame.WinnerId);
    }


    private async Task WinGameAsync(Guid matchId, Guid playerId)
    {
        for (int i = 0; i < 4; i++)
        {
            await _liveScoreService.AddPointToMatchAsync(matchId, playerId);
        }
    }


}