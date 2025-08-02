using System;
using System.Collections.Generic;

namespace TennisScores.Domain.Dtos
{
    public class MatchDetailsDto
    {
        public Guid Id { get; set; }

        // Joueur 1
        public string Player1FirstName { get; set; } = string.Empty;
        public string Player1LastName { get; set; } = string.Empty;

        // Joueur 2
        public string Player2FirstName { get; set; } = string.Empty;
        public string Player2LastName { get; set; } = string.Empty;

        // Tournoi (facultatif)
        public string? TournamentName { get; set; }
        public DateTime? TournamentStartDate { get; set; }

        // Score & état du match
        public int BestOfSets { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Surface { get; set; }

        // Vainqueur (facultatif)
        public string? WinnerFirstName { get; set; }
        public string? WinnerLastName { get; set; }

        // Détail des sets joués
        public List<SetScoreDto> Sets { get; set; } = new();
    }
}
