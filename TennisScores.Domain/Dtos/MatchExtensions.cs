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
            .Select(set => MapSetScoreDto(match, set))
            .ToList();

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
                s.WinnerId == match.Player2Id),
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

    /// <summary>
    /// Maps a Match entity to a MatchDetailsDto.
    /// Used to display match scores
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    public static MatchDetailsDto MapToFullDto(this Match match)
    {
        var winner = match.WinnerId == match.Player1Id
            ? match.Player1
            : match.WinnerId == match.Player2Id
                ? match.Player2
                : match.Winner;

        var newDto = new MatchDetailsDto
        {
            Id = match.Id,
            TournamentId = match.TournamentId,
            Player1 = new PlayerDto
            {
                Id = match.Player1Id,
                FirstName = match.Player1?.FirstName ?? "",
                LastName = match.Player1?.LastName ?? "",
            },
            Player2 = new PlayerDto
            {
                Id = match.Player2Id,
                FirstName = match.Player2?.FirstName ?? "",
                LastName = match.Player2?.LastName ?? ""
            },
            ServingPlayerId = match.ServingPlayerId,
            WinnerFirstName = winner?.FirstName,
            WinnerLastName = winner?.LastName,
            StartTime = match.StartTime,
            EndTime = match.EndTime,
            IsCompleted = match.IsCompleted,
            Score = FormatMatchScore(match),
            BestOfSets = GetBestOfSets(match),
            Surface = match.Surface,
            TournamentName = match.Tournament?.Name,
            TournamentStartDate = match.Tournament?.StartDate,
            Sets = [.. match.Sets.OrderBy(s => s.SetNumber).Select(s => MapSetScoreDto(match, s))],

        };

        var currentScore = ComputeCurrentScore(match);
        newDto.Player1.CurrentScore = currentScore["Player1"];
        newDto.Player2.CurrentScore = currentScore["Player2"];

        return newDto;
    }
    
        public static MatchDto MapToMatchDto(this Match match)
    {
        var newDto = new MatchDto
        {
            Id = match.Id,
            Player1FirstName = match.Player1?.FirstName ?? "",
            Player1LastName = match.Player1?.LastName ?? "",
            Player2FirstName = match.Player2?.FirstName ?? "",
            Player2LastName = match.Player2?.LastName ?? "",
            BestOfSets = GetBestOfSets(match),
            StartTime = match.StartTime,
            EndTime = match.EndTime,
            IsCompleted = match.IsCompleted,
            Score = FormatMatchScore(match),
            WinnerFirstName = match.WinnerId == match.Player1Id
                ? match.Player1?.FirstName
                : match.WinnerId == match.Player2Id
                    ? match.Player2?.FirstName
                    : match.Winner?.FirstName,
            WinnerLastName = match.WinnerId == match.Player1Id
                ? match.Player1?.LastName
                : match.WinnerId == match.Player2Id
                    ? match.Player2?.LastName
                    : match.Winner?.LastName
        };

        var currentScore = ComputeCurrentScore(match);
        newDto.CurrentScore = $"{currentScore["Player1"]} - {currentScore["Player2"]}";

        return newDto;
    }

    private static SetScoreDto MapSetScoreDto(Match match, TennisSet set)
    {
        var player1Games = set.Games.Count(g => g.WinnerId == match.Player1Id);
        var player2Games = set.Games.Count(g => g.WinnerId == match.Player2Id);
        var tieBreakGame = set.Games
            .Where(g => g.IsTiebreak)
            .OrderByDescending(g => g.GameNumber)
            .FirstOrDefault();
        var player1TieBreakPoints = tieBreakGame?.Points.Count(p => p.WinnerId == match.Player1Id);
        var player2TieBreakPoints = tieBreakGame?.Points.Count(p => p.WinnerId == match.Player2Id);
        var isSuperTieBreak = tieBreakGame != null && IsSuperTieBreakSet(match, set);

        return new SetScoreDto
        {
            SetNumber = set.SetNumber,
            Player1Games = player1Games,
            Player2Games = player2Games,
            IsCompleted = set.IsCompleted,
            WinnerId = set.WinnerId,
            IsTieBreak = tieBreakGame != null,
            IsSuperTieBreak = isSuperTieBreak,
            Player1TieBreakPoints = player1TieBreakPoints,
            Player2TieBreakPoints = player2TieBreakPoints,
            DisplayScore = FormatSetScore(
                player1Games,
                player2Games,
                player1TieBreakPoints,
                player2TieBreakPoints,
                isSuperTieBreak)
        };
    }

    private static string FormatMatchScore(Match match)
    {
        if (match.Sets == null || match.Sets.Count == 0)
            return "0-0";

        return string.Join(" ", match.Sets
            .OrderBy(s => s.SetNumber)
            .Select(s => MapSetScoreDto(match, s).DisplayScore)
            .Where(score => !string.IsNullOrWhiteSpace(score)));
    }

    private static string FormatSetScore(
        int player1Games,
        int player2Games,
        int? player1TieBreakPoints,
        int? player2TieBreakPoints,
        bool isSuperTieBreak)
    {
        if (isSuperTieBreak && player1TieBreakPoints.HasValue && player2TieBreakPoints.HasValue)
            return $"{player1TieBreakPoints}-{player2TieBreakPoints}";

        if (player1TieBreakPoints.HasValue &&
            player2TieBreakPoints.HasValue &&
            player1Games + player2Games > 0)
        {
            if (player1Games > player2Games)
                return $"{player1Games}-{player2Games}({player2TieBreakPoints})";

            if (player2Games > player1Games)
                return $"{player1Games}({player1TieBreakPoints})-{player2Games}";
        }

        return $"{player1Games}-{player2Games}";
    }

    private static Dictionary<string, string> ComputeCurrentScore(Match match)
    {
        Dictionary<string, string> currentScore = [];
        if (match.Sets == null || match.Sets.Count == 0)
        {
            currentScore["Player1"] = "0";
            currentScore["Player2"] = "0";
            return currentScore;
        }

        var lastSet = match.Sets.OrderByDescending(s => s.SetNumber).FirstOrDefault();
        if (lastSet?.Games == null || lastSet.Games.Count == 0)
        {
            currentScore["Player1"] = "0";
            currentScore["Player2"] = "0";
            return currentScore;
        }

        var lastGame = lastSet.Games.OrderByDescending(g => g.GameNumber).FirstOrDefault();
        if (lastGame == null)
        {
            currentScore["Player1"] = "0";
            currentScore["Player2"] = "0";
            return currentScore;
        }

        var pointsForPlayer1 = lastGame.Points.Count(p => p.WinnerId == match.Player1Id);
        var pointsForPlayer2 = lastGame.Points.Count(p => p.WinnerId == match.Player2Id);

        if (ShouldDisplayAsTiebreak(match, lastSet, lastGame))
        {
            // Tie-break or super tie-break
            currentScore["Player1"] = pointsForPlayer1.ToString();
            currentScore["Player2"] = pointsForPlayer2.ToString();
        }
        else
        {
            // Jeu classique
            currentScore["Player1"] = FormatGameScore(pointsForPlayer1, pointsForPlayer2);
            currentScore["Player2"] = FormatGameScore(pointsForPlayer2, pointsForPlayer1);
        }
        return currentScore;
    }

    private static bool ShouldDisplayAsTiebreak(Match match, TennisSet set, Game game)
    {
        var format = match.Tournament?.MatchFormat;
        if (format == null || !format.TieBreakEnabled)
            return false;

        if (format.SuperTieBreakForFinalSet && IsFinalSet(match, set))
            return true;

        var completedGames = set.Games.Where(g => g.IsCompleted).ToList();
        var player1Games = completedGames.Count(g => g.WinnerId == match.Player1Id);
        var player2Games = completedGames.Count(g => g.WinnerId == match.Player2Id);

        return game.IsTiebreak &&
            player1Games == format.GamesPerSet &&
            player2Games == format.GamesPerSet &&
            game.GameNumber == completedGames.Count + 1;
    }

    private static int GetBestOfSets(Match match)
        => match.Tournament?.MatchFormat == null
            ? match.BestOfSets
            : (match.Tournament.MatchFormat.SetsToWin * 2) - 1;

    private static bool IsFinalSet(Match match, TennisSet set)
        => set.SetNumber == GetBestOfSets(match);

    private static bool IsSuperTieBreakSet(Match match, TennisSet set)
    {
        var format = match.Tournament?.MatchFormat;
        return format?.SuperTieBreakForFinalSet == true && IsFinalSet(match, set);
    }

    private static string FormatGameScore(int pointsFor, int pointsAgainst)
    {
        if (pointsFor >= 3 && pointsAgainst >= 3)
        {
            if (pointsFor == pointsAgainst) return "40";     
            if (pointsFor == pointsAgainst + 1) return "AD";  
            if (pointsAgainst == pointsFor + 1) return "40";   
        }

        return pointsFor switch
        {
            0 => "0",
            1 => "15",
            2 => "30",
            3 => "40",
            _ => "AD"
        };
    }
}
