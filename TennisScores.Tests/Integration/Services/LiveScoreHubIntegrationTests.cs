using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TennisScores.API.Services;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Enums;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Tests.Integration.Services;

public class LiveScoreHubIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public LiveScoreHubIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddPoint_EmitsPointAddedEvent()
    {
        // Créer client Http (pour API)
        var client = _factory.CreateClient();

        // Créer HubConnection avec WebApplicationFactory handler
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(client.BaseAddress!, "/liveScoreHub"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await hubConnection.StartAsync();

        Point? receivedPoint = null;
        var tcs = new TaskCompletionSource<Point>();

        hubConnection.On<Point>("PointAdded", point =>
        {
            receivedPoint = point;
            tcs.SetResult(point);
        });

        // Seed joueurs, match, etc. ici avec DbContext obtenu par scope DI
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TennisDbContext>();

        var player1 = new Player { FirstName = "Carlos", LastName = "Alcaraz" };
        var player2 = new Player { FirstName = "Jannik", LastName = "Sinner" };
        context.Players.AddRange(player1, player2);

        var tournament = new Tournament { Name = "Test Open", Location = "Paris", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(7), MatchFormatId = 1 };
        context.Tournaments.Add(tournament);

        await context.SaveChangesAsync();

        var match = new Match { Player1Id = player1.Id, Player2Id = player2.Id, TournamentId = tournament.Id, StartTime = DateTime.UtcNow };
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var liveScoreService = scope.ServiceProvider.GetRequiredService<ILiveScoreService>();

        // Appel de la méthode qui déclenche l'événement SignalR
        await liveScoreService.AddPointToMatchAsync(match.Id, player1.Id, PointType.Unknown);

        // Attente événement SignalR
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(3000));
        Assert.True(completed == tcs.Task, "PointAdded event not received.");

        Assert.NotNull(receivedPoint);
        Assert.Equal(player1.Id, receivedPoint!.WinnerId);

        await hubConnection.DisposeAsync();
    }
}
