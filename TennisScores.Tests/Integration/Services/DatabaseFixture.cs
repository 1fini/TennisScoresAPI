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
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Debug)
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
        SeedMatchFormats(context);
        SeedTournaments(context);

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

    private void SeedMatchFormats(TennisDbContext context)
    {
        context.MatchFormats.Add(new MatchFormat
        {
            Id = 1,
            Name = "Format 1", // Traditional
            GamesPerSet = 6,
            SetsToWin = 3,
            TieBreakEnabled = true,
            DecidingPointEnabled = false,
            SuperTieBreakForFinalSet = false
        });

        context.MatchFormats.Add(new MatchFormat
        {
            Id = 2,
            Name = "Format 2", // Allowed from 12 years old +
            GamesPerSet = 6,
            SetsToWin = 2,
            TieBreakEnabled = true,
            DecidingPointEnabled = false,
            SuperTieBreakForFinalSet = false
        });
        context.MatchFormats.Add(new MatchFormat
        {
            Id = 3,
            Name = "Format 3",
            SetsToWin = 2,
            GamesPerSet = 4,
            TieBreakEnabled = true,
            DecidingPointEnabled = true,
            SuperTieBreakForFinalSet = true,
            Application = "Format TMC"
        });
        context.MatchFormats.Add(new MatchFormat
        {
            Id = 4,
            Name = "Format 4",
            SetsToWin = 2,
            GamesPerSet = 6,
            TieBreakEnabled = true,
            DecidingPointEnabled = true,
            SuperTieBreakForFinalSet = true,
            Application = "Double Format"
        });
        context.MatchFormats.Add(new MatchFormat
        {
            Id = 5,
            Name = "Format 5",
            SetsToWin = 2,
            GamesPerSet = 3,
            TieBreakEnabled = true,
            DecidingPointEnabled = true,
            SuperTieBreakForFinalSet = true,
            Application = "TMC à partir de 8 ans"
        });
        context.MatchFormats.Add(new MatchFormat
        {
            Id = 6,
            Name = "Format 6",
            SetsToWin = 2,
            GamesPerSet = 4,
            TieBreakEnabled = true, //tiebreak at 3/3
            DecidingPointEnabled = true,
            SuperTieBreakForFinalSet = true,
            Application = "TMC 11–15 ans"
        });
        context.MatchFormats.Add(new MatchFormat
        {
            Id = 7,
            Name = "Format 7",
            SetsToWin = 2,
            GamesPerSet = 5,
            TieBreakEnabled = true, //tiebreak at 4/4
            DecidingPointEnabled = true,
            SuperTieBreakForFinalSet = true,
            Application = "TMC 11–15 ans"
        });
    }

    private void SeedTournaments(TennisDbContext context)
    {
        // For testing Format 2
        context.Tournaments.Add(new Tournament
        {
            Name = "US Open 2",
            StartDate = new DateTime(2025, 8, 2),
            Location = "New York",
            MatchFormatId = 2
        });

        // For testing Format 1
        context.Tournaments.Add(new Tournament
        {
            Name = "US Open",
            StartDate = new DateTime(2025, 8, 2),
            Location = "New York",
            MatchFormatId = 1
        });
    }
}
