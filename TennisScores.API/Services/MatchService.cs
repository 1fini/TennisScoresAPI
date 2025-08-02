using TennisScores.Domain.Entities;
using TennisScores.Domain.Dtos;
using TennisScores.Domain.Repositories;
using TennisScoresAPI.Dtos;

namespace TennisScores.API.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ILogger<MatchService> _logger;

    public MatchService(
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        ITournamentRepository tournamentRepository,
        ILogger<MatchService> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<MatchDto> CreateMatchAsync(CreateMatchRequest request)
    {
        // Récupération ou création des joueurs
        var player1 = await _playerRepository.GetOrCreateAsync(request.Player1FirstName, request.Player1LastName);
        var player2 = await _playerRepository.GetOrCreateAsync(request.Player2FirstName, request.Player2LastName);

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
            Id = Guid.NewGuid(),
            Player1Id = player1.Id,
            Player2Id = player2.Id,
            BestOfSets = request.BestOfSets,
            StartTime = DateTime.UtcNow,
            TournamentId = tournament?.Id
        };

        await _matchRepository.AddAsync(match);

        return new MatchDto
        {
            Id = match.Id,
            Player1FirstName = player1.FirstName,
            Player1LastName = player1.LastName,
            Player2FirstName = player2.FirstName,
            Player2LastName = player2.LastName,
            BestOfSets = match.BestOfSets,
            StartTime = match.StartTime,
            Surface = match.Surface
        };
    }

    public async Task<MatchDetailsDto?> GetMatchAsync(Guid matchId)
    {
        var match = await _matchRepository
            .GetMatchWithDetailsAsync(matchId);

        if (match == null) return null;

        return new MatchDetailsDto
        {
            Id = match.Id,
            Player1FirstName = match.Player1?.FirstName ?? "",
            Player1LastName = match.Player1?.LastName ?? "",
            Player2FirstName = match.Player2?.FirstName ?? "",
            Player2LastName = match.Player2?.LastName ?? "",
            WinnerFirstName = match.Winner?.FirstName,
            WinnerLastName = match.Winner?.LastName,
            StartTime = match.StartTime,
            EndTime = match.EndTime,
            BestOfSets = match.BestOfSets,
            Surface = match.Surface,
            TournamentName = match.Tournament?.Name,
            TournamentStartDate = match.Tournament?.StartDate,
            Sets = [.. match.Sets.OrderBy(s => s.SetNumber).Select(s => new SetScoreDto
            {
                SetNumber = s.SetNumber,
                Player1Games = s.Games.Count(g => g.WinnerId == match.Player1Id),
                Player2Games = s.Games.Count(g => g.WinnerId == match.Player2Id),
            })]
        };
    }

}
