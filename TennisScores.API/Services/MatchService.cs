using TennisScores.Domain.Entities;
using TennisScores.Domain.Dtos;
using TennisScores.Domain.Repositories;
using TennisScores.Domain;
using TennisScores.Domain.Enums;
using TennisScores.Domain.Scoring;

namespace TennisScores.API.Services;

public class MatchService(
    IMatchRepository matchRepository,
    IPlayerRepository playerRepository,
    ITournamentRepository tournamentRepository,
    ILogger<MatchService> logger,
    IUnitOfWork unitOfWork,
    ScoringEngine scoringEngine) : IMatchService
{
    private readonly IMatchRepository _matchRepository = matchRepository;
    private readonly IPlayerRepository _playerRepository = playerRepository;
    private readonly ITournamentRepository _tournamentRepository = tournamentRepository;
    private readonly ILogger<MatchService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ScoringEngine _scoringEngine = scoringEngine;
    private readonly ScoringReplay _scoringReplay = new(scoringEngine);

    public async Task<MatchDto> CreateMatchAsync(CreateMatchRequest request)
    {
        // Data validation
        if (request.Player1Id == request.Player2Id)
        {
            throw new ArgumentException("Players cannot be the same.");
        }
        if (request.BestOfSets < 1 || request.BestOfSets > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(request.BestOfSets), "BestOfSets must be between 1 and 5.");
        }
        if (request.ServingPlayer != request.Player1Id && request.ServingPlayer != request.Player2Id)
        {
            throw new ArgumentException("Serving player must be one of the match participants.");
        }

        var player1 = await _playerRepository.GetByIdAsync(request.Player1Id) ?? throw new ArgumentException($"Player 1 with ID '{request.Player1Id}' not found.");
        var player2 = await _playerRepository.GetByIdAsync(request.Player2Id) ?? throw new ArgumentException($"Player 2 with ID '{request.Player2Id}' not found.");
        var servingPlayer = request.ServingPlayer == player1.Id ? player1 : player2;

        Tournament? tournament = null;
        var bestOfSets = request.BestOfSets;
        if (request.TournamentId != null && request.TournamentId.HasValue)
        {
            tournament = await _tournamentRepository.GetByIdAsync(request.TournamentId.Value);

            if (tournament == null)
            {
                throw new ArgumentException($"Tournament with ID '{request.TournamentId}' not found.");
            }

            bestOfSets = GetBestOfSetsFromFormat(tournament.MatchFormat);
        }
        else
        {
            // If no tournament is specified, ensure BestOfSets is provided
            if (request.BestOfSets < 1 || request.BestOfSets > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(request.BestOfSets), "BestOfSets must be between 1 and 5 when no tournament is specified.");
            }
        }

        var match = new Match
        {
            Player1Id = player1.Id,
            Player2Id = player2.Id,
            ServingPlayerId = servingPlayer.Id,
            IsCompleted = false,
            BestOfSets = bestOfSets,
            StartTime = DateTime.UtcNow,
            TournamentId = tournament?.Id
        };

        await _matchRepository.AddAsync(match);
        _logger.LogInformation($"Match created: {match.Id} between {player1.FirstName} {player1.LastName} and {player2.FirstName} {player2.LastName}");

        await _unitOfWork.SaveChangesAsync();
        
        return new MatchDto
        {
            Id = match.Id,
            Player1FirstName = player1.FirstName,
            Player1LastName = player1.LastName,
            Player2FirstName = player2.FirstName,
            Player2LastName = player2.LastName,
            BestOfSets = match.BestOfSets,
            StartTime = match.StartTime
        };
    }

    private static int GetBestOfSetsFromFormat(MatchFormat format)
        => (format.SetsToWin * 2) - 1;

    public async Task<MatchDetailsDto?> GetMatchAsync(Guid matchId)
    {
        var match = await _matchRepository
            .GetMatchWithDetailsAsync(matchId);

        if (match == null) return null;

        return match.MapToFullDto();
    }

    public async Task<MatchAnalyticsDto?> GetAnalyticsAsync(Guid matchId)
    {
        var match = await _matchRepository.GetFullMatchByIdAsync(matchId);
        if (match is null)
            return null;

        var points = match.Sets
            .OrderBy(set => set.SetNumber)
            .SelectMany(set => set.Games
                .OrderBy(game => game.GameNumber)
                .SelectMany(game => game.Points
                    .OrderBy(point => point.Timestamp)
                    .ThenBy(point => point.Id)))
            .ToList();
        var player1 = new AnalyticsAccumulator(match.Player1Id);
        var player2 = new AnalyticsAccumulator(match.Player2Id);

        foreach (var point in points)
        {
            var winner = point.WinnerId == match.Player1Id
                ? player1
                : point.WinnerId == match.Player2Id
                    ? player2
                    : throw new InvalidOperationException(
                        $"Point '{point.Id}' has an invalid winner.");
            var loser = winner == player1 ? player2 : player1;
            winner.TotalPointsWon++;

            switch (point.PointType)
            {
                case PointType.Ace:
                    winner.Aces++;
                    break;
                case PointType.Winner:
                    winner.Winners++;
                    break;
                case PointType.DoubleFault:
                    loser.DoubleFaults++;
                    break;
                case PointType.UnforcedError:
                    loser.UnforcedErrors++;
                    break;
                case PointType.ForcedError:
                    loser.ForcedErrors++;
                    break;
            }
        }

        var serviceContextAvailable = TryAddServiceContext(
            match,
            points,
            player1,
            player2);
        return new MatchAnalyticsDto(
            match.Id,
            match.IsCompleted,
            serviceContextAvailable,
            player1.ToDto(match.Player1?.FirstName, match.Player1?.LastName,
                serviceContextAvailable),
            player2.ToDto(match.Player2?.FirstName, match.Player2?.LastName,
                serviceContextAvailable));
    }

    public async Task<List<MatchDto>> GetAllAsync()
    {
        var matches = await _matchRepository.GetAllAsync();

        return [.. matches.Select(m => m.MapToMatchDto())];
    }

    public async Task<bool> DeleteMatchAsync(Guid matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match == null)
        {
            return false;
        }

        _matchRepository.Remove(match);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private bool TryAddServiceContext(
        Match match,
        IReadOnlyCollection<Point> points,
        AnalyticsAccumulator player1,
        AnalyticsAccumulator player2)
    {
        try
        {
            var observedState = ScoringStateFactory.FromMatch(match);
            var awards = points.Select(point => new AwardPoint(
                point.WinnerId!.Value,
                point.PointType,
                point.Timestamp)).ToList();
            var initialServer = _scoringReplay.InferInitialServingPlayerId(
                observedState,
                awards);
            var state = _scoringReplay.Replay(
                observedState,
                initialServer,
                []);

            foreach (var award in awards)
            {
                var server = state.ServingPlayerId == match.Player1Id
                    ? player1
                    : state.ServingPlayerId == match.Player2Id
                        ? player2
                        : throw new InvalidOperationException(
                            "Serving player is not a match participant.");
                var returner = server == player1 ? player2 : player1;
                server.PointsServed++;
                returner.PointsReturned++;
                state = _scoringEngine.Apply(state, award).State;
            }

            return true;
        }
        catch (InvalidOperationException)
        {
            player1.PointsServed = 0;
            player1.PointsReturned = 0;
            player2.PointsServed = 0;
            player2.PointsReturned = 0;
            return false;
        }
    }

    private sealed class AnalyticsAccumulator(Guid playerId)
    {
        public Guid PlayerId { get; } = playerId;
        public int TotalPointsWon { get; set; }
        public int Aces { get; set; }
        public int DoubleFaults { get; set; }
        public int Winners { get; set; }
        public int UnforcedErrors { get; set; }
        public int ForcedErrors { get; set; }
        public int PointsServed { get; set; }
        public int PointsReturned { get; set; }

        public PlayerMatchAnalyticsDto ToDto(
            string? firstName,
            string? lastName,
            bool serviceContextAvailable)
            => new(
                PlayerId,
                firstName ?? string.Empty,
                lastName ?? string.Empty,
                TotalPointsWon,
                Aces,
                DoubleFaults,
                Winners,
                UnforcedErrors,
                ForcedErrors,
                serviceContextAvailable ? PointsServed : null,
                serviceContextAvailable ? PointsReturned : null);
    }
}
