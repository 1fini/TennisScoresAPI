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
                BallColor = "Jaune",
                NumberOfSets = 3,
                GamesPerSet = 6,
                TieBreakEnabled = true,
                NoAdvantage = false,
                SuperTieBreakEnabled = false,
                Application = "Format traditionnel"
            },
            new MatchFormat
            {
                Id = 2,
                Name = "Format 2",
                BallColor = "Jaune",
                NumberOfSets = 2,
                GamesPerSet = 6,
                TieBreakEnabled = true,
                NoAdvantage = false,
                SuperTieBreakEnabled = true,
                Application = "Format officiel 65+"
            },
            new MatchFormat
            {
                Id = 3,
                Name = "Format 3",
                BallColor = "Vert, Jaune",
                NumberOfSets = 3,
                GamesPerSet = 4,
                TieBreakEnabled = true,
                NoAdvantage = true,
                SuperTieBreakEnabled = true,
                Application = "Format TMC"
            },
            new MatchFormat
            {
                Id = 4,
                Name = "Format 4",
                BallColor = "Jaune",
                NumberOfSets = 2,
                GamesPerSet = 6,
                TieBreakEnabled = true,
                NoAdvantage = true,
                SuperTieBreakEnabled = true,
                Application = "Format double"
            },
            new MatchFormat
            {
                Id = 5,
                Name = "Format 5",
                BallColor = "Orange, Vert, Jaune",
                NumberOfSets = 2,
                GamesPerSet = 3,
                TieBreakEnabled = true,
                NoAdvantage = true,
                SuperTieBreakEnabled = true,
                Application = "TMC à partir de 8 ans"
            },
            new MatchFormat
            {
                Id = 6,
                Name = "Format 6",
                BallColor = "Orange, Vert, Jaune",
                NumberOfSets = 2,
                GamesPerSet = 4,
                TieBreakEnabled = true,
                NoAdvantage = true,
                SuperTieBreakEnabled = true,
                Application = "TMC 11–15 ans"
            },
            new MatchFormat
            {
                Id = 7,
                Name = "Format 7",
                BallColor = "Vert, Jaune",
                NumberOfSets = 2,
                GamesPerSet = 5,
                TieBreakEnabled = true,
                NoAdvantage = true,
                SuperTieBreakEnabled = true,
                Application = "TMC 11–15 ans"
            }
        ]);
    }
}
