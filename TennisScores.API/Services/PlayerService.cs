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
        var player = new Player
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Age = request.Age,
            Nationality = request.Nationality,
            FftRanking = request.FftRanking
        };

        await _playerRepository.AddAsync(player);
        await _unitOfWork.SaveChangesAsync();

        return player.Id;
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        return await _playerRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        return await _playerRepository.GetAllAsync();
    }
}
