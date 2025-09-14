using TennisScores.Domain.Dtos;
using TennisScores.Domain.Entities;

namespace TennisScores.API.Services;

/// <summary>
/// Interface for player services.
/// </summary>
public interface IPlayerService
{
    Task<Guid> CreatePlayerAsync(CreatePlayerRequest request);
    Task<Player?> GetByIdAsync(Guid id);
    Task<IEnumerable<Player>> GetAllAsync();
}
