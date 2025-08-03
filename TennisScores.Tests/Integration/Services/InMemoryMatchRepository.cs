using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;

namespace TennisScores.Tests.Integration.Services;

// 🔧 Mocks pour les tests d'intégration
public class InMemoryMatchRepository : IMatchRepository
{
    private readonly Match _match;
    public InMemoryMatchRepository(Match match) => _match = match;

    public Task<Match?> GetFullMatchByIdAsync(Guid id)
        => Task.FromResult(_match.Id == id ? _match : null);

    public Task<Match?> GetByIdAsync(Guid id) => throw new NotImplementedException();
    public Task<IEnumerable<Match>> GetAllAsync() => throw new NotImplementedException();
    public Task<IEnumerable<Match>> GetMatchesByPlayerIdAsync(Guid playerId)
        => throw new NotImplementedException();
    public Task<IEnumerable<Match>> GetMatchesByTournamentIdAsync(Guid tournamentId)
        => throw new NotImplementedException();
    public Task<Match?> GetMatchWithDetailsAsync(Guid matchId)
        => throw new NotImplementedException();
    public Task AddAsync(Match entity) => throw new NotImplementedException();
    public Task SaveChangesAsync(Match entity) => throw new NotImplementedException();
    public Task SaveChangesAsync() => throw new NotImplementedException();
    public void Remove(Match entity) => throw new NotImplementedException();   
    public IQueryable<Match> Query() => throw new NotImplementedException();
}