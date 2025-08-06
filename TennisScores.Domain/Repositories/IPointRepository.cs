using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Repositories;

public interface IPointRepository : IRepository<Point>
{
    Task<IEnumerable<Point>> GetAllByGameIdAsync(Guid gameId);
}
