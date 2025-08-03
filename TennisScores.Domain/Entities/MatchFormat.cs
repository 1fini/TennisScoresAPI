namespace TennisScores.Domain.Entities
{
    public class MatchFormat
    {
        public int Id { get; set; } // Format 1 → 7
        public string Name { get; set; } = string.Empty; // "Format 1"
        public string BallColor { get; set; } = string.Empty; // "Jaune", "Vert", "Orange"
        public int NumberOfSets { get; set; } // 2 ou 3
        public int GamesPerSet { get; set; } // 3, 4, 5, 6
        public bool TieBreakEnabled { get; set; }
        public bool NoAdvantage { get; set; }
        public bool SuperTieBreakEnabled { get; set; }
        public string Application { get; set; } = string.Empty; // Pour mémoire

        // Navigation
        public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    }
}
