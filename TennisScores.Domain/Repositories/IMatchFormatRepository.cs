using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Repositories;

public interface IMatchFormatRepository
{
    Task<List<MatchFormat>> GetAllAsync();
    Task<MatchFormat?> GetByIdAsync(int id);
}
