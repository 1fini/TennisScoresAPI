using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Dtos;

public class LiveScoreDto
{
    public Guid MatchId { get; set; }
    public string Player1Name { get; set; } = default!;
    public string Player2Name { get; set; } = default!;
    public List<SetScoreDto> Sets { get; set; } = default!;
    public int Player1SetsWon { get; set; }
    public int Player2SetsWon { get; set; }
    public int Player1Games { get; set; }
    public int Player2Games { get; set; }
    public string CurrentScore { get; set; } = "0-0"; // Ex: 15-30
    public string ServingPlayer { get; set; } = string.Empty; // Player1 or Player2
    public GameScoreDto CurrentGame { get; set; } = default!;
    public bool IsMatchOver { get; set; }
    public Guid? WinnerId { get; set; }
}
