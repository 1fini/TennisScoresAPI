using Microsoft.AspNetCore.SignalR;
using TennisScores.Domain.Dtos;

namespace TennisScores.API.Hubs
{
    public class ScoreHub : Hub
    {
        // Join a tournament group (to receive updates for this tournament)
        // This allows clients to subscribe to tournament updates
        // and receive notifications about matches, scores, etc.
        // This is useful for TournamentDetails page.
        public async Task JoinTournamentGroup(Guid tournamentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Tournament-{tournamentId}");
        }

        /// <summary>
        /// Leave a tournament group.
        /// This allows clients to unsubscribe from tournament updates.
        /// This is useful for TournamentDetails page.
        /// </summary>
        /// <param name="tournamentId"></param>
        /// <returns></returns>
        public async Task LeaveTournamentGroup(Guid tournamentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Tournament-{tournamentId}");
        }

        /// <summary>
        /// Join a match group.
        /// This allows clients to subscribe to match updates
        /// and receive notifications about points, sets, etc.
        /// This is useful for MatchDetails page.
        /// Clients can join a match group to receive real-time updates
        /// about the match they are interested in.
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public async Task JoinMatchGroup(Guid matchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Match-{matchId}");
        }

        /// <summary>
        /// Leave a match group.
        /// This allows clients to unsubscribe from match updates.
        /// Clients can leave a match group when they are no longer interested
        /// in receiving updates about that match.
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        public async Task LeaveMatchGroup(Guid matchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Match-{matchId}");
        }

        public override async Task OnConnectedAsync()
        {
            var matchId = Context.GetHttpContext()!.Request.Query["matchId"];
            var tournamentId = Context.GetHttpContext()!.Request.Query["tournamentId"];

            if (!string.IsNullOrEmpty(matchId))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Match-{matchId}");

            if (!string.IsNullOrEmpty(tournamentId))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Tournament-{tournamentId}");

            await base.OnConnectedAsync();
        }
    }
}
