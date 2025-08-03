
using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Repositories
{
    public interface IMatchRepository : IRepository<Match>
    {
        Task<IEnumerable<Match>> GetMatchesByPlayerIdAsync(Guid playerId);
        Task<IEnumerable<Match>> GetMatchesByTournamentIdAsync(Guid tournamentId);
        Task<Match?> GetMatchWithDetailsAsync(Guid matchId);
        Task<Match?> GetFullMatchByIdAsync(Guid matchId);
    }
}
