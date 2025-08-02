namespace TennisScores.Domain.Entities;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SetId { get; set; }
    public required TennisSet Set { get; set; }

    public int GameNumber { get; set; } // Dans le set

    public int Player1Points { get; set; } = 0;
    public int Player2Points { get; set; } = 0;

    public bool IsTiebreak { get; set; } = false;

    public Guid? WinnerId { get; set; }
    public Player? Winner { get; set; }

    public ICollection<Point> Points { get; set; } = new List<Point>();
}
