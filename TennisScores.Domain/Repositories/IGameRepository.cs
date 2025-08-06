namespace TennisScores.Domain.Repositories;

using TennisScores.Domain.Entities;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id);
    Task<IEnumerable<Game>> GetAllBySetIdAsync(Guid setId);
    Task AddAsync(Game game);
    void Update(Game game);
    Task DeleteAsync(Guid id);
}
