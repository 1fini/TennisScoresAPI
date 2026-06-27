using Microsoft.AspNetCore.SignalR;
using TennisScores.Domain;
using TennisScores.Domain.Dtos;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Enums;
using TennisScores.Domain.Repositories;
using TennisScores.API.Hubs;

namespace TennisScores.API.Services;

public class LiveScoreService(
    IMatchRepository matchRepository,
    IMatchFormatRepository matchFormatRepository,
    IUnitOfWork unitOfWork,
    ISetRepository setRepository,
    IGameRepository gameRepository,
    IPointRepository pointRepository,
    IHubContext<ScoreHub> hubContext) : ILiveScoreService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMatchRepository _matchRepository = matchRepository;
    private readonly IMatchFormatRepository _matchFormatRepository = matchFormatRepository;
    private readonly ISetRepository _setRepository = setRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IPointRepository _pointRepository = pointRepository;
    private readonly IHubContext<ScoreHub> _hubContext = hubContext;

    public async Task AddPointToMatchAsync(
        Guid matchId,
        Guid winnerId,
        PointType pointType)
    {
        var match = await _matchRepository.GetFullMatchByIdAsync(matchId) ??
            throw new ArgumentException("Match does not exist");

        var format = ValidatePointCanBeAdded(match, winnerId);
        var currentSet = await GetOrCreateCurrentSetAsync(match);
        var currentGame = await GetOrCreateCurrentGameAsync(match, currentSet);

        var point = new Point
        {
            WinnerId = winnerId,
            Winner = match.Player1Id == winnerId ? match.Player1 : match.Player2,
            PointType = pointType,
            Timestamp = DateTime.UtcNow
        };
        currentGame.Points.Add(point);
        await _pointRepository.AddAsync(point);

        var isGameOver = CheckGameIsOver(currentGame, match);
        if (currentGame.IsTiebreak)
        {
            UpdateServerAfterTieBreakPoint(match, currentGame);
        }

        if (isGameOver)
        {
            currentGame.IsCompleted = true;
            currentGame.WinnerId = winnerId;
            _gameRepository.Update(currentGame);

            if (!currentGame.IsTiebreak)
            {
                SwitchServer(match);
            }

            if (CheckSetIsOver(currentSet, match, format))
            {
                currentSet.IsCompleted = true;
                currentSet.WinnerId = winnerId;

                _gameRepository.Update(currentGame);
                if (CheckMatchIsOver(match))
                {
                    match.IsCompleted = true;
                    match.WinnerId = winnerId;
                }
                else
                {
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
                var newGame = new Game
                {
                    SetId = currentSet.Id,
                    Set = currentSet,
                    GameNumber = currentSet.Games.Count + 1,
                    IsTiebreak = ShouldGameBeTiebreak(match, currentSet)
                };

                currentSet.Games.Add(newGame);
                await _gameRepository.AddAsync(newGame);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        await _hubContext.BroadcastPoint(match.MapToFullDto());
    }

    private MatchFormat ValidatePointCanBeAdded(Match match, Guid winnerId)
    {
        if (match.IsCompleted)
            throw new InvalidOperationException("Cannot add a point to a completed match.");

        if (winnerId != match.Player1Id && winnerId != match.Player2Id)
            throw new ArgumentException("Point winner must be one of the match participants.", nameof(winnerId));

        return match.Tournament?.MatchFormat ?? throw new ArgumentException("Match Format not found");
    }

    private async Task<TennisSet> GetOrCreateCurrentSetAsync(Match match)
    {
        var currentSet = match.Sets
            .Where(s => !s.IsCompleted)
            .OrderBy(s => s.SetNumber)
            .LastOrDefault();

        if (currentSet != null)
            return currentSet;

        if (match.Sets.Count >= GetBestOfSets(match))
            throw new InvalidOperationException("Sets count is greater than the match format allows.");

        currentSet = new TennisSet
        {
            MatchId = match.Id,
            Match = match,
            SetNumber = match.Sets.Count + 1,
            Games = []
        };

        match.Sets.Add(currentSet);
        await _setRepository.AddAsync(currentSet);

        return currentSet;
    }

    private async Task<Game> GetOrCreateCurrentGameAsync(Match match, TennisSet currentSet)
    {
        var currentGame = currentSet.Games
            .OrderByDescending(g => g.GameNumber)
            .FirstOrDefault(g => !g.IsCompleted);

        if (currentGame != null)
        {
            var shouldBeTiebreak = ShouldGameBeTiebreak(match, currentSet, currentGame);
            if (currentGame.IsTiebreak != shouldBeTiebreak)
            {
                currentGame.IsTiebreak = shouldBeTiebreak;
                _gameRepository.Update(currentGame);
            }

            return currentGame;
        }

        currentGame = new Game
        {
            SetId = currentSet.Id,
            Set = currentSet,
            GameNumber = currentSet.Games.Count + 1,
            IsTiebreak = ShouldGameBeTiebreak(match, currentSet),
            Points = []
        };

        currentSet.Games.Add(currentGame);
        await _gameRepository.AddAsync(currentGame);

        return currentGame;
    }

    private static bool CheckGameIsOver(Game game, Match match)
    {
        var p1Points = game.Points.Count(p => p.WinnerId == match.Player1Id);
        var p2Points = game.Points.Count(p => p.WinnerId == match.Player2Id);

        var matchFormat = match.Tournament!.MatchFormat;

        if (game.IsTiebreak)
        {
            //Tiebreak or super tiebreak
            int pointsToWin = IsFinalSetSuperTieBreak(match, game.Set)
                ? matchFormat.SuperTieBreakPoints
                : matchFormat.TieBreakPoints;
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
        if (matchFormat.TieBreakEnabled && player1Games >= requiredGames && player2Games >= requiredGames)
        {
            var tieBreakGame = set.Games.LastOrDefault(g => g.IsTiebreak);
            return tieBreakGame is not null && tieBreakGame.IsCompleted;
        }

        // 🎯 Cas 3 : super tie-break dans le set décisif (dernier set du match)
        if (matchFormat.SuperTieBreakForFinalSet && IsFinalSet(match, set))
        {
            var lastGame = set.Games.LastOrDefault();
            if (lastGame is not null && lastGame.IsTiebreak)
            {
                return lastGame.IsCompleted;
            }
        }

        // 🎯 Cas par défaut : le set continue
        return false;
    }

    private static bool CheckMatchIsOver(Match match)
    {
        var player1SetWins = match.Sets.Count(s => s.IsCompleted && s.WinnerId == match.Player1Id);
        var player2SetWins = match.Sets.Count(s => s.IsCompleted && s.WinnerId == match.Player2Id);

        var setsNeededToWin = GetSetsToWin(match);

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
        return ShouldGameBeTiebreak(match, set);
    }

    private static bool ShouldGameBeTiebreak(Match match, TennisSet set, Game? game = null)
    {
        var format = match.Tournament!.MatchFormat;

        if (!format.TieBreakEnabled)
            return false;

        if (IsFinalSetSuperTieBreak(match, set))
            return true;

        var completedGames = set.Games.Where(g => g.IsCompleted).ToList();
        var player1Games = completedGames.Count(g => g.WinnerId == match.Player1Id);
        var player2Games = completedGames.Count(g => g.WinnerId == match.Player2Id);

        return player1Games == format.GamesPerSet &&
            player2Games == format.GamesPerSet &&
            (game == null || game.GameNumber == completedGames.Count + 1);
    }


    private static bool IsFinalSetSuperTieBreak(Match match, TennisSet set)
    {
        var matchFormat = match.Tournament!.MatchFormat;

        if (!matchFormat.SuperTieBreakForFinalSet)
            return false;

        return IsFinalSet(match, set);
    }

    private static int GetSetsToWin(Match match)
        => match.Tournament!.MatchFormat.SetsToWin;

    private static int GetBestOfSets(Match match)
        => (GetSetsToWin(match) * 2) - 1;

    private static bool IsFinalSet(Match match, TennisSet set)
        => set.SetNumber == GetBestOfSets(match);

    private void SwitchServer(Match match)
    {
        match.ServingPlayerId = (match.ServingPlayerId == match.Player1Id)
            ? match.Player2Id
            : match.Player1Id;
    }

    private void UpdateServerAfterTieBreakPoint(Match match, Game game)
    {
        var pointsPlayed = game.Points.Count;
        if (pointsPlayed % 2 == 1)
        {
            SwitchServer(match);
        }
    }
}
