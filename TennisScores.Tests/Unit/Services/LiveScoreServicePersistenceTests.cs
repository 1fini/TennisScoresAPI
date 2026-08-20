using Microsoft.AspNetCore.SignalR;
using Moq;
using TennisScores.API.Hubs;
using TennisScores.API.Services;
using TennisScores.Domain;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Enums;
using TennisScores.Domain.Events;
using TennisScores.Domain.Repositories;
using TennisScores.Domain.Scoring;
using MatchEntity = TennisScores.Domain.Entities.Match;

namespace TennisScores.Tests.Unit.Services;

public class LiveScoreServicePersistenceTests
{
    [Fact]
    public async Task AddPointToMatchAsync_AppendsThenSavesOnceThenBroadcasts()
    {
        var harness = CreateHarness(saveFails: false);

        await harness.Service.AddPointToMatchAsync(
            harness.Match.Id,
            harness.Match.Player1Id,
            PointType.Winner);

        Assert.Equal(
            new[] { "append", "save", "broadcast", "broadcast" },
            harness.Timeline);
        harness.UnitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        harness.MatchEventRepository.Verify(
            repository => repository.AppendAsync(
                harness.Match.Id,
                It.Is<IReadOnlyCollection<IMatchDomainEvent>>(events =>
                    events.Count == 1 && events.Single() is PointWon),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddPointToMatchAsync_WhenPersistenceFails_DoesNotBroadcast()
    {
        var harness = CreateHarness(saveFails: true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            harness.Service.AddPointToMatchAsync(
                harness.Match.Id,
                harness.Match.Player1Id,
                PointType.Winner));

        Assert.Equal("Persistence failed.", exception.Message);
        Assert.Equal(new[] { "append", "save" }, harness.Timeline);
        harness.ClientProxy.Verify(
            client => client.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Harness CreateHarness(bool saveFails)
    {
        var player1 = new Player
        {
            Id = Guid.NewGuid(),
            FirstName = "Carlos",
            LastName = "Alcaraz"
        };
        var player2 = new Player
        {
            Id = Guid.NewGuid(),
            FirstName = "Jannik",
            LastName = "Sinner"
        };
        var format = new MatchFormat
        {
            Id = 2,
            Name = "Format 2",
            SetsToWin = 2,
            GamesPerSet = 6,
            TieBreakEnabled = true,
            TieBreakPoints = 7,
            SuperTieBreakPoints = 10
        };
        var tournament = new Tournament
        {
            Id = Guid.NewGuid(),
            Name = "Persistence ordering tournament",
            Location = "Test",
            StartDate = DateTime.UtcNow.Date,
            MatchFormat = format,
            MatchFormatId = format.Id
        };
        var match = new MatchEntity
        {
            Id = Guid.NewGuid(),
            Player1Id = player1.Id,
            Player2Id = player2.Id,
            Player1 = player1,
            Player2 = player2,
            ServingPlayerId = player1.Id,
            Tournament = tournament,
            TournamentId = tournament.Id,
            Sets = []
        };

        var timeline = new List<string>();
        var matchRepository = new Mock<IMatchRepository>();
        matchRepository.Setup(repository => repository.GetFullMatchByIdAsync(match.Id))
            .ReturnsAsync(match);

        var setRepository = new Mock<ISetRepository>();
        setRepository.Setup(repository => repository.AddAsync(It.IsAny<TennisSet>()))
            .Returns(Task.CompletedTask);
        var gameRepository = new Mock<IGameRepository>();
        gameRepository.Setup(repository => repository.AddAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);
        var pointRepository = new Mock<IPointRepository>();
        pointRepository.Setup(repository => repository.AddAsync(It.IsAny<Point>()))
            .Returns(Task.CompletedTask);

        var matchEventRepository = new Mock<IMatchEventRepository>();
        matchEventRepository.Setup(repository => repository.AppendAsync(
                match.Id,
                It.IsAny<IReadOnlyCollection<IMatchDomainEvent>>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => timeline.Add("append"))
            .ReturnsAsync([]);

        var unitOfWork = new Mock<IUnitOfWork>();
        var saveSetup = unitOfWork.Setup(repository =>
            repository.SaveChangesAsync(It.IsAny<CancellationToken>()));
        saveSetup.Callback(() => timeline.Add("save"));
        if (saveFails)
        {
            saveSetup.ThrowsAsync(new InvalidOperationException("Persistence failed."));
        }
        else
        {
            saveSetup.ReturnsAsync(1);
        }

        var clientProxy = new Mock<IClientProxy>();
        clientProxy.Setup(client => client.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => timeline.Add("broadcast"))
            .Returns(Task.CompletedTask);
        var hubClients = new Mock<IHubClients>();
        hubClients.Setup(clients => clients.Group(It.IsAny<string>()))
            .Returns(clientProxy.Object);
        var hubContext = new Mock<IHubContext<ScoreHub>>();
        hubContext.SetupGet(context => context.Clients)
            .Returns(hubClients.Object);

        var service = new LiveScoreService(
            matchRepository.Object,
            unitOfWork.Object,
            setRepository.Object,
            gameRepository.Object,
            pointRepository.Object,
            matchEventRepository.Object,
            new ScoringEngine(),
            hubContext.Object);

        return new Harness(
            service,
            match,
            timeline,
            unitOfWork,
            matchEventRepository,
            clientProxy);
    }

    private sealed record Harness(
        LiveScoreService Service,
        MatchEntity Match,
        List<string> Timeline,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<IMatchEventRepository> MatchEventRepository,
        Mock<IClientProxy> ClientProxy);
}
