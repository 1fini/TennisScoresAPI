using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Infrastructure.Repositories;

public class MatchFormatRepository : IMatchFormatRepository
{
    private readonly TennisDbContext _context;

    public MatchFormatRepository(TennisDbContext context)
    {
        _context = context;
    }

    public Task<List<MatchFormat>> GetAllAsync()
        => _context.MatchFormats.ToListAsync();

    public Task<MatchFormat?> GetByIdAsync(int id)
        => _context.MatchFormats
        .FirstOrDefaultAsync(f => f.Id == id);
}
