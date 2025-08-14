using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Infrastructure.Repositories;

public class TournamentRepository(TennisDbContext context) : Repository<Tournament>(context), ITournamentRepository
{
    public async Task<Tournament?> GetByNameAndStartDateAsync(string name, DateTime startDate)
    {
        return await _context.Tournaments
            .Include(t => t.MatchFormat)
            .Include(t => t.Matches)
            .FirstOrDefaultAsync(t => t.Name == name && t.StartDate == startDate);
    }

    public override async Task<IEnumerable<Tournament>> GetAllAsync()
    {
        return await _context.Tournaments
            .Include(t => t.MatchFormat)
            .Include(t => t.Matches)
                .ThenInclude(m => m.Player1)
            .Include(t => t.Matches)
                .ThenInclude(m => m.Player2)
            .Include(t => t.Matches)
                .ThenInclude(m => m.Winner)
            .ToListAsync();
    }

    public override async Task<Tournament?> GetByIdAsync(Guid id)
    {
        return await _context.Tournaments
            .Include(t => t.MatchFormat)
            .Include(t => t.Matches)
                .ThenInclude(m => m.Player1)
            .Include(t => t.Matches)
                .ThenInclude(m => m.Player2)
            .Include(t => t.Matches)
                .ThenInclude(m => m.Winner)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public Task<IEnumerable<Tournament>> GetUpcomingAsync()
    {
        throw new NotImplementedException();
    }
}

