using System.Text.Json.Serialization;

namespace TennisScores.Domain.Enums;

/// <summary>
/// Enum representing FFT (Fédération Française de Tennis) rankings.
/// These rankings are used to classify players in various series.
/// The rankings range from non-classé (NC) to specific top rankings.
/// Each ranking corresponds to a specific level of play, with NC being the lowest and top
/// NC et 4e série	
/// NC	
/// 40	
/// 30/5	
/// 30/4	
/// 30/3	
/// 30/2	
/// 30/1	
/// 3e série	
/// 30	
/// 15/5	
/// 15/4	
/// 15/3	
/// 15/2	
/// 15/1	
/// 2e série	
/// 15	
/// 5/6	
/// 4/6	
/// 3/6	
/// 2/6	
/// 1/6	
/// 0	
/// -2/6	
/// -4/6	
/// -15	
/// Top 60 Dames Top 100 Messieurs	
/// Top 40 Dames Top 60 Messieurs	
/// 1re série	
/// Top 20 Dames
/// Top 30 Messieurs
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FftRanking
{
    NC,        // Non classé, 4e série
    Quarante,  // 40
    TrenteCinq,// 30/5
    TrenteQuatre, // 30/4
    TrenteTrois,  // 30/3
    TrenteDeux,   // 30/2
    TrenteUn,    // 30/1

    Trente,     // 30 (3e série)
    QuinzeCinq, // 15/5
    QuinzeQuatre, // 15/4
    QuinzeTrois,  // 15/3
    QuinzeDeux,   // 15/2
    QuinzeUn,    // 15/1

    Quinze,     // 15 (2e série)
    CinqSix,    // 5/6
    QuatreSix,  // 4/6
    TroisSix,   // 3/6
    DeuxSix,    // 2/6
    UnSix,      // 1/6
    Zero,       // 0
    MoinsDeuxSix,  // -2/6
    MoinsQuatreSix,// -4/6
    MoinsQuinze,   // -15

    // Rankings spécifiques / tops
    Top60DamesTop100Messieurs,
    Top40DamesTop60Messieurs,
    Top20DamesTop30Messieurs // 1re série
}

