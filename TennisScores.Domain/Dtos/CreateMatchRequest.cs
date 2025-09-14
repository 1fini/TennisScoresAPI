using System.ComponentModel.DataAnnotations;

namespace TennisScores.Domain.Dtos;

public class CreateMatchRequest
{
    /// <summary>
    /// The ID of the first player.
    /// </summary>
    [Required]
    public Guid Player1Id { get; set; }
    /// <summary>
    /// The ID of the second player.
    /// </summary>
    [Required]
    public Guid Player2Id { get; set; }

    /// <summary>
    /// The number of sets required to win the match. Must be between 1 and 5.
    /// </summary>
    [Range(1, 5, ErrorMessage = "BestOfSets must be between 1 and 5.")]
    [Display(Name = "Best of Sets")]
    public int BestOfSets { get; set; } = 3;

    /// <summary>
    /// The ID of the player who is serving first.
    /// </summary>
    [Required]
    public Guid ServingPlayer { get; set; }

    /// <summary>
    /// The ID of the tournament the match is part of, if applicable.
    /// </summary>
    public Guid? TournamentId { get; set; }

    /// <summary>
    /// The date and time when the match is scheduled to take place. Defaults to the current date and time if not provided.
    /// </summary>
    public DateTime MatchDate { get; set; } = DateTime.UtcNow;

    // Validation to ensure either BestOfSets or TournamentId is provided, but not both
    /*public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (BestOfSets is null && TournamentId is null)
        {
            yield return new ValidationResult(
                "Either BestOfSets or TournamentId must be provided.",
                new[] { nameof(BestOfSets), nameof(TournamentId) }
            );
        }

        if (BestOfSets is not null && TournamentId is not null)
        {
            yield return new ValidationResult(
                "You cannot provide both BestOfSets and TournamentId at the same time.",
                new[] { nameof(BestOfSets), nameof(TournamentId) }
            );
        }
    }*/
}