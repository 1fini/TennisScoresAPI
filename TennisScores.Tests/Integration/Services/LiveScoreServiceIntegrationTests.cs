using TennisScores.API.Services;
using TennisScores.Infrastructure.Repositories;
using TennisScores.Infrastructure;
using TennisScores.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Moq;
using TennisScoresAPI.Hubs;
using Match = TennisScores.Domain.Entities.Match;

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

        var mockClients = new Mock<IHubClients>();
        var mockGroupManager = new Mock<IClientProxy>();
        var mockHubContext = new Mock<IHubContext<ScoreHub>>();

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockGroupManager.Object);
        mockHubContext.Setup(c => c.Clients).Returns(mockClients.Object);
        

        _liveScoreService = new LiveScoreService(
            _matchRepository,
            _matchFormatRepository,
            _unitOfWork,
            _setRepository,
            _gameRepository,
            _pointRepository,
            mockHubContext.Object);
    }

    #region Format 2
    // Straight game win
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

    // Game win with Advantage
    [Fact]
    public async Task AddPointToMatchAsync_Player1WinsGameAfterAd_GameIsCompleted()
    {
        // Arrange
        var carlos = _context.Players.SingleOrDefault(p => p.FirstName == "Carlos")!.Id;
        var jannik = _context.Players.SingleOrDefault(p => p.FirstName == "Jannik")!.Id;
        var tournament = _context.Tournaments.SingleOrDefault(t => t.Name == "US Open 2");

        var match = new Match
        {
            Player1Id = carlos,
            Player2Id = jannik,
            Sets = [],
            Tournament = tournament,
            TournamentId = tournament!.Id
        };

        _context.Matches.Add(match);
        await _unitOfWork.SaveChangesAsync();

        //Act
        for (int i = 0; i < 6; i++) // 40-40
        {
            var scorer = i % 2 == 0 ? carlos : jannik;
            await _liveScoreService.AddPointToMatchAsync(match.Id, scorer);
        }

        // Advantage Carlos
        await _liveScoreService.AddPointToMatchAsync(match.Id, carlos);

        // Deuce
        await _liveScoreService.AddPointToMatchAsync(match.Id, jannik);

        // Advantage Carlos
        await _liveScoreService.AddPointToMatchAsync(match.Id, carlos);
        // Game Carlos
        await _liveScoreService.AddPointToMatchAsync(match.Id, carlos);

        //Assert
        var updatedMatch = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        var currentSet = updatedMatch?.Sets.OrderBy(s => s.SetNumber).LastOrDefault();
        var previousCompletedGame = currentSet?.Games.SingleOrDefault(g => g.IsCompleted);
        var player1PointsWon = previousCompletedGame!.Points.Count(p => p.WinnerId == carlos);
        var player2PointsWon = previousCompletedGame!.Points.Count(p => p.WinnerId == jannik);

        Assert.NotNull(currentSet);
        Assert.NotNull(previousCompletedGame);
        Assert.True(previousCompletedGame!.IsCompleted, "Game should be completed");
        Assert.Equal(1, currentSet.Games.Count(g => g.IsCompleted && g.WinnerId == carlos));
        Assert.Equal(0, currentSet.Games.Count(g => g.IsCompleted && g.WinnerId == jannik));
        Assert.Single(updatedMatch!.Sets);
        Assert.Equal(carlos, previousCompletedGame.WinnerId);
        Assert.Equal(carlos, previousCompletedGame.Winner?.Id);
        Assert.Equal(6, player1PointsWon);
        Assert.Equal(4, player2PointsWon);
        Assert.False(currentSet.IsCompleted, "Set should not be completed");
        Assert.False(updatedMatch!.IsCompleted, "Match should not be over");
    }

    // Tiebreak is engaged + 1 point
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
        for (int i = 0; i < 6; i++)
        {
            await WinGameAsync(match.Id, player1);
            if ( i < 4)
            {
                await WinGameAsync(match.Id, player2);
            }
        }

        // --- Set 2 : 6–6 ---
        for (int i = 0; i < 6; i++)
        {
            await WinGameAsync(match.Id, player1);
            if ( i < 6)
            {
                await WinGameAsync(match.Id, player2);
            }
        }

        // --- Tie-break 12–10 Carlos ---
        for (int i = 0; i < 20; i++) // 10–10
        {
            var scorer = i % 2 == 0 ? player1 : player2;
            await _liveScoreService.AddPointToMatchAsync(match.Id, scorer);
        }

        // Carlos marque les 2 derniers points : 12–10
        await _liveScoreService.AddPointToMatchAsync(match.Id, player1);
        await _liveScoreService.AddPointToMatchAsync(match.Id, player1);

        // Assert
        var updatedMatch = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        var firstSetGamesWonByPlayer1 = updatedMatch!.Sets.First(s => s.SetNumber == 1).Games.Count(g => g.WinnerId == player1);
        var firstSetGamesWonByPlayer2 = updatedMatch!.Sets.First(s => s.SetNumber == 1).Games.Count(g => g.WinnerId == player2);
        var secondSetGamesWonByPlayer1 = updatedMatch!.Sets.First(s => s.SetNumber == 2).Games.Count(g => g.WinnerId == player1);
        var secondSetGamesWonByPlayer2 = updatedMatch!.Sets.First(s => s.SetNumber == 2).Games.Count(g => g.WinnerId == player2);
        var secondSet_ = updatedMatch!.Sets.Single(s => s.SetNumber == 2);
        var tieBreakGameSecondSet = secondSet_.Games.Single(g => g.IsTiebreak);
        var pointsWonInTiebreakByPlayer1 = tieBreakGameSecondSet.Points.Count(p => p.WinnerId == player1);
        var pointsWonInTiebreakByPlayer2 = tieBreakGameSecondSet.Points.Count(p => p.WinnerId == player2);

        Assert.NotNull(updatedMatch);
        Assert.Equal(2, updatedMatch.Sets.Count);
        Assert.Equal(6, firstSetGamesWonByPlayer1);
        Assert.Equal(4, firstSetGamesWonByPlayer2);
        Assert.Equal(7, secondSetGamesWonByPlayer1);
        Assert.Equal(6, secondSetGamesWonByPlayer2);
        Assert.Equal(13, tieBreakGameSecondSet.GameNumber);
        Assert.Equal(10, pointsWonInTiebreakByPlayer2);
        Assert.Equal(12, pointsWonInTiebreakByPlayer1);
        Assert.True(tieBreakGameSecondSet.IsTiebreak, "Last game should be a tiebreak");
        Assert.True(tieBreakGameSecondSet.IsCompleted, "Tiebreak game should be completed");
        Assert.Equal(player1, tieBreakGameSecondSet.WinnerId);
        Assert.True(updatedMatch!.IsCompleted, "Match should be completed");
        Assert.Equal(player1, updatedMatch.WinnerId);
    }

    // 3 sets win 6-4, 6-7, 7-6
    [Fact]
    public async Task AddPointToMatchAsync_3SetMatch_MatchCompleted()
    {
        // Arrange
        var player1 = _context.Players.Single(p => p.FirstName == "Carlos").Id;
        var player2 = _context.Players.Single(p => p.FirstName == "Jannik").Id;
        // Tournament attached to format 1
        var tournament = _context.Tournaments.SingleOrDefault(t => t.Name == "US Open 2");

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
        for (int i = 0; i < 6; i++)
        {
            await WinGameAsync(match.Id, player1);
            if ( i < 4)
            {
                await WinGameAsync(match.Id, player2);
            }
        }

        // --- Set 2 : 6–6 ---
        for (int i = 0; i < 6; i++)
        {
            await WinGameAsync(match.Id, player1);
            if ( i < 6)
            {
                await WinGameAsync(match.Id, player2);
            }
        }

        // --- Tie-break 12–10 ---
        for (int i = 0; i < 20; i++) // 10–10
        {
            var scorer = i % 2 == 0 ? player1 : player2;
            await _liveScoreService.AddPointToMatchAsync(match.Id, scorer);
        }

        // Jannik marque les 2 derniers points : 12–10
        await _liveScoreService.AddPointToMatchAsync(match.Id, player2);
        await _liveScoreService.AddPointToMatchAsync(match.Id, player2);

        //3rd Set
        // --- Set 2 : 7–6 ---
        for (int i = 0; i < 6; i++) // 6-6
        {
            await WinGameAsync(match.Id, player1);
            await WinGameAsync(match.Id, player2);
        }

        // --- Tie-break 7–0 Carlos ---
        for (int i = 0; i < 7; i++)
        {
            await _liveScoreService.AddPointToMatchAsync(match.Id, player1);
        }

        // Assert
        var updatedMatch = await _matchRepository.GetFullMatchByIdAsync(match.Id);
        var firstSetGamesWonByPlayer1 = updatedMatch!.Sets.First(s => s.SetNumber == 1).Games.Count(g => g.WinnerId == player1);
        var firstSetGamesWonByPlayer2 = updatedMatch!.Sets.First(s => s.SetNumber == 1).Games.Count(g => g.WinnerId == player2);
        var secondSetGamesWonByPlayer1 = updatedMatch!.Sets.First(s => s.SetNumber == 2).Games.Count(g => g.WinnerId == player1);
        var secondSetGamesWonByPlayer2 = updatedMatch!.Sets.First(s => s.SetNumber == 2).Games.Count(g => g.WinnerId == player2);
        var thirdSetGamesWonByPlayer1 = updatedMatch!.Sets.First(s => s.SetNumber == 3).Games.Count(g => g.WinnerId == player1);
        var thirdSetGamesWonByPlayer2 = updatedMatch!.Sets.First(s => s.SetNumber == 3).Games.Count(g => g.WinnerId == player2);
        var secondSet_ = updatedMatch!.Sets.Single(s => s.SetNumber == 2);
        var thirdSet_ = updatedMatch!.Sets.Single(s => s.SetNumber == 3);
        var tieBreakGameSecondSet = secondSet_.Games.Single(g => g.IsTiebreak);
        var tieBreakGame3Set = thirdSet_.Games.Single(g => g.IsTiebreak);
        var pointsWonInTiebreakByPlayer1 = tieBreakGameSecondSet.Points.Count(p => p.WinnerId == player1);
        var pointsWonInTiebreakByPlayer2 = tieBreakGameSecondSet.Points.Count(p => p.WinnerId == player2);
        var pointsWonInThirdSetTiebreakByPlayer1 = tieBreakGame3Set.Points.Count(p => p.WinnerId == player1);
        var pointsWonInThirdSetTiebreakByPlayer2 = tieBreakGame3Set.Points.Count(p => p.WinnerId == player2);

        Assert.NotNull(updatedMatch);
        Assert.Equal(3, updatedMatch.Sets.Count);
        Assert.Equal(6, firstSetGamesWonByPlayer1);
        Assert.Equal(4, firstSetGamesWonByPlayer2);
        Assert.Equal(6, secondSetGamesWonByPlayer1);
        Assert.Equal(7, secondSetGamesWonByPlayer2);
        Assert.Equal(13, tieBreakGameSecondSet.GameNumber);
        Assert.Equal(10, pointsWonInTiebreakByPlayer1);
        Assert.Equal(12, pointsWonInTiebreakByPlayer2);
        Assert.True(tieBreakGameSecondSet.IsTiebreak, "Last game should be a tiebreak");
        Assert.True(tieBreakGameSecondSet.IsCompleted, "Tiebreak game should be completed");
        Assert.Equal(player2, tieBreakGameSecondSet.WinnerId);
        Assert.Equal(7, thirdSetGamesWonByPlayer1);
        Assert.Equal(6, thirdSetGamesWonByPlayer2);
        Assert.Equal(7, pointsWonInThirdSetTiebreakByPlayer1);
        Assert.Equal(0, pointsWonInThirdSetTiebreakByPlayer2);
        Assert.Equal(player1, tieBreakGame3Set.WinnerId);
        Assert.Equal(player1, thirdSet_.WinnerId);
        Assert.True(updatedMatch!.IsCompleted, "Match should be completed");
        Assert.Equal(player1, updatedMatch.WinnerId);
    }
    #endregion Format 2

    private async Task WinGameAsync(Guid matchId, Guid playerId)
    {
        for (int i = 0; i < 4; i++)
        {
            await _liveScoreService.AddPointToMatchAsync(matchId, playerId);
        }
    }

}