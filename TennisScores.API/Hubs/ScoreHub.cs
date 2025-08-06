using Microsoft.AspNetCore.SignalR;

namespace TennisScoresAPI.Hubs
{
    public class ScoreHub : Hub
    {
        public async Task SendPointUpdate(string matchId, object pointData)
        {
            await Clients.Group(matchId).SendAsync("ReceivePoint", pointData);
        }

        public override async Task OnConnectedAsync()
        {
            //TODO Log connections
            var matchId = Context.GetHttpContext()!.Request.Query["matchId"];
            await Groups.AddToGroupAsync(Context.ConnectionId, matchId!);
            await base.OnConnectedAsync();
        }
    }
}