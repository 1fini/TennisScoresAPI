namespace TennisScores.Domain.Dtos;

public class GameScoreDto
{
    public int Player1Points { get; set; }
    public int Player2Points { get; set; }
    public bool IsTiebreak { get; set; }
    public bool IsSuperTieBreak { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? WinnerId { get; set; }
}