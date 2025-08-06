using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TennisScores.Domain.Entities;

namespace TennisScores.Infrastructure.Data;

public class MatchFormatConfiguration : IEntityTypeConfiguration<MatchFormat>
{
    public void Configure(EntityTypeBuilder<MatchFormat> builder)
    {
        builder.HasKey(f => f.Id);

        builder.HasData(
        [
            new MatchFormat
            {
                Id = 1,
                Name = "Format 1",
                SetsToWin = 3,
                GamesPerSet = 6,
                TieBreakEnabled = true,
                DecidingPointEnabled = false,
                SuperTieBreakForFinalSet = false,
                Application = "Format traditionnel"
            },
            new MatchFormat
            {
                Id = 2,
                Name = "Format 2",
                SetsToWin = 2,
                GamesPerSet = 6,
                TieBreakEnabled = true,
                DecidingPointEnabled = false,
                SuperTieBreakForFinalSet = true,
                Application = "Format officiel 65+"
            },
            new MatchFormat
            {
                Id = 3,
                Name = "Format 3",
                SetsToWin = 2,
                GamesPerSet = 4,
                TieBreakEnabled = true,
                DecidingPointEnabled = true,
                SuperTieBreakForFinalSet = true,
                Application = "Format TMC"
            },
            new MatchFormat
            {
                Id = 4,
                Name = "Format 4",
                SetsToWin = 2,
                GamesPerSet = 6,
                TieBreakEnabled = true,
                DecidingPointEnabled = true,
                SuperTieBreakForFinalSet = true,
                Application = "Double Format"
            },
            new MatchFormat
            {
                Id = 5,
                Name = "Format 5",
                SetsToWin = 2,
                GamesPerSet = 3,
                TieBreakEnabled = true,
                DecidingPointEnabled = true,
                SuperTieBreakForFinalSet = true,
                Application = "TMC à partir de 8 ans"
            },
            new MatchFormat
            {
                Id = 6,
                Name = "Format 6",
                SetsToWin = 2,
                GamesPerSet = 4,
                TieBreakEnabled = true, //tiebreak at 3/3
                DecidingPointEnabled = true,
                SuperTieBreakForFinalSet = true,
                Application = "TMC 11–15 ans"
            },
            new MatchFormat
            {
                Id = 7,
                Name = "Format 7",
                SetsToWin = 2,
                GamesPerSet = 5,
                TieBreakEnabled = true, //tiebreak at 4/4
                DecidingPointEnabled = true,
                SuperTieBreakForFinalSet = true,
                Application = "TMC 11–15 ans"
            }
        ]);
    }
}
