using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TennisScores.API.Services;
using TennisScores.Domain;
using TennisScores.Domain.Dtos;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure;
using TennisScores.Infrastructure.Data;
using TennisScores.Infrastructure.Repositories;
using Xunit;

namespace TennisScores.Tests.Integration.Services;

public class MatchServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly MatchService _matchService;
    private readonly TennisDbContext _context;

    public MatchServiceTests(DatabaseFixture fixture)
    {
        _context = fixture.Context;

        IMatchRepository matchRepository = new MatchRepository(_context);
        IPlayerRepository playerRepository = new PlayerRepository(_context);
        ITournamentRepository tournamentRepository = new TournamentRepository(_context);
        ILogger<MatchService> logger = NullLogger<MatchService>.Instance;
        IUnitOfWork unitOfWork = new UnitOfWork(_context);

        _matchService = new MatchService(
            matchRepository,
            playerRepository,
            tournamentRepository,
            logger,
            unitOfWork);
    }
    private TennisDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TennisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TennisDbContext(options);
    }

    [Fact]
    public async Task GetMatchAsync_ShouldReturnNull_WhenMatchDoesNotExist()
    {
        // Arrange
        var matchId = Guid.NewGuid(); // ID inexistant

        // Act
        var result = await _matchService.GetMatchAsync(matchId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMatchAsync_ReturnsMatchDetails_WhenMatchExists()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var player1 = new Player { Id = Guid.NewGuid(), FirstName = "Roger", LastName = "Federer" };
        var player2 = new Player { Id = Guid.NewGuid(), FirstName = "Rafael", LastName = "Nadal" };

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Player1 = player1,
            Player2 = player2,
            Player1Id = player1.Id,
            Player2Id = player2.Id,
            BestOfSets = 3,
            StartTime = DateTime.UtcNow,
            Sets = new List<TennisSet>()
        };

        var set1 = new TennisSet
        {
            Id = Guid.NewGuid(),
            SetNumber = 1,
            Match = match,
            MatchId = match.Id,
            Games = new List<Game>()
        };

        var game1 = new Game
        {
            Id = Guid.NewGuid(),
            Set = set1,
            SetId = set1.Id,
            WinnerId = player1.Id
        };

        var game2 = new Game
        {
            Id = Guid.NewGuid(),
            Set = set1,
            SetId = set1.Id,
            WinnerId = player2.Id
        };

        var game3 = new Game
        {
            Id = Guid.NewGuid(),
            Set = set1,
            SetId = set1.Id,
            WinnerId = player1.Id
        };

        set1.Games.Add(game1);
        set1.Games.Add(game2);
        set1.Games.Add(game3);
        match.Sets.Add(set1);

        context.Players.AddRange(player1, player2);
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var matchRepo = new MatchRepository(context);
        var matchService = new MatchService(
            matchRepo,
            null!,
            null!,
            null!,
            null!); // autres deps null pour ce test

        // Act
        var result = await matchService.GetMatchAsync(match.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Roger", result.Player1.FirstName);
        Assert.Equal("Nadal", result.Player2.LastName);
        Assert.Single(result.Sets);
        Assert.Equal(2, result.Sets[0].Player1Games);
        Assert.Equal(1, result.Sets[0].Player2Games);
    }

    [Fact]
    public async Task CreateMatchAsync_ShouldCreatePlayersAndMatch_WhenPlayersDoNotExist()
    {
        // Arrange
        var player1 = _context.Players.FirstOrDefault(p => p.FirstName == "Carlos" && p.LastName == "Alcaraz");
        var player2 = _context.Players.FirstOrDefault(p => p.FirstName == "Jannik" && p.LastName == "Sinner");

        var tournament = _context.Tournaments.FirstOrDefault(t => t.Name == "US Open" && t.StartDate == new DateTime(2025, 8, 2));

        var request = new CreateMatchRequest
        {
            Player1Id = player1?.Id ?? Guid.NewGuid(),
            Player2Id = player2?.Id ?? Guid.NewGuid(),
            BestOfSets = 3,
            TournamentId = tournament?.Id,
            ServingPlayer = player1?.Id ?? Guid.NewGuid(),
        };

        // Act
        var matchId = await _matchService.CreateMatchAsync(request);

        // Assert
        var match = await _context.Matches.FindAsync(matchId.Id);
        Assert.NotNull(match);
        Assert.Equal(3, match.BestOfSets);

        Assert.NotNull(player1);
        Assert.NotNull(player2);
        Assert.Equal("Carlos", player1.FirstName);
        Assert.Equal("Alcaraz", player1.LastName);
        Assert.Equal("Jannik", player2.FirstName);
        Assert.Equal("Sinner", player2.LastName);

        Assert.NotNull(tournament);
        Assert.Equal("US Open", tournament.Name);
        Assert.Equal(new DateTime(2025, 8, 2), tournament.StartDate);
    }

    [Fact]
    public async Task CreateMatchAsync_ShouldThrow_WhenTournamentNotFound()
    {
        // Arrange
        var player1 = new Player { FirstName = "Aryna", LastName = "Sabalenka" };
        var player2 = new Player { FirstName = "Iga", LastName = "Swiatek" };
        _context.Players.AddRange(player1, player2);
        await _context.SaveChangesAsync();

        var tournamentId = Guid.NewGuid(); // Unknown tournament ID

        var request = new CreateMatchRequest
        {
            Player1Id = player1.Id,
            Player2Id = player2.Id,
            ServingPlayer = player1.Id,
            TournamentId = tournamentId,
        };

        // Act
        Func<Task> act = async () => await _matchService.CreateMatchAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Tournament with ID '{tournamentId}' not found.");  
    }
}
