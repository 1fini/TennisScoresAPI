using TennisScores.Domain;
using TennisScores.Domain.Entities;

namespace TennisScores.Tests.Integration.Services;

public class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => 
        Task.FromResult(1); // Simulate a successful save operation
}