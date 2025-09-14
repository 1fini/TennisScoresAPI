namespace TennisScores.Domain.Dtos;

public class MatchFormatDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SetsToWin { get; set; }
    public int GamesPerSet { get; set; }
    public bool TieBreakEnabled { get; set; }
    public bool DecidingPointEnabled { get; set; }
    public bool SuperTieBreakForFinalSet { get; set; }
    public string? Application { get; set; }
}
