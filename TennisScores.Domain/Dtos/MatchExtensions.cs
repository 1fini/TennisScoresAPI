using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Dtos;

public static class MatchExtensions
{
    public static LiveScoreDto MapToScoreDto(this Match match)
    {
        var player1Name = $"{match.Player1?.FirstName} {match.Player1?.LastName}";
        var player2Name = $"{match.Player2?.FirstName} {match.Player2?.LastName}";

        var sets = match.Sets
            .OrderBy(s => s.SetNumber)
            .Select(set => new SetScoreDto
            {
                SetNumber = set.SetNumber,
                Player1Games = set.Games.Count(g => g.WinnerId == match.Player1Id),
                Player2Games = set.Games.Count(g => g.WinnerId == match.Player2Id),
                IsCompleted = set.IsCompleted
            }).ToList();

        var currenSet = match.Sets
            .OrderByDescending(s => s.SetNumber)
            .FirstOrDefault();

        var currentGame = currenSet?.Games
            .OrderByDescending(g => g.GameNumber)
            .FirstOrDefault();

        return new LiveScoreDto
        {
            MatchId = match.Id,
            Player1Name = player1Name,
            Player2Name = player2Name,
            Player1SetsWon = match.Sets.Count(s => s.IsCompleted &&
                s.WinnerId == match.Player1Id),
            Player2SetsWon = match.Sets.Count(s => s.IsCompleted &&
                s.WinnerId == match.Player1Id),
            Sets = sets,
            IsMatchOver = match.IsCompleted,
            WinnerId = match.WinnerId,
            CurrentGame = currentGame == null ? new GameScoreDto() : new GameScoreDto
            {
                Player1Points = currentGame.Points.Count(p => p.WinnerId == match.Player1Id),
                Player2Points = currentGame.Points.Count(p => p.WinnerId == match.Player2Id),
                IsTiebreak = currentGame.IsTiebreak,
                // TODO Fix IsSuperTiebreak init
                // IsSuperTieBreak = currenSet.SetNumber == match.Tournament.MatchFormat.SetsToWin
            }
        };
    }
}