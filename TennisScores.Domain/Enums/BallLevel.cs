using System.Text.Json.Serialization;

namespace TennisScores.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BallLevel
{
    White, // Niveau débutant
    Purple, // Niveau débutant +
    Red, // Niveau intermédiaire
    Orange, // Niveau intermédiaire +
    Green, // Niveau confirmé
    Yellow // Niveau avancé
}
