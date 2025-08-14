using System.Text.Json.Serialization;
using TennisScores.Domain.Enums;

namespace TennisScores.Domain.Dtos;

    public class TournamentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Location { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }

        public string? MinRankingFft { get; set; }
        public string? MaxRankingFft { get; set; }

        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AgeCategory? AgeCategory { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TournamentType Type { get; set; }

        public MatchFormatDto MatchFormat { get; set; } = default!;
        public int MatchFormatId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TournamentSubType SubType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BallLevel BallLevel { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CourtSurface Surface { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayingCondition Condition { get; set; }

        public decimal? PrizeMoney { get; set; }
        public string? PrizeMoneyCurrency { get; set; }

        public List<MatchDto> Matches { get; set; } = new();
    }