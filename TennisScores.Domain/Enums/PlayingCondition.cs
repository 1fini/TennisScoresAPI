using System.Text.Json.Serialization;

namespace TennisScores.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlayingCondition
{
    Indoor, // Indoor conditions
    Outdoor, // Outdoor conditions
    Mixed // Mixed conditions (indoor/outdoor)
}
