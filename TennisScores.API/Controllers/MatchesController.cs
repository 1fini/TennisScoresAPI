using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TennisScores.Domain.Entities;
using TennisScores.Domain.Dtos;
using TennisScores.API.Services;
using TennisScores.Domain.Repositories;

namespace TennisScoresAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService _matchService;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(
            IMatchService matchService,
            ILogger<MatchesController> logger)
        {
            _matchService = matchService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var match = await _matchService.CreateMatchAsync(request);

            return CreatedAtAction(nameof(CreateMatch), new { id = match.Id }, match);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatchDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMatch(Guid id)
        {
            var match = await _matchService.GetMatchAsync(id);

            return match is null ? NotFound() : Ok(match);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MatchDto>))]
        public async Task<IActionResult> GetAllMatches()
        {
            var matches = await _matchService.GetAllAsync();
            return Ok(matches);
        }
    }
}
