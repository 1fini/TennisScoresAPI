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
            BestOfSets = GetBestOfSets(match),
            Surface = match.Surface,
            TournamentName = match.Tournament?.Name,
            TournamentStartDate = match.Tournament?.StartDate,
            Sets = [.. match.Sets.OrderBy(s => s.SetNumber).Select(s => new SetScoreDto
            {
                SetNumber = s.SetNumber,
                Player1Games = s.Games.Count(g => g.WinnerId == match.Player1Id),
                Player2Games = s.Games.Count(g => g.WinnerId == match.Player2Id),
                IsCompleted = s.IsCompleted
            })],

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
