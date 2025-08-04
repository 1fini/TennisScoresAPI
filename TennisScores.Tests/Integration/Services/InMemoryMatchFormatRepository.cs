using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;

namespace TennisScores.Tests.Integration.Services;

// 🔧 Mocks pour les tests d'intégration
public class InMemoryMatchFormatRepository(MatchFormat matchFormat) : IMatchFormatRepository
{
    private readonly MatchFormat _matchFormat = matchFormat;

    public Task<MatchFormat?> GetByIdAsync(int id) => Task.FromResult(_matchFormat.Id == id ? _matchFormat : null);
    public Task<List<MatchFormat>> GetAllAsync() => throw new NotImplementedException();
}