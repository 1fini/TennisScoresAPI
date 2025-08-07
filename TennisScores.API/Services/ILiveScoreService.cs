using System.Drawing;
using TennisScores.Domain.Enums;

namespace TennisScores.API.Services;

/// <summary>
/// Interface for live score services.
/// </summary>
public interface ILiveScoreService
{
    //Task<LiveScoreDto?> GetLiveScoreAsync(Guid matchId);
    //Task<LiveScoreDto?> PointWonAsync(Guid matchId, Guid playerId);
    Task AddPointToMatchAsync(Guid matchId, Guid winnerId, PointType pointType);
    /*Task<LiveScoreDto?> StartMatchAsync(Guid matchId, Guid player1Id, Guid player2Id);
    Task<LiveScoreDto?> StartSetAsync(Guid matchId, Guid player1Id, Guid player2Id);
    Task<LiveScoreDto?> StartGameAsync(Guid matchId, Guid player1Id, Guid player2Id);
    Task<LiveScoreDto?> AddPointAsync(Guid matchId, Guid playerId, PointType pointType);
    Task<LiveScoreDto?> EndGameAsync(Guid matchId, Guid playerId);
    Task<LiveScoreDto?> EndSetAsync(Guid matchId, Guid playerId);
    Task<LiveScoreDto?> EndMatchAsync(Guid matchId, Guid playerId);
    Task<LiveScoreDto?> GetMatchDetailsAsync(Guid matchId);
    Task<LiveScoreDto?> GetLiveScoreAsync(Guid matchId);
    Task<LiveScoreDto?> GetSetDetailsAsync(Guid matchId, int setNumber);
    Task<LiveScoreDto?> GetGameDetailsAsync(Guid matchId, int setNumber, int gameNumber);
    Task<LiveScoreDto?> GetPointDetailsAsync(Guid matchId, int setNumber, int gameNumber, int pointNumber);
    Task<LiveScoreDto?> GetPlayerDetailsAsync(Guid matchId, Guid playerId);
    Task<LiveScoreDto?> GetMatchHistoryAsync(Guid matchId);
    Task<LiveScoreDto?> GetUpcomingMatchesAsync();
    Task<LiveScoreDto?> GetTournamentDetailsAsync(Guid tournamentId);
    Task<LiveScoreDto?> GetPlayerRankingsAsync();
    Task<LiveScoreDto?> GetTournamentRankingsAsync(Guid tournamentId);
    Task<LiveScoreDto?> GetMatchStatisticsAsync(Guid matchId);
    Task<LiveScoreDto?> GetSetStatisticsAsync(Guid matchId, int setNumber);
    Task<LiveScoreDto?> GetGameStatisticsAsync(Guid matchId, int setNumber, int gameNumber);
    Task<LiveScoreDto?> GetPointStatisticsAsync(Guid matchId, int setNumber, int gameNumber, int pointNumber);
    Task<LiveScoreDto?> GetPlayerStatisticsAsync(Guid matchId, Guid playerId);
    Task<LiveScoreDto?> GetTournamentMatchesAsync(Guid tournamentId);
    Task<LiveScoreDto?> GetPlayerMatchesAsync(Guid playerId);
    Task<LiveScoreDto?> GetMatchFormatsAsync();
    Task<LiveScoreDto?> GetMatchFormatDetailsAsync(Guid matchFormatId);
    Task<LiveScoreDto?> GetMatchFormatByNameAsync(string name);
    Task<LiveScoreDto?> GetMatchFormatByIdAsync(Guid matchFormatId);*/
}
