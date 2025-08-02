namespace TennisScores.Domain.Dtos
{
    public class SetScoreDto
    {
        public int SetNumber { get; set; }            // 1, 2, 3, etc.
        public int Player1Games { get; set; }         // ex: 6
        public int Player2Games { get; set; }         // ex: 4
    }
}
