using TennisScores.Domain;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;

namespace TennisScores.API.Services;

public class LiveScoreServiceCopy
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchRepository _matchRepository;

    public LiveScoreServiceCopy(IUnitOfWork unitOfWork, IMatchRepository matchRepository)
    {
        _unitOfWork = unitOfWork;
        _matchRepository = matchRepository;
    }

    public async Task<bool> AddPointToMatchAsync(Guid matchId, Guid playerId)
    {
        var match = await _matchRepository.GetFullMatchByIdAsync(matchId);
        if (match == null) return false;


        EnsureInitialSetAndGame(match);

        var currentSet = match.Sets.OrderByDescending(s => s.SetNumber).FirstOrDefault();
        if (currentSet == null) throw new InvalidOperationException("Le match n'a pas encore de set.");

        var currentGame = currentSet.Games.OrderByDescending(g => g.GameNumber).FirstOrDefault();
        if (currentGame == null) throw new InvalidOperationException("Le set n'a pas encore de jeu.");

        // Créer le point
        var point = new Point
        {
            GameId = currentGame.Id,
            Game = currentGame,
            PointNumber = currentGame.Points.Count + 1,
            WinnerId = playerId,
            Timestamp = DateTime.UtcNow
        };
        currentGame.Points.Add(point);

        // Incrémenter le score du jeu
        if (playerId == match.Player1Id) currentGame.Player1Points++;
        else if (playerId == match.Player2Id) currentGame.Player2Points++;
        else throw new InvalidOperationException("Le joueur ne participe pas à ce match.");

        // Vérifier si le jeu est gagné
        var winnerId = GetGameWinner(match, currentGame);
        if (winnerId != null)
        {
            currentGame.WinnerId = winnerId;
            currentGame.Winner = (winnerId == match.Player1Id) ? match.Player1 : match.Player2;

            if (winnerId == match.Player1Id) currentSet.Player1Games++;
            else currentSet.Player2Games++;

            // Vérifier si le set est gagné
            var setWinnerId = GetSetWinner(match, currentSet);
            if (setWinnerId != null)
            {
                currentSet.WinnerId = setWinnerId;
                currentSet.Winner = (setWinnerId == match.Player1Id) ? match.Player1 : match.Player2;

                var totalSetsWonByP1 = match.Sets.Count(s => s.WinnerId == match.Player1Id);
                var totalSetsWonByP2 = match.Sets.Count(s => s.WinnerId == match.Player2Id);
                var setsToWin = (match.BestOfSets == 5) ? 3 : 2;

                if (totalSetsWonByP1 == setsToWin || totalSetsWonByP2 == setsToWin)
                {
                    match.WinnerId = (totalSetsWonByP1 == setsToWin) ? match.Player1Id : match.Player2Id;
                    match.EndTime = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // Créer un nouveau set
                    var newSet = new TennisSet
                    {
                        MatchId = match.Id,
                        Match = match,
                        SetNumber = currentSet.SetNumber + 1
                    };

                    if (IsSuperTiebreak(match, newSet))
                    {
                        newSet.Games.Add(new Game
                        {
                            SetId = newSet.Id,
                            Set = newSet,
                            GameNumber = 1,
                            IsTiebreak = true
                        });
                    }

                    match.Sets.Add(newSet);
                }
            }
            else
            {
                // Nouveau jeu
                var newGame = new Game
                {
                    SetId = currentSet.Id,
                    Set = currentSet,
                    GameNumber = currentSet.Games.Count + 1,
                    IsTiebreak = IsTiebreakNeeded(currentSet) || IsSuperTiebreak(match, currentSet)
                };

                currentSet.Games.Add(newGame);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static void EnsureInitialSetAndGame(Match match)
    {
        if (match.Sets.Count == 0)
        {
            var set = new TennisSet
            {
                Match = match,
                MatchId = match.Id,
                SetNumber = 1
            };
            var gameInit = new Game
            {
                Set = set,
                SetId = set.Id,
                GameNumber = 1
            };
            set.Games.Add(gameInit);
            match.Sets.Add(set);
        }
        else
        {
            var currentSet = match.Sets.OrderByDescending(s => s.SetNumber).First();
            if (currentSet.Games.Count == 0)
            {
                var gameInit2 = new Game
                {
                    Set = currentSet,
                    SetId = currentSet.Id,
                    GameNumber = 1
                };
                currentSet.Games.Add(gameInit2);
            }
        }
    }

    private bool IsTiebreakNeeded(TennisSet set)
    {
        return set.Player1Games == 6 && set.Player2Games == 6;
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
