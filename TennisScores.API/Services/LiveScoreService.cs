using System.Data;

using Microsoft.EntityFrameworkCore;
using TennisScores.Domain;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;

namespace TennisScores.API.Services;

public class LiveScoreService(
    IMatchRepository matchRepository,
    IMatchFormatRepository matchFormatRepository,
    IUnitOfWork unitOfWork,
    ISetRepository setRepository,
    IGameRepository gameRepository,
    IPointRepository pointRepository) : ILiveScoreService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMatchRepository _matchRepository = matchRepository;
    private readonly IMatchFormatRepository _matchFormatRepository = matchFormatRepository;
    private readonly ISetRepository _setRepository = setRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IPointRepository _pointRepository = pointRepository;

    public async Task AddPointToMatchAsync(Guid matchId, Guid winnerId)
    {
        try
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
                    MatchId = match.Id,
                    Match = match,
                    SetNumber = match.Sets.Count + 1,
                    Games = []
                };
                if (match.Sets.Count >= match.Tournament.MatchFormat.GamesPerSet)
                    throw new InvalidOperationException("Sets count is greater thant MatchFormat defines");

                match.Sets.Add(currentSet);
                await _setRepository.AddAsync(currentSet);
            }

            // Get the current Game or create one
            var currentGame = currentSet.Games
                .OrderByDescending(g => g.GameNumber)
                .FirstOrDefault(g => !g.IsCompleted);

            if (currentGame == null)
            {
                currentGame = new Game
                {
                    SetId = currentSet.Id,
                    Set = currentSet,
                    GameNumber = currentSet.Games.Count + 1,
                    Points = []
                };

                currentSet.Games.Add(currentGame);
                await _gameRepository.AddAsync(currentGame);
            }

            // Add Point in Current Game
            var point = new Point
            {
                WinnerId = winnerId,
                Winner = match.Player1Id == winnerId ? match.Player1 : match.Player2,
                Timestamp = DateTime.UtcNow
            };
            currentGame.Points.Add(point);
            await _pointRepository.AddAsync(point);


            // Is game completed?
            if (CheckGameIsOver(currentGame, match))
            {
                currentGame.IsCompleted = true;
                currentGame.WinnerId = winnerId;

                /*var gamesWon = currentSet.Games
                .Where(g => g.IsCompleted && g.WinnerId == winnerId)
                .Count();*/

                // Is Set Completed?
                if (CheckSetIsOver(currentSet, match, format))
                {
                    currentSet.IsCompleted = true;
                    currentSet.WinnerId = winnerId;

                    _gameRepository.Update(currentGame);
                    // Is Match Over?
                    if (CheckMatchIsOver(match))
                    {
                        match.IsCompleted = true;
                        match.WinnerId = winnerId;
                    }
                    else
                    {
                        // New Set
                        var nextSet = new TennisSet
                        {
                            MatchId = match.Id,
                            Match = match,
                            SetNumber = match.Sets.Count + 1,
                            Games = []
                        };
                        match.Sets.Add(nextSet);
                        await _setRepository.AddAsync(nextSet);
                    }
                }
                else
                {
                    // New Game in same Set
                    var newGame = new Game
                    {
                        SetId = currentSet.Id,
                        Set = currentSet,
                        GameNumber = currentSet.Games.Count + 1,
                        IsTiebreak = IsTiebreakNeeded(currentSet) || IsSuperTieBreak(match, currentSet)
                    };

                    currentSet.Games.Add(newGame);
                    await _gameRepository.AddAsync(newGame);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Retry the operation after resolving the concurrency conflict
        }
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
        var games = set.Games.Where(g => g.IsCompleted).ToList();
        var player1Games = games.Count(g => g.WinnerId == match.Player1Id);
        var player2Games = games.Count(g => g.WinnerId == match.Player2Id);

        var requiredGames = matchFormat.GamesPerSet;
        var gameDifference = Math.Abs(player1Games - player2Games);

        // 🎯 Cas 1 : victoire normale (ex: 6–4 ou 5–3)
        if ((player1Games >= requiredGames || player2Games >= requiredGames) && gameDifference >= 2)
        {
            return true;
        }

        // 🎯 Cas 2 : tie-break activé à égalité (ex: 6–6)
        if (matchFormat.TieBreakEnabled && player1Games == requiredGames && player2Games == requiredGames)
        {
            var tieBreakGame = set.Games.LastOrDefault(g => g.IsTiebreak);
            return tieBreakGame is not null && tieBreakGame.IsCompleted;
        }

        // 🎯 Cas 3 : super tie-break dans le set décisif (dernier set du match)
        if (matchFormat.SuperTieBreakForFinalSet && set.SetNumber == match.BestOfSets)
        {
            var lastGame = set.Games.LastOrDefault();
            if (lastGame is not null && IsSuperTieBreak(lastGame, matchFormat))
            {
                return lastGame.IsCompleted;
            }
        }

        // 🎯 Cas par défaut : le set continue
        return false;
    }

    private static bool IsSuperTieBreak(Game game, MatchFormat format)
    {
        return format.SuperTieBreakForFinalSet && game.IsTiebreak;
    }

    private static bool CheckMatchIsOver(Match match)
    {
        var player1SetWins = match.Sets.Count(s => s.IsCompleted && s.WinnerId == match.Player1Id);
        var player2SetWins = match.Sets.Count(s => s.IsCompleted && s.WinnerId == match.Player2Id);

        var setsNeededToWin = (match.BestOfSets / 2) + 1;

        if (player1SetWins >= setsNeededToWin)
        {
            match.WinnerId = match.Player1Id;
            match.IsCompleted = true;
            return true;
        }

        if (player2SetWins >= setsNeededToWin)
        {
            match.WinnerId = match.Player2Id;
            match.IsCompleted = true;
            return true;
        }

        return false;
    }


    private static bool IsTiebreakNeeded(TennisSet set)
    {
        var match = set.Match;
        var format = match.Tournament!.MatchFormat;

        if (!format.TieBreakEnabled)
            return false;

        var completedGames = set.Games.Where(g => g.IsCompleted).ToList();
        var player1Games = completedGames.Count(g => g.WinnerId == match.Player1Id);
        var player2Games = completedGames.Count(g => g.WinnerId == match.Player2Id);

        return player1Games == format.GamesPerSet && player2Games == format.GamesPerSet;
    }


    private static bool IsSuperTieBreak(Match match, TennisSet set)
    {
        var matchFormat = match.Tournament!.MatchFormat;

        if (!matchFormat.SuperTieBreakForFinalSet)
            return false;

        // Est-ce le dernier set du match ?
        var isFinalSet = set.SetNumber == match.BestOfSets;

        // Est-ce qu’un jeu est en cours ET déclaré comme tie-break ?
        var lastGame = set.Games.LastOrDefault();
        var isTieBreakGame = lastGame != null && lastGame.IsTiebreak;

        return isFinalSet && isTieBreakGame;
    }


}
