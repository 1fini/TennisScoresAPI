using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;

namespace TennisScores.API.Services;

// MatchFormatService.cs
public class MatchFormatService : IMatchFormatService
{
    private readonly IMatchFormatRepository _repository;

    public MatchFormatService(IMatchFormatRepository repository)
    {
        _repository = repository;
    }

    public Task<List<MatchFormat>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public async Task<MatchFormat?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
