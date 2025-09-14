
using TennisScores.Domain.Enums;

namespace TennisScores.Domain.Dtos;
public class AddPointRequest
{
    public Guid MatchId { get; set; }
    public Guid WinnerId { get; set; }
    public PointType PointType { get; set; } // Exemple : Ace, Fault, Winner, etc.
}
