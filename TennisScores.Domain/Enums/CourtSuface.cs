using System.Text.Json.Serialization;

namespace TennisScores.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CourtSurface
{
    Clay, // Clay
    Grass, // Grass
    Hard, // Hard
    Carpet, // Carpet
    Synthetic, // Synthetic
    Other
}
