namespace TennisScores.Domain.Entities;

public class Point
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;    // Ajouté
    public int PointNumber { get; set; } // Ex: 1er point, 2e point, etc.
    public Guid? WinnerId { get; set; }
    public Player? Winner { get; set; }
    public string? Description { get; set; } // Ex: "Ace", "Double faute", etc.
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
