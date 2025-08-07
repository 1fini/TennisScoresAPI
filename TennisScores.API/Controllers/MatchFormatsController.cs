using Microsoft.AspNetCore.Mvc;
using TennisScores.API.Services;
using TennisScores.Domain.Entities;

namespace TennisScores.API.Controllers;

/// <summary>
/// Controller for managing match formats.
/// Provides endpoints to retrieve match formats.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MatchFormatsController : ControllerBase
{
    private readonly IMatchFormatService _service;

    public MatchFormatsController(IMatchFormatService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchFormat>>> GetAll()
    {
        var formats = await _service.GetAllAsync();
        return Ok(formats);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MatchFormat>> GetById(int id)
    {
        var format = await _service.GetByIdAsync(id);
        if (format == null)
        {
            return NotFound();
        }
        return Ok(format);
    }
}