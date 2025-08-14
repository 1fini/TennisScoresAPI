using System.Text.Json.Serialization;

namespace TennisScores.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TournamentSubType
{
    None, // No specific subtype
    GrandChelem, // Tournoi Multi-Chances Grand Chelem
    ChallengeTerritorial, // Tournoi Multi-Chances Challenge Territorial
    ChallengeDepartemental, // Tournoi Multi-Chances Challenge Départemental
    GrandChelemChallenge, // Tournoi Multi-Chances Grand Chelem Challenge
}
