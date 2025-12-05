using TennisScores.Domain.Dtos;

namespace TennisScores.API.Services;
public interface ITournamentService
{
    Task<TournamentDto> CreateTournamentAsync(CreateTournamentRequest request);
    Task<TournamentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TournamentDto>> GetAllAsync();
    Task<bool> DeleteTournament(Guid id);
}
