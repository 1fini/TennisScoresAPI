using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Infrastructure.Repositories;

public class PointRepository(TennisDbContext context) : Repository<Point>(context), IPointRepository
{
    public async Task<IEnumerable<Point>> GetAllByGameIdAsync(Guid gameId)
    {
        return await _dbSet.Where(p => p.GameId == gameId).ToListAsync();
    }
}