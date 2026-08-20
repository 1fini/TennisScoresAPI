using Microsoft.AspNetCore.SignalR;
using TennisScores.API.Hubs;
using TennisScores.Domain;
using TennisScores.Domain.Dtos;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Enums;
using TennisScores.Domain.Repositories;
using TennisScores.Domain.Scoring;

namespace TennisScores.API.Services;

public class LiveScoreService(
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork,
    ISetRepository setRepository,
    IGameRepository gameRepository,
    IPointRepository pointRepository,
    ScoringEngine scoringEngine,
    IHubContext<ScoreHub> hubContext) : ILiveScoreService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMatchRepository _matchRepository = matchRepository;
    private readonly ISetRepository _setRepository = setRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IPointRepository _pointRepository = pointRepository;
    private readonly ScoringEngine _scoringEngine = scoringEngine;
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
}
