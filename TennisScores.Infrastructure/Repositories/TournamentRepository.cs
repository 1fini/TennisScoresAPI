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
            .FirstOrDefaultAsync(t => t.Name == name && t.StartDate == startDate);
    }

    public Task<IEnumerable<Tournament>> GetUpcomingAsync()
    {
        throw new NotImplementedException();
    }
}

