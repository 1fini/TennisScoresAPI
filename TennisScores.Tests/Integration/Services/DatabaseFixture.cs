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
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }

    private void SeedData(TennisDbContext context)
    {
        context.MatchFormats.Add(new MatchFormat
        {
            Id = 7,
            Name = "Format 7",
            SetsToWin = 2,
            TieBreakEnabled = true,
            DecidingPointEnabled = false,
            SuperTieBreakForFinalSet = false
        });

        context.Tournaments.Add(new Tournament
        {
            Id = Guid.NewGuid(),
            Name = "US Open",
            StartDate = new DateTime(2025, 8, 2),
            Location = "New York",
            MatchFormatId = 7
        });

        context.Players.Add(new Player
        {
            FirstName = "Jannik",
            LastName = "Sinner",
            FftRanking = Domain.Enums.FftRanking.Top20DamesTop30Messieurs, //TODO create ATP rankins for top world players
            Nationality = "Italian",
            Age = 23
        });

        context.Players.Add(new Player
        {
            FirstName = "Carlos",
            LastName = "Alcaraz",
            Nationality = "Spanish",
            Age = 21
        });

        context.SaveChanges();
    }
}
