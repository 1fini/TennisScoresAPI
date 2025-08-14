using System.Text.Json.Serialization;

namespace TennisScores.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TournamentType
{
    Open,
    TMC,  // Tournoi Multi-Chances
    GPRJ, // Grand Prix Régional Jeunes
    TournoiFédéral, // Tournoi Fédéral Catégorie 2
    TournoiNational, // Tournoi National
    CNE // Championnat National des Équipes
}
