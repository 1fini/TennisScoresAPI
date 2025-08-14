using TennisScores.Domain.Dtos;

namespace TennisScores.API.Services;

/// <summary>
/// Interface for match services.
/// </summary>
public interface IMatchService
{
    Task<MatchDto> CreateMatchAsync(CreateMatchRequest request);
    Task<MatchDetailsDto?> GetMatchAsync(Guid matchId);
    Task<List<MatchDto>> GetAllAsync();
    /*
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerIdAsync(Guid playerId);
    Task<IEnumerable<MatchDto>> GetMatchesByTournamentIdAsync(Guid tournamentId);
    Task<IEnumerable<MatchDto>> GetAllMatchesAsync();
    Task UpdateMatchAsync(Guid matchId, UpdateMatchRequest request);
    Task DeleteMatchAsync(Guid matchId);
    Task<IEnumerable<MatchDto>> GetMatchesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerNameAsync(string firstName, string lastName);
    Task<IEnumerable<MatchDto>> GetMatchesByTournamentNameAsync(string tournamentName);
    Task<IEnumerable<MatchDto>> GetMatchesByBestOfSetsAsync(int bestOfSets);
    //Task<IEnumerable<MatchDto>> GetMatchesByStatusAsync(MatchStatus status);
    Task<IEnumerable<MatchDto>> GetMatchesByScoreAsync(string score);
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndTournamentAsync(Guid playerId, Guid tournamentId);
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndDateAsync(Guid playerId, DateTime date);
    Task<IEnumerable<MatchDto>> GetMatchesByTournamentAndDateAsync(Guid tournamentId, DateTime date);
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndScoreAsync(Guid playerId, string score);
    Task<IEnumerable<MatchDto>> GetMatchesByTournamentAndScoreAsync(Guid tournamentId, string score);
    //Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndStatusAsync(Guid playerId, MatchStatus status);
    //Task<IEnumerable<MatchDto>> GetMatchesByTournamentAndStatusAsync(Guid tournamentId, MatchStatus status);
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndBestOfSetsAsync(Guid playerId, int bestOfSets);
    Task<IEnumerable<MatchDto>> GetMatchesByTournamentAndBestOfSetsAsync(Guid tournamentId, int bestOfSets);
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndTournamentAndDateAsync(Guid playerId, Guid tournamentId, DateTime date);
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndTournamentAndScoreAsync(Guid playerId, Guid tournamentId, string score);
    //Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndTournamentAndStatusAsync(Guid playerId, Guid tournamentId, MatchStatus status);
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndTournamentAndBestOfSetsAsync(Guid playerId, Guid tournamentId, int bestOfSets);
    Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndDateAndScoreAsync(Guid playerId, DateTime date, string score);
    //Task<IEnumerable<MatchDto>> GetMatchesByPlayerAndDateAndStatusAsync (Guid playerId, DateTime date, MatchStatus status);*/
}