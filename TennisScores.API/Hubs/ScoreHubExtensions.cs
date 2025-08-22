using Microsoft.AspNetCore.SignalR;
using TennisScores.Domain.Dtos;

namespace TennisScores.API.Hubs;
public static class HubContextExtensions
{
    public static async Task BroadcastPoint(this IHubContext<ScoreHub> hubContext, MatchDetailsDto match)
    {
        // Send to the tournament group
        await hubContext.Clients.Group($"Tournament-{match.TournamentId}")
            .SendAsync("ReceiveMatchUpdate", match);

        // Send to the match group
        await hubContext.Clients.Group($"Match-{match.Id}")
            .SendAsync("ReceiveMatchUpdate", match);
    }
}
