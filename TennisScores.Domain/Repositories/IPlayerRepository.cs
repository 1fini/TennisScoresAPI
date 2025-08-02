// TennisScores.Domain/Repositories/IPlayerRepository.cs
using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Repositories;

public interface IPlayerRepository : IRepository<Player>
{
    Task<Player?> GetByFullNameAsync(string firstName, string lastName);
    Task<Player> GetOrCreateAsync(string firstName, string lastName);
}
