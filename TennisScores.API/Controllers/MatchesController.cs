using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Dtos;
using TennisScores.API.Services;

namespace TennisScoresAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        //private readonly TennisDbContext _context;
        private readonly IMatchService _matchService;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(IMatchService matchService, ILogger<MatchesController> logger)
        {
            _matchService = matchService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch(CreateMatchRequest request)
        {
            _logger.LogDebug("Creating match between {Player1} and {Player2}", request.Player1FirstName, request.Player2FirstName);

            /* // Vérifier ou créer les joueurs
            var player1 = await _context.Players.FirstOrDefaultAsync(p =>
                                    p.FirstName.ToLower() == request.Player1FirstName.ToLower() &&
                                    p.LastName.ToLower() == request.Player1LastName.ToLower()
                                ) ?? new Player
                                {
                                    FirstName = request.Player1FirstName,
                                    LastName = request.Player1LastName
                                };
            var player2 = await _context.Players.FirstOrDefaultAsync(p =>
                                    p.FirstName.ToLower() == request.Player2FirstName.ToLower() &&
                                    p.LastName.ToLower() == request.Player2LastName.ToLower()
                                ) ?? new Player
                                {
                                    FirstName = request.Player2FirstName,
                                    LastName = request.Player2LastName
                                };

            if (player1.Id == Guid.Empty)
                _context.Players.Add(player1);
            if (player2.Id == Guid.Empty)
                _context.Players.Add(player2);

            Tournament? tournament = null;

            if (!string.IsNullOrWhiteSpace(request.TournamentName))
            {
                tournament = await _context.Tournaments
                    .FirstOrDefaultAsync(t => t.Name == request.TournamentName);

                if (tournament == null)
                {
                    return BadRequest($"Tournament with name '{request.TournamentName}' not found.");
                }
            }

            await _context.SaveChangesAsync();

            var match = new Match
            {
                Player1Id = player1.Id,
                Player2Id = player2.Id,
                BestOfSets = request.BestOfSets,
                StartTime = DateTime.UtcNow
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();

            var matchDto = new MatchDto
            {
                Id = match.Id,
                Player1FirstName = player1.FirstName,
                Player1LastName = player1.LastName,
                Player2FirstName = player2.FirstName,
                Player2LastName = player2.LastName,
                BestOfSets = match.BestOfSets,
                StartTime = match.StartTime,
                EndTime = match.EndTime,
                Surface = match.Surface,
                WinnerFirstName = null,  // To update when match is finished
                WinnerLastName = null  // To update when match is finished
            };

            return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, matchDto); */
            
            var result = await _matchService.CreateMatchAsync(request);
            _logger.LogInformation("Match created successfully with ID {MatchId}", result.Id);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMatch(Guid id)
        {
            var match = await _matchService.GetMatchAsync(id);
            
            return match is null ? NotFound() : Ok(match);
        }
    }
}
