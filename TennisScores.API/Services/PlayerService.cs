using TennisScores.API.Services;
using TennisScores.Domain;
using TennisScores.Domain.Dtos;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PlayerService(IPlayerRepository playerRepository, IUnitOfWork unitOfWork)
    {
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreatePlayerAsync(CreatePlayerRequest request)
    {
        DateTime? normalizedBirthdate = null;

        if (request.Birthdate.HasValue)
        {
            var dto = request.Birthdate.Value;
            normalizedBirthdate = dto.ToUniversalTime();
        }
        var player = new Player
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            BirthDate = normalizedBirthdate,
            Nationality = request.Nationality,
            FftRanking = request.FftRanking
        };

        await _playerRepository.AddAsync(player);
        await _unitOfWork.SaveChangesAsync();

        return player.Id;
    }

    public async Task<PlayerLightDto?> GetByIdAsync(Guid id)
    {
        return (await _playerRepository.GetByIdAsync(id))?.ToLightDto();
    }

    public async Task<IEnumerable<PlayerLightDto>> GetAllAsync()
    {
        return (await _playerRepository.GetAllAsync()).Select(p => p.ToLightDto());
    }

    public async Task<IEnumerable<PlayerLightDto>> SearchPlayersByNamePatternAsync(string name)
    {
        return (await _playerRepository.SearchByNamePatternAsync(name)).Select(p => p.ToLightDto());
    }

    public async Task<bool> DeletePlayer(Guid id)
    {
        var player = await _playerRepository.GetByIdAsync(id);

        if (player != null)
        {
            _playerRepository.Remove(player);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        else return false;
    }
}
