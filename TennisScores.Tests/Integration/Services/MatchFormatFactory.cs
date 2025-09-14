using TennisScores.Domain.Entities;

namespace TennisScores.Tests.Integration.Services;

public static class MatchFormatFactory
{
    public static MatchFormat CreateMatchFormat2()
    {
        var matchFormat = new MatchFormat
        {
            Id = 2,
            DecidingPointEnabled = false,
            Application = "",
            GamesPerSet = 6,
            Name = "Format 2",
            SetsToWin = 2,
            SuperTieBreakForFinalSet = false,
            SuperTieBreakPoints = 10,
            TieBreakEnabled = true,
            TieBreakPoints = 7,
        };

        return matchFormat;
    }
}