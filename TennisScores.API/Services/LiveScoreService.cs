using TennisScores.Domain.Repositories;

public class LiveScoreService : ILiveScoreService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LiveScoreService(IMatchRepository matchRepository, IUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LiveScoreDto?> GetLiveScoreAsync(Guid matchId)
    {
        var match = await _matchRepository.Query()
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Sets)
                .ThenInclude(s => s.Games)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null) return null;

        var currentSet = match.Sets.OrderByDescending(s => s.SetNumber).FirstOrDefault();
        var currentGame = currentSet?.Games.OrderByDescending(g => g.GameNumber).FirstOrDefault();

        return new LiveScoreDto
        {
            MatchId = match.Id,
            Player1Name = $"{match.Player1?.FirstName} {match.Player1?.LastName}",
            Player2Name = $"{match.Player2?.FirstName} {match.Player2?.LastName}",
            Player1Sets = match.Sets.Count(s => s.Games.Count(g => g.WinnerId == match.Player1Id) > s.Games.Count(g => g.WinnerId == match.Player2Id)),
            Player2Sets = match.Sets.Count(s => s.Games.Count(g => g.WinnerId == match.Player2Id) > s.Games.Count(g => g.WinnerId == match.Player1Id)),
            Player1Games = currentSet?.Games.Count(g => g.WinnerId == match.Player1Id) ?? 0,
            Player2Games = currentSet?.Games.Count(g => g.WinnerId == match.Player2Id) ?? 0,
            CurrentScore = currentGame != null ? $"{currentGame.Player1Score}-{currentGame.Player2Score}" : "0-0",
            Server = match.ServerId == match.Player1Id ? "Player1" : "Player2"
        };
    }

    public async Task<LiveScoreDto?> PointWonAsync(Guid matchId, Guid playerId)
    {
        // ➜ à implémenter : logique d’ajout de point, conversion 15/30/40/AD, passage au jeu, set, match
        throw new NotImplementedException();
    }
}
