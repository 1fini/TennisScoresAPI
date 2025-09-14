namespace TennisScores.Domain.Dtos;

public class SetScoreDto
{
    public int SetNumber { get; set; }
    public int Player1Games { get; set; }
    public int Player2Games { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? WinnerId { get; set; }
}