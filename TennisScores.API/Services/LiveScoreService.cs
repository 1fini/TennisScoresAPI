// TennisScores.API/Services/LiveScoreService.cs
using TennisScores.Domain;
using TennisScores.Domain.Repositories;

namespace TennisScores.API.Services
{
    public class LiveScoreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMatchRepository _matchRepository;

        public LiveScoreService(IUnitOfWork unitOfWork, IMatchRepository matchRepository)
        {
            _unitOfWork = unitOfWork;
            _matchRepository = matchRepository;
        }

        // Exemple simplifié d'une méthode qui enregistre un point gagné
        public async Task<bool> AddPointToMatchAsync(Guid matchId, Guid playerId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                return false;

            // Logique simplifiée : ajouter un point dans le dernier jeu du dernier set
            var currentSet = match.Sets.OrderByDescending(s => s.SetNumber).FirstOrDefault();
            if (currentSet == null)
                throw new InvalidOperationException("Le match n'a pas encore de set.");

            var currentGame = currentSet.Games.OrderByDescending(g => g.GameNumber).FirstOrDefault();
            if (currentGame == null)
                throw new InvalidOperationException("Le set n'a pas encore de jeu.");

            // Exemple : mettre à jour le score du jeu selon le playerId
            // Ici, il faudrait implémenter ta logique métier pour incrémenter le score.

            // Par exemple fictif, on indique le gagnant du point (à adapter selon ta logique)
            //currentGame.PointsWonByPlayerId = playerId;

            // Sauvegarder les changements
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
