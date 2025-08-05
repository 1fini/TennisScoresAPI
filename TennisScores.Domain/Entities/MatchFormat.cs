namespace TennisScores.Domain.Entities
{
    public class MatchFormat
    {
        public int Id { get; set; } // Format 1 → 7
        public string Name { get; set; } = string.Empty; // "Format 1"
        public int SetsToWin { get; set; } // 2 ou 3
        public int GamesPerSet { get; set; } // 3, 4, 5, 6
        public bool TieBreakEnabled { get; set; }
        public bool DecidingPointEnabled { get; set; } // NoAd actived ?
        public bool SuperTieBreakForFinalSet { get; set; }
        public int SuperTieBreakPoints { get; set; } = 10;// 10 points by default
        public int TieBreakPoints { get; set; } = 7; // 7 points by default
        public string Application { get; set; } = string.Empty;

        // Navigation
        //public ICollection<Tournament> Tournaments { get; set; } = [];
    }
}
