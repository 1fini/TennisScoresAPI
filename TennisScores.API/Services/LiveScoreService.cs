using TennisScores.Domain;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Repositories;

namespace TennisScores.API.Services;

public class LiveScoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchFormatRepository _matchFormatRepository;

    public LiveScoreService(IUnitOfWork unitOfWork, IMatchRepository matchRepository, IMatchFormatRepository matchFormatRepository)
    {
        _unitOfWork = unitOfWork;
        _matchRepository = matchRepository;
        _matchFormatRepository = matchFormatRepository;
    }

    public async Task AddPointToMatchAsync(Guid matchId, Guid winnerId)
    {
        var match = await _matchRepository.GetMatchWithDetailsAsync(matchId) ?? throw new Exception("Match not found");
        var format = await _matchFormatRepository.GetByIdAsync(match.Tournament!.MatchFormat.Id) ?? throw new Exception("Match format not found");
        var currentSet = match.Sets.OrderByDescending(s => s.SetNumber).FirstOrDefault();
        if (currentSet == null)
        {
            currentSet = new TennisSet { Match = match, MatchId = match.Id, SetNumber = 1 };
            match.Sets.Add(currentSet);
        }

        var currentGame = currentSet.Games.OrderByDescending(g => g.GameNumber).FirstOrDefault();
        if (currentGame == null || currentGame.WinnerId != null)
        {
            currentGame = new Game
            {
                Set = currentSet,
                SetId = currentSet.Id,
                GameNumber = currentSet.Games.Count + 1,
                Points = []
            };
            currentSet.Games.Add(currentGame);
        }

        // Ajouter le point
        var point = new Point
        {
            GameId = currentGame.Id,
            WinnerId = winnerId,
            Timestamp = DateTime.UtcNow
        };
        currentGame.Points.Add(point);

        // Calculer le score
        var pointsP1 = currentGame.Points.Count(p => p.WinnerId == match.Player1Id);
        var pointsP2 = currentGame.Points.Count(p => p.WinnerId == match.Player2Id);

        // Points classiques
        bool isDeuce = pointsP1 >= 3 && pointsP2 >= 3 && pointsP1 == pointsP2;
        bool hasAdvantage = Math.Abs(pointsP1 - pointsP2) == 1 && (pointsP1 >= 3 || pointsP2 >= 3);

        bool shouldEndGame = (pointsP1 >= 4 || pointsP2 >= 4) &&
            ((format.DecidingPointEnabled && pointsP1 == 4 || pointsP2 == 4) ||
            (!format.DecidingPointEnabled && Math.Abs(pointsP1 - pointsP2) >= 2));

        if (shouldEndGame)
        {
            currentGame.WinnerId = pointsP1 > pointsP2 ? match.Player1Id : match.Player2Id;
        }

        // Fin de set ?
        int gamesP1 = currentSet.Games.Count(g => g.WinnerId == match.Player1Id);
        int gamesP2 = currentSet.Games.Count(g => g.WinnerId == match.Player2Id);

        bool isTieBreak = format.TieBreakEnabled &&
            gamesP1 == format.GamesPerSet &&
            gamesP2 == format.GamesPerSet;

        bool setWon = (gamesP1 >= format.GamesPerSet || gamesP2 >= format.GamesPerSet) &&
            Math.Abs(gamesP1 - gamesP2) >= 2;

        if (setWon || isTieBreak)
        {
            currentSet.WinnerId = gamesP1 > gamesP2 ? match.Player1Id : match.Player2Id;
            currentSet.IsCompleted = true;

            // Super Tie-break ?
            int setsP1 = match.Sets.Count(s => s.WinnerId == match.Player1Id);
            int setsP2 = match.Sets.Count(s => s.WinnerId == match.Player2Id);

            bool isFinalSet = (setsP1 + setsP2 + 1 == format.SetsToWin * 2 - 1);
            if (isFinalSet && format.SuperTieBreakForFinalSet)
            {
                // prochain set : super tie-break
                var superSet = new TennisSet { Match = match, MatchId = match.Id, SetNumber = currentSet.SetNumber + 1 };
                match.Sets.Add(superSet);
            }
        }

        // Fin de match ?
        int setsWonByP1 = match.Sets.Count(s => s.WinnerId == match.Player1Id);
        int setsWonByP2 = match.Sets.Count(s => s.WinnerId == match.Player2Id);

        if (setsWonByP1 == format.SetsToWin || setsWonByP2 == format.SetsToWin)
        {
            match.IsCompleted = true;
            match.WinnerId = setsWonByP1 > setsWonByP2 ? match.Player1Id : match.Player2Id;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private bool IsSuperTiebreak(Match match, TennisSet set)
    {
        return match.BestOfSets == 3 && set.SetNumber == 3;
    }

    private Guid? GetGameWinner(Match match, Game game)
    {
        int p1 = game.Player1Points;
        int p2 = game.Player2Points;

        if (game.IsTiebreak)
        {
            int target = IsSuperTiebreak(match, game.Set) ? 10 : 7;

            if (p1 >= target && p1 - p2 >= 2) return match.Player1Id;
            if (p2 >= target && p2 - p1 >= 2) return match.Player2Id;
            return null;
        }

        if (p1 >= 4 && p1 - p2 >= 2) return match.Player1Id;
        if (p2 >= 4 && p2 - p1 >= 2) return match.Player2Id;

        return null;
    }

    private Guid? GetSetWinner(Match match, TennisSet set)
    {
        int p1 = set.Player1Games;
        int p2 = set.Player2Games;

        if (p1 >= 6 && p1 - p2 >= 2) return match.Player1Id;
        if (p2 >= 6 && p2 - p1 >= 2) return match.Player2Id;

        return null;
    }
}
