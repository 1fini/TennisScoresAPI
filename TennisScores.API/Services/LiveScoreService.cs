using System.Security.Principal;
using TennisScores.Domain;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Repositories;

namespace TennisScores.API.Services;

public class LiveScoreService(IMatchRepository matchRepository, IMatchFormatRepository matchFormatRepository, IUnitOfWork unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMatchRepository _matchRepository = matchRepository;
    private readonly IMatchFormatRepository _matchFormatRepository = matchFormatRepository;

    public async Task AddPointToMatchAsync(Guid matchId, Guid winnerId)
    {
        var match = await _matchRepository.GetFullMatchByIdAsync(matchId) ??
            throw new ArgumentException("Match does not exist");

        var format = match.Tournament?.MatchFormat ?? throw new ArgumentException("Match Format not found");

        // Get the current set or create a new one
        var currentSet = match.Sets
            .Where(s => !s.IsCompleted)
            .OrderBy(s => s.SetNumber)
            .LastOrDefault();

        if (currentSet == null)
        {
            currentSet = new TennisSet
            {
                Id = Guid.NewGuid(),
                MatchId = match.Id,
                Match = match,
                SetNumber = match.Sets.Count + 1,
                Games = []
            };
            if (match.Sets.Count >= match.Tournament.MatchFormat.GamesPerSet)
                throw new InvalidOperationException("Sets count is greater thant MatchFormat defines");

            match.Sets.Add(currentSet);
        }

        // Get the current Game or create one
        var currentGame = currentSet.Games
            .OrderByDescending(g => g.GameNumber)
            .FirstOrDefault(g => !g.IsCompleted);

        if (currentGame == null)
        {
            currentGame = new Game
            {
                Id = Guid.NewGuid(),
                SetId = currentSet.Id,
                Set = currentSet,
                GameNumber = currentSet.Games.Count + 1,
                Points = []
            };

            currentSet.Games.Add(currentGame);
        }

        // Add Point in Current Game
        currentGame.Points.Add(new Point
        {
            Id = Guid.NewGuid(),
            WinnerId = winnerId,
            Winner = match.Player1Id == winnerId ? match.Player1 : match.Player2,
            Timestamp = DateTime.UtcNow
        });

        // Is game completed?
        if (CheckGameIsOver(currentGame, match))
        {
            currentGame.IsCompleted = true;
            currentGame.WinnerId = winnerId;

            var gamesWon = currentSet.Games
            .Where(g => g.IsCompleted && g.WinnerId == winnerId)
            .Count();

            // Is Set Completed?
            if (CheckSetIsOver(currentSet, match, format))
            {
                currentSet.IsCompleted = true;
                currentSet.WinnerId = winnerId;

                var setsWon = match.Sets
                .Where(s => s.IsCompleted && s.WinnerId == winnerId)
                .Count();

                // Is Match Over?
                if (CheckMatchIsOver(match, format))
                {
                    match.IsCompleted = true;
                    match.WinnerId = winnerId;
                }
                else
                {
                    // New Set
                    var nextSet = new TennisSet
                    {
                        Id = Guid.NewGuid(),
                        MatchId = match.Id,
                        Match = match,
                        SetNumber = match.Sets.Count + 1,
                        Games = []
                    };
                    match.Sets.Add(nextSet);
                }
            }
            else
            {
                // New Game in same Set
                
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private static int GetGamesWonByPlayer(TennisSet set, Guid playerId)
    {
        return set.Games.Count(g => g.IsCompleted && g.WinnerId == playerId);
    }

    private static int GetSetsWonByPlayer(Match match, Guid playerId)
    {
        return match.Sets.Count(s => s.IsCompleted && s.WinnerId == playerId);
    }

    private static bool CheckGameIsOver(Game game, Match match)
    {
        var p1Points = game.Points.Count(p => p.WinnerId == match.Player1Id);
        var p2Points = game.Points.Count(p => p.WinnerId == match.Player2Id);

        var matchFormat = match.Tournament!.MatchFormat;

        if (game.IsTiebreak)
        {
            //Tiebreak or super tiebreak
            int pointsToWin = matchFormat.SuperTieBreakForFinalSet && IsSuperTieBreak(game, matchFormat) ? matchFormat.SuperTieBreakPoints : matchFormat.TieBreakPoints;
            int maxPoints = Math.Max(p1Points, p2Points);
            int diff = Math.Abs(p1Points - p2Points);

            return maxPoints >= pointsToWin && diff >= 2;
        }
        else
        {
            //Standard
            if (matchFormat.DecidingPointEnabled)
            {
                //No AD
                return p1Points == 4 || p2Points == 4;
            }
            else
            {
                //Avantage
                if ((p1Points >= 4 || p2Points >= 4) && Math.Abs(p1Points - p2Points) >= 2)
                    return true;
            }
        }
        return false;
    }

    private static bool IsTiebreakGame(Game game, Match match, MatchFormat format)
    {
        var currentSet = match.Sets.FirstOrDefault(s => s.Id == game.SetId);
        if (currentSet == null || !format.TieBreakEnabled) return false;

        var gamesP1 = currentSet.Games.Count(g => g.WinnerId == match.Player1Id);
        var gamesP2 = currentSet.Games.Count(g => g.WinnerId == match.Player2Id);

        return gamesP1 == format.GamesPerSet && gamesP2 == format.GamesPerSet;
    }

    private static bool CheckSetIsOver(TennisSet set, Match match, MatchFormat matchFormat)
    {
        var player1Games = set.Games.Count(g => g.WinnerId == match.Player1Id);
        var Player2Games = set.Games.Count(g => g.WinnerId == match.Player2Id);

        var requiredGames = matchFormat.GamesPerSet;
        var tiebreakThreshold = matchFormat.TieBreakEnabled ? requiredGames : requiredGames + 1;

        var gameDifference = Math.Abs(player1Games - Player2Games);

        // Direct victory (e.g. 6-4, 4-2, etc.)
        if ((player1Games >= requiredGames || Player2Games >= requiredGames) && gameDifference >= 2)
            return true;

        //Tie-Break
        if (matchFormat.TieBreakEnabled && player1Games == requiredGames && Player2Games == requiredGames)
            return true;

        //Super TieBreak
        if (matchFormat.SuperTieBreakForFinalSet && set.SetNumber == match.BestOfSets)
        {
            var lastGame = set.Games.LastOrDefault();
            if (lastGame != null && IsSuperTieBreak(lastGame, matchFormat))
                return lastGame!.IsCompleted;
        }
        return false;
    }

    private static bool IsSuperTieBreak(Game game, MatchFormat format)
    {
        return format.SuperTieBreakForFinalSet && game.IsTiebreak;
    }

    private static bool CheckMatchIsOver(Match match, MatchFormat matchFormat)
    {
        var setsToWin = matchFormat.SetsToWin;
        var setsWonByPlayer1 = GetSetsWonByPlayer(match, match.Player1Id);
        var setsWonByPlayer2 = GetSetsWonByPlayer(match, match.Player2Id);

        return setsWonByPlayer1 == setsToWin || setsWonByPlayer2 == setsToWin;
    }
}
