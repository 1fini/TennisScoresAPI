using TennisScores.Domain.Entities;
using TennisScores.Domain.Dtos;
using TennisScores.Domain.Repositories;
using TennisScores.Domain;

namespace TennisScores.API.Services;

public class MatchService(
    IMatchRepository matchRepository,
    IPlayerRepository playerRepository,
    ITournamentRepository tournamentRepository,
    ILogger<MatchService> logger,
    IUnitOfWork unitOfWork) : IMatchService
{
    private readonly IMatchRepository _matchRepository = matchRepository;
    private readonly IPlayerRepository _playerRepository = playerRepository;
    private readonly ITournamentRepository _tournamentRepository = tournamentRepository;
    private readonly ILogger<MatchService> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<MatchDto> CreateMatchAsync(CreateMatchRequest request)
    {
        // Data validation
        if (string.IsNullOrWhiteSpace(request.Player1FirstName) || string.IsNullOrWhiteSpace(request.Player1LastName) ||
            string.IsNullOrWhiteSpace(request.Player2FirstName) || string.IsNullOrWhiteSpace(request.Player2LastName))
        {
            throw new ArgumentException("Player names cannot be empty.");
        }
        if (request.Player1FirstName == request.Player2FirstName && request.Player1LastName == request.Player2LastName)
        {
            throw new ArgumentException("Players cannot be the same.");
        }
        if (request.BestOfSets < 1 || request.BestOfSets > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(request.BestOfSets), "BestOfSets must be between 1 and 5.");
        }
        if (string.IsNullOrWhiteSpace(request.ServingPlayerLastName))
        {
            throw new ArgumentException("Serving player last name cannot be empty.");
        }

        var player1 = await _playerRepository.GetOrCreateAsync(request.Player1FirstName, request.Player1LastName);
        var player2 = await _playerRepository.GetOrCreateAsync(request.Player2FirstName, request.Player2LastName);

        // Vérification de l'existence du joueur servant
        var servingPlayer = await _playerRepository.GetByFullNameAsync(
            request.ServingPlayerFirstName,
            request.ServingPlayerLastName) ?? throw new ArgumentException($"Serving player '{request.ServingPlayerFirstName} {request.ServingPlayerLastName}' not found.");
        if (servingPlayer.Id != player1.Id && servingPlayer.Id != player2.Id)
        {
            throw new ArgumentException("Serving player must be one of the match participants.");
        }
        // Gestion du tournoi (optionnel)
        Tournament? tournament = null;
        if (!string.IsNullOrWhiteSpace(request.TournamentName) && request.TournamentStartDate != null)
        {
            tournament = await _tournamentRepository.GetByNameAndStartDateAsync(request.TournamentName!, request.TournamentStartDate!.Value);

            if (tournament == null)
            {
                throw new ArgumentException($"Tournament with name '{request.TournamentName}' and start date '{request.TournamentStartDate}' not found.");
            }
        }

        // Création du match
        var match = new Match
        {
            Player1Id = player1.Id,
            Player2Id = player2.Id,
            ServingPlayerId = servingPlayer.Id,
            IsCompleted = false,
            BestOfSets = request.BestOfSets,
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

    public async Task<MatchDetailsDto?> GetMatchAsync(Guid matchId)
    {
        var match = await _matchRepository
            .GetMatchWithDetailsAsync(matchId);

        if (match == null) return null;

        return match.MapToFullDto();
    }

    public async Task<List<MatchDto>> GetAllAsync()
    {
        var matches = await _matchRepository.GetAllAsync();

        return [.. matches.Select(m => m.MapToMatchDto())];
    } 
}
