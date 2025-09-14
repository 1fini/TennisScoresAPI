namespace TennisScores.Domain.Entities;

public class TennisSet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MatchId { get; set; }
    public required Match Match { get; set; }

    public int SetNumber { get; set; } // 1, 2, 3, ...

    public int Player1Games { get; set; } = 0;
    public int Player2Games { get; set; } = 0;

    public Guid? WinnerId { get; set; }
    public Player? Winner { get; set; }
    public bool IsCompleted { get; set; }

    public ICollection<Game> Games { get; set; } = new List<Game>();
}
