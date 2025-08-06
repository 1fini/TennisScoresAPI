using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Repositories;

public interface ITournamentRepository : IRepository<Tournament>
{
    Task<Tournament?> GetByNameAndStartDateAsync(string name, DateTime startDate);
    Task<IEnumerable<Tournament>> GetUpcomingAsync();
}
