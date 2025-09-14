
using System.Text.Json.Serialization;

namespace TennisScores.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PointType
{
    Unknown = 0,
    Ace = 1,
    DoubleFault = 2,
    Fault = 3,
    Winner = 4,
    UnforcedError = 5,
    ForcedError = 6,
    Let = 7,
    TimeViolation = 8
}
