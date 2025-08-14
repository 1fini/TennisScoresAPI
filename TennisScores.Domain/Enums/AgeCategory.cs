using System.Text.Json.Serialization;

namespace TennisScores.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgeCategory
{
    Under10,
    Under12,
    Under14,
    Under16,
    Under18,
    Senior,
    Veteran,
    Open, // Open category for all ages
}
