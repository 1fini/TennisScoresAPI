using TennisScores.Domain;
using TennisScores.Domain.Dtos;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;

namespace TennisScores.API.Services;

public class TournamentService : ITournamentService
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TournamentService(ITournamentRepository tournamentRepository, IUnitOfWork unitOfWork)
    {
        _tournamentRepository = tournamentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateTournamentAsync(CreateTournamentRequest request)
    {
        var tournament = new Tournament
        {
            Name = request.Name,
            Location = request.Location,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Description = request.Description,
            MinRankingFft = request.MinRankingFft,
            MaxRankingFft = request.MaxRankingFft,
            MinAge = request.MinAge,
            MaxAge = request.MaxAge,
            AgeCategory = request.AgeCategory,
            Type = request.Type,
            SubType = request.SubType,
            BallLevel = request.BallLevel,
            Surface = request.Surface,
            Condition = request.Condition,
            PrizeMoney = request.PrizeMoney,
            PrizeMoneyCurrency = request.PrizeMoneyCurrency,
            MatchFormatId = request.MatchFormatId,
        };

        await _tournamentRepository.AddAsync(tournament);
        await _unitOfWork.SaveChangesAsync();

        return tournament.Id;
    }

    public async Task<TournamentDto?> GetByIdAsync(Guid id)
    {
        return (await _tournamentRepository.GetByIdAsync(id))?.ToDetailedDto();
    }

    public async Task<IEnumerable<TournamentDto>> GetAllAsync()
    {
        return (await _tournamentRepository.GetAllAsync()).Select(t => t.ToDetailedDto());
    }
}
