using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Infrastructure.Repositories;

public class TournamentRepository : Repository<Tournament>, ITournamentRepository
{
    private readonly TennisDbContext _context;

    public TournamentRepository(TennisDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Tournament?> GetByNameAndStartDateAsync(string name, DateTime startDate)
    {
        return await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Name == name && t.StartDate == startDate);
    }
}

