using TennisScores.Domain.Dtos;
using TennisScores.Domain.Entities;

namespace TennisScores.API.Services;
public interface ITournamentService
{
    Task<Guid> CreateTournamentAsync(CreateTournamentRequest request);
    Task<Tournament?> GetByIdAsync(Guid id);
    Task<IEnumerable<Tournament>> GetAllAsync();
}
