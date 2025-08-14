
using System.Text.Json.Serialization;

namespace TennisScores.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PointType
{
    Unknown = 0,

    // Services
    Ace = 1,
    DoubleFault = 2,
    Fault = 3,

    // Échanges
    Winner = 4,
    UnforcedError = 5,
    ForcedError = 6,

    // Autres (optionnels)
    Let = 7,
    TimeViolation = 8
}
