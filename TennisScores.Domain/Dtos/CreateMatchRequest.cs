using System.ComponentModel.DataAnnotations;

namespace TennisScores.Domain.Dtos;

public class CreateMatchRequest
{
    [Required]
    public string Player1FirstName { get; set; } = string.Empty;
    [Required]
    public string Player1LastName { get; set; } = string.Empty;
    [Required]
    public string Player2FirstName { get; set; } = string.Empty;
    [Required]
    public string Player2LastName { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "BestOfSets must be between 1 and 5.")]
    [Required]
    [Display(Name = "Best of Sets")]
    public int BestOfSets { get; set; } = 3;

    [Required]
    public string ServingPlayerLastName { get; set; } = string.Empty;
    
    [Required]
    public string ServingPlayerFirstName { get; set; } = string.Empty;

    public string? TournamentName { get; set; }
    public DateTime? TournamentStartDate { get; set; }
}