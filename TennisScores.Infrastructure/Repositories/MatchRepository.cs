using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Infrastructure.Repositories
{
    public class MatchRepository(TennisDbContext context) : Repository<Match>(context), IMatchRepository
    {
        public async Task<IEnumerable<Match>> GetMatchesByPlayerIdAsync(Guid playerId)
        {
            return await _context.Matches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Match>> GetMatchesByTournamentIdAsync(Guid tournamentId)
        {
            return await _context.Matches
                .Include(m => m.Tournament)
                .Where(m => m.TournamentId == tournamentId)
                .ToListAsync();
        }

        public async Task<Match?> GetMatchWithDetailsAsync(Guid matchId)
        {
            return await _context.Matches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.Winner)
                .Include(m => m.Tournament)
                    .ThenInclude(t => t!.MatchFormat)
                .Include(m => m.Sets)
                    .ThenInclude(s => s.Games)
                        .ThenInclude(g => g.Points)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }

        public async Task<Match?> GetFullMatchByIdAsync(Guid matchId)
        {
            return await _context.Matches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .Include(m => m.Winner)
                .Include(m => m.Tournament)
                    .ThenInclude(t => t!.MatchFormat)
                .Include(m => m.Sets)
                    .ThenInclude(s => s.Games)
                        .ThenInclude(g => g.Points)
                .FirstOrDefaultAsync(m => m.Id == matchId);
        }
    }
}
