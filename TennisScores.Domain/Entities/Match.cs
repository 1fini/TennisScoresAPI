using System.ComponentModel.DataAnnotations;

namespace TennisScores.Domain.Entities;

public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid Player1Id { get; set; }
    [Required]
    public Guid Player2Id { get; set; }
    public Player Player1 { get; set; }
    public Player Player2 { get; set; }
    public Guid ServingPlayerId { get; set; }
    public int BestOfSets { get; set; } = 3;
    public DateTime? ScheduledStartTime { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public string? Surface { get; set; } // Terre battue, dur, etc.
    public Guid? WinnerId { get; set; }
    public Player? Winner { get; set; }
    public bool IsCompleted { get; set; }
    public ICollection<TennisSet> Sets { get; set; } = new List<TennisSet>();
    public Tournament? Tournament { get; set; } // Le tournoi auquel le match appartient
    public Guid? TournamentId { get; set; } // Foreign key for the tournament
}
