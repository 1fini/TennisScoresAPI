using Microsoft.AspNetCore.SignalR;
using TennisScores.API.Hubs;
using TennisScores.Domain;
using TennisScores.Domain.Dtos;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Enums;
using TennisScores.Domain.Events;
using TennisScores.Domain.Repositories;
using TennisScores.Domain.Scoring;

namespace TennisScores.API.Services;

public class LiveScoreService(
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork,
    ISetRepository setRepository,
    IGameRepository gameRepository,
    IPointRepository pointRepository,
    IMatchEventRepository matchEventRepository,
    ScoringEngine scoringEngine,
    IHubContext<ScoreHub> hubContext) : ILiveScoreService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMatchRepository _matchRepository = matchRepository;
    private readonly ISetRepository _setRepository = setRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IPointRepository _pointRepository = pointRepository;
    private readonly IMatchEventRepository _matchEventRepository = matchEventRepository;
    private readonly ScoringEngine _scoringEngine = scoringEngine;
    private readonly ScoringReplay _scoringReplay = new(scoringEngine);
    private readonly IHubContext<ScoreHub> _hubContext = hubContext;

    public async Task AddPointToMatchAsync(
        Guid matchId,
        Guid winnerId,
        PointType pointType)
    {
        var match = await _matchRepository.GetFullMatchByIdAsync(matchId) ??
            throw new ArgumentException("Match does not exist");
        var occurredAt = DateTime.UtcNow;
        var result = _scoringEngine.Apply(
            ScoringStateFactory.FromMatch(match),
            new AwardPoint(winnerId, pointType, occurredAt));

        await ApplyProjectionAsync(match, result, pointType, occurredAt);
        await _matchEventRepository.AppendAsync(match.Id, result.Events);
        await _unitOfWork.SaveChangesAsync();
        await _hubContext.BroadcastPoint(match.MapToFullDto());
    }

    public async Task UndoLastPointAsync(Guid matchId)
    {
        var match = await _matchRepository.GetFullMatchByIdAsync(matchId) ??
            throw new KeyNotFoundException("Match does not exist");
        var projectionPoints = GetProjectionPoints(match);
        if (projectionPoints.Count == 0)
            throw new UndoNotAvailableException("Match has no point to undo.");

        var observedState = ScoringStateFactory.FromMatch(match);
        var awards = projectionPoints
            .Select(item => new AwardPoint(
                item.Point.WinnerId ?? throw new InvalidOperationException(
                    $"Point '{item.Point.Id}' has no winner."),
                item.Point.PointType,
                item.Point.Timestamp))
            .ToList();
        var initialServingPlayerId = _scoringReplay.InferInitialServingPlayerId(
            observedState,
            awards);
        var replayedState = _scoringReplay.Replay(
            observedState,
            initialServingPlayerId,
            awards.Take(awards.Count - 1));
        var undone = projectionPoints[^1];

        ApplyUndoProjection(match, replayedState, undone.Point);
        await _matchEventRepository.AppendAsync(
            match.Id,
            [new PointUndone(
                undone.Point.Id,
                undone.Point.WinnerId!.Value,
                undone.Point.PointType,
                undone.SetNumber,
                undone.GameNumber,
                undone.Point.Timestamp,
                DateTime.UtcNow)]);
        await _unitOfWork.SaveChangesAsync();
        await _hubContext.BroadcastPoint(match.MapToFullDto());
    }

    private async Task ApplyProjectionAsync(
        Match match,
        ScoringResult result,
        PointType pointType,
        DateTime occurredAt)
    {
        foreach (var scoringSet in result.State.Sets.OrderBy(set => set.SetNumber))
        {
            var set = match.Sets.SingleOrDefault(candidate =>
                candidate.SetNumber == scoringSet.SetNumber);
            if (set is null)
            {
                set = new TennisSet
                {
                    MatchId = match.Id,
                    Match = match,
                    SetNumber = scoringSet.SetNumber,
                    Games = []
                };
                match.Sets.Add(set);
                await _setRepository.AddAsync(set);
            }

            set.IsCompleted = scoringSet.IsCompleted;
            set.WinnerId = scoringSet.WinnerId;
            set.Winner = ResolvePlayer(match, scoringSet.WinnerId);

            foreach (var scoringGame in scoringSet.Games.OrderBy(game => game.GameNumber))
            {
                var game = set.Games.SingleOrDefault(candidate =>
                    candidate.GameNumber == scoringGame.GameNumber);
                if (game is null)
                {
                    game = new Game
                    {
                        SetId = set.Id,
                        Set = set,
                        GameNumber = scoringGame.GameNumber,
                        Points = []
                    };
                    set.Games.Add(game);
                    await _gameRepository.AddAsync(game);
                }

                game.IsTiebreak = scoringGame.IsTiebreak;
                game.IsCompleted = scoringGame.IsCompleted;
                game.WinnerId = scoringGame.WinnerId;
                game.Winner = ResolvePlayer(match, scoringGame.WinnerId);
            }
        }

        var appliedSet = match.Sets.Single(set =>
            set.SetNumber == result.AppliedSetNumber);
        var appliedGame = appliedSet.Games.Single(game =>
            game.GameNumber == result.AppliedGameNumber);
        var appliedPoint = result.State.Sets
            .Single(set => set.SetNumber == result.AppliedSetNumber)
            .Games.Single(game => game.GameNumber == result.AppliedGameNumber)
            .Points.Last();
        var point = new Point
        {
            GameId = appliedGame.Id,
            Game = appliedGame,
            WinnerId = appliedPoint.WinnerId,
            PointType = pointType,
            Timestamp = occurredAt
        };
        point.Winner = ResolvePlayer(match, point.WinnerId);
        appliedGame.Points.Add(point);
        await _pointRepository.AddAsync(point);

        match.ServingPlayerId = result.State.ServingPlayerId;
        match.IsCompleted = result.State.IsCompleted;
        match.WinnerId = result.State.WinnerId;
        match.Winner = ResolvePlayer(match, result.State.WinnerId);
        match.EndTime = result.State.EndTime;
    }

    private static Player? ResolvePlayer(Match match, Guid? playerId)
        => playerId == match.Player1Id
            ? match.Player1
            : playerId == match.Player2Id
                ? match.Player2
                : null;

    private static List<ProjectionPoint> GetProjectionPoints(Match match)
        => match.Sets
            .OrderBy(set => set.SetNumber)
            .SelectMany(set => set.Games
                .OrderBy(game => game.GameNumber)
                .SelectMany(game => game.Points
                    .OrderBy(point => point.Timestamp)
                    .ThenBy(point => point.Id)
                    .Select(point => new ProjectionPoint(
                        set.SetNumber,
                        game.GameNumber,
                        point))))
            .ToList();

    private void ApplyUndoProjection(
        Match match,
        ScoringState replayedState,
        Point undonePoint)
    {
        foreach (var set in match.Sets
                     .OrderByDescending(candidate => candidate.SetNumber)
                     .ToList())
        {
            var replayedSet = replayedState.Sets.SingleOrDefault(candidate =>
                candidate.SetNumber == set.SetNumber);
            if (replayedSet is null)
            {
                _setRepository.Remove(set);
                match.Sets.Remove(set);
                continue;
            }

            set.IsCompleted = replayedSet.IsCompleted;
            set.WinnerId = replayedSet.WinnerId;
            set.Winner = ResolvePlayer(match, replayedSet.WinnerId);

            foreach (var game in set.Games
                         .OrderByDescending(candidate => candidate.GameNumber)
                         .ToList())
            {
                var replayedGame = replayedSet.Games.SingleOrDefault(candidate =>
                    candidate.GameNumber == game.GameNumber);
                if (replayedGame is null)
                {
                    _gameRepository.Remove(game);
                    set.Games.Remove(game);
                    continue;
                }

                game.IsTiebreak = replayedGame.IsTiebreak;
                game.IsCompleted = replayedGame.IsCompleted;
                game.WinnerId = replayedGame.WinnerId;
                game.Winner = ResolvePlayer(match, replayedGame.WinnerId);
            }
        }

        var retainedGame = match.Sets
            .SelectMany(set => set.Games)
            .SingleOrDefault(game => game.Id == undonePoint.GameId);
        if (retainedGame is not null)
        {
            _pointRepository.Remove(undonePoint);
            retainedGame.Points.Remove(undonePoint);
        }

        match.ServingPlayerId = replayedState.ServingPlayerId;
        match.IsCompleted = replayedState.IsCompleted;
        match.WinnerId = replayedState.WinnerId;
        match.Winner = ResolvePlayer(match, replayedState.WinnerId);
        match.EndTime = replayedState.EndTime;
    }

    private sealed record ProjectionPoint(
        int SetNumber,
        int GameNumber,
        Point Point);
}
