using Microsoft.AspNetCore.Mvc;
using TennisScores.API.Services;
using TennisScores.Domain.Dtos;

namespace TennisScores.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentService _tournamentService;

    public TournamentsController(ITournamentService tournamentService)
    {
        _tournamentService = tournamentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTournamentRequest request)
    {
        var id = await _tournamentService.CreateTournamentAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tournament = await _tournamentService.GetByIdAsync(id);
        if (tournament == null) return NotFound();
        return Ok(tournament);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tournaments = await _tournamentService.GetAllAsync();
        return Ok(tournaments);
    }
}
