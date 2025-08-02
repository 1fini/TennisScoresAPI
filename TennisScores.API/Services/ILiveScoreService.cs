public interface ILiveScoreService
{
    Task<LiveScoreDto?> GetLiveScoreAsync(Guid matchId);
    Task<LiveScoreDto?> PointWonAsync(Guid matchId, Guid playerId);
}
