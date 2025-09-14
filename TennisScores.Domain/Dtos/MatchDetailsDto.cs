using System;
using System.Collections.Generic;
using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Dtos
{
    public class MatchDetailsDto
    {
        public Guid Id { get; set; }

        public required PlayerDto Player1 { get; set; }
        public required PlayerDto Player2 { get; set; }

        public required Guid ServingPlayerId { get; set; }
        public string? TournamentName { get; set; }
        public Guid? TournamentId { get; set; }
        public DateTime? TournamentStartDate { get; set; }

        public int BestOfSets { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Surface { get; set; }
        public string? WinnerFirstName { get; set; }
        public string? WinnerLastName { get; set; }
        public List<SetScoreDto> Sets { get; set; } = [];
    }
}
