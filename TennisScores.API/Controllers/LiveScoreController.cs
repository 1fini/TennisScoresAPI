namespace TennisScores.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using TennisScores.API.Services;
using TennisScores.Domain.Dtos;
using System.Threading.Tasks;

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

    [HttpPost("{matchId:guid}/undo-last-point")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UndoLastPointAsync(Guid matchId)
    {
        try
        {
            await _liveScoringService.UndoLastPointAsync(matchId);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (UndoNotAvailableException exception)
        {
            return Conflict(new { message = exception.Message });
        }

        return Ok(new { message = "Dernier point annulé avec succès." });
    }
}
