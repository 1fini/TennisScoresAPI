using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Tests.Integration.Services;

public class DatabaseFixture : IDisposable
{
    public TennisDbContext Context { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<TennisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new TennisDbContext(options);

        // You can seed initial data here if needed:
        SeedData(Context);
    }

    public void Dispose()
    {
        Context.Dispose();
    }

    private void SeedData(TennisDbContext context)
    {
        // Example of seeding data
        // context.Players.Add(new Player { Id = Guid.NewGuid(), FirstName = "Roger", LastName = "Federer" });

        context.Tournaments.Add(new Tournament
        {
            Id = Guid.NewGuid(),
            Name = "US Open",
            StartDate = new DateTime(2025, 8, 2),
            Location = "New York"
        });

        context.SaveChanges();
    }
}
