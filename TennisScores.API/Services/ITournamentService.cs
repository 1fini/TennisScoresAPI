using TennisScores.Domain.Dtos;

namespace TennisScores.API.Services;
public interface ITournamentService
{
    Task<Guid> CreateTournamentAsync(CreateTournamentRequest request);
    Task<TournamentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TournamentDto>> GetAllAsync();
}
