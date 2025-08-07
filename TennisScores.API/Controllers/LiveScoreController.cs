namespace TennisScores.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using TennisScores.API.Services;
using TennisScores.Domain.Dtos;
using System.Threading.Tasks;
using TennisScores.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
public class LiveScoreController : ControllerBase
{
    private readonly ILiveScoreService _liveScoringService;

    public LiveScoreController(ILiveScoreService liveScoringService)
    {
        _liveScoringService = liveScoringService;
    }

    [HttpPost("add-point")]
    public async Task<IActionResult> AddPointAsync([FromBody] AddPointRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _liveScoringService.AddPointToMatchAsync(
            request.MatchId,
            request.WinnerId,
            request.PointType);

        return Ok(new { message = "Point ajouté avec succès." });
    }
}
