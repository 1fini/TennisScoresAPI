using TennisScores.Domain.Entities;

namespace TennisScores.API.Services;

public interface IMatchFormatService
{
    Task<List<MatchFormat>> GetAllAsync();
    Task<MatchFormat?> GetByIdAsync(int id);
}
