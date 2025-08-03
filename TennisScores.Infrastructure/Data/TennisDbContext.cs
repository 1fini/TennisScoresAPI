using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;

namespace TennisScores.Infrastructure.Data;

public class TennisDbContext : DbContext
{
    public TennisDbContext(DbContextOptions<TennisDbContext> options)
        : base(options) {}

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<TennisSet> Sets => Set<TennisSet>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Point> Points => Set<Point>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<MatchFormat> MatchFormats => Set<MatchFormat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Player
        modelBuilder.Entity<Player>()
            .HasMany(p => p.MatchesAsPlayer1)
            .WithOne(m => m.Player1)
            .HasForeignKey(m => m.Player1Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Player>()
            .HasMany(p => p.MatchesAsPlayer2)
            .WithOne(m => m.Player2)
            .HasForeignKey(m => m.Player2Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Match ↔ Winner (optional)
        modelBuilder.Entity<Match>()
            .HasOne(m => m.Winner)
            .WithMany()
            .HasForeignKey(m => m.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Set ↔ Match
        modelBuilder.Entity<TennisSet>()
            .HasOne(s => s.Match)
            .WithMany(m => m.Sets)
            .HasForeignKey(s => s.MatchId);

        // Set ↔ Winner
        modelBuilder.Entity<TennisSet>()
            .HasOne(s => s.Winner)
            .WithMany()
            .HasForeignKey(s => s.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Game ↔ Set
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Set)
            .WithMany(s => s.Games)
            .HasForeignKey(g => g.SetId);

        // Game ↔ Winner
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Winner)
            .WithMany()
            .HasForeignKey(g => g.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Point ↔ Game
        modelBuilder.Entity<Point>()
            .HasOne(p => p.Game)
            .WithMany(g => g.Points)
            .HasForeignKey(p => p.GameId);

        // Point ↔ Winner
        modelBuilder.Entity<Point>()
            .HasOne(p => p.Winner)
            .WithMany()
            .HasForeignKey(p => p.WinnerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Tournament ↔ Matches (1-n)
        modelBuilder.Entity<Tournament>()
            .HasMany(t => t.Matches)
            .WithOne(m => m.Tournament)
            .HasForeignKey(m => m.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tournament>()
            .HasIndex(t => new { t.Name, t.StartDate })
            .IsUnique();

        modelBuilder.ApplyConfiguration(new MatchFormatConfiguration());
    }
}
