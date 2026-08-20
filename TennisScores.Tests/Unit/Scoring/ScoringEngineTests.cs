using System.Collections.Immutable;
using System.Text.Json;
using TennisScores.Domain.Enums;
using TennisScores.Domain.Scoring;

namespace TennisScores.Tests.Unit.Scoring;

public class ScoringEngineTests
{
    private static readonly Guid Player1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Player2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTime FirstPointAt = new(2026, 8, 20, 18, 0, 0, DateTimeKind.Utc);
    private readonly ScoringEngine _engine = new();

    [Fact]
    public void Apply_SameStateAndCommand_ProducesEquivalentResult()
    {
        var state = NewState(TraditionalFormat());
        var command = new AwardPoint(Player1Id, PointType.Ace, FirstPointAt);

        var first = _engine.Apply(state, command);
        var second = _engine.Apply(state, command);

        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        Assert.Empty(state.Sets);
    }

    [Fact]
    public void Apply_FourStraightPoints_CompletesGameAndSwitchesServerOnce()
    {
        var state = NewState(TraditionalFormat());

        state = AwardMany(state, Player1Id, 4);

        var firstSet = Assert.Single(state.Sets);
        var completedGame = firstSet.Games.Single(game => game.GameNumber == 1);
        Assert.True(completedGame.IsCompleted);
        Assert.Equal(Player1Id, completedGame.WinnerId);
        Assert.Equal(Player2Id, state.ServingPlayerId);
        Assert.Single(firstSet.Games, game => !game.IsCompleted);

        state = Award(state, Player2Id);
        Assert.Equal(Player2Id, state.ServingPlayerId);
    }

    [Fact]
    public void Apply_AdvantageReturnsToDeuce_RequiresTwoPointMargin()
    {
        var state = NewState(TraditionalFormat());
        state = AwardMany(state, Player1Id, 3);
        state = AwardMany(state, Player2Id, 3);
        state = Award(state, Player1Id);
        state = Award(state, Player2Id);

        var gameAtDeuce = state.Sets.Single().Games.Single();
        Assert.False(gameAtDeuce.IsCompleted);
        Assert.Equal(4, gameAtDeuce.Points.Count(point => point.WinnerId == Player1Id));
        Assert.Equal(4, gameAtDeuce.Points.Count(point => point.WinnerId == Player2Id));

        state = Award(state, Player2Id);
        Assert.False(state.Sets.Single().Games.Single().IsCompleted);

        state = Award(state, Player2Id);
        Assert.True(state.Sets.Single().Games.Single(game => game.GameNumber == 1).IsCompleted);
    }

    [Fact]
    public void Apply_NoAdAtDeuce_CompletesOnDecidingPoint()
    {
        var state = NewState(TraditionalFormat() with { DecidingPointEnabled = true });
        state = AwardMany(state, Player1Id, 3);
        state = AwardMany(state, Player2Id, 3);

        state = Award(state, Player2Id);

        var completedGame = state.Sets.Single().Games.Single(game => game.GameNumber == 1);
        Assert.True(completedGame.IsCompleted);
        Assert.Equal(Player2Id, completedGame.WinnerId);
    }

    [Fact]
    public void Apply_AtSixAll_UsesTieBreakWinByTwoAndRotatesAfterOddPoints()
    {
        var state = NewState(TraditionalFormat());
        for (var game = 0; game < 6; game++)
        {
            state = WinGame(state, Player1Id);
            state = WinGame(state, Player2Id);
        }

        var tieBreak = state.Sets.Single().Games.Single(game => !game.IsCompleted);
        Assert.True(tieBreak.IsTiebreak);
        Assert.Equal(13, tieBreak.GameNumber);

        var serverBeforeTieBreak = state.ServingPlayerId;
        state = Award(state, Player1Id);
        Assert.NotEqual(serverBeforeTieBreak, state.ServingPlayerId);
        var serverAfterFirstPoint = state.ServingPlayerId;

        state = Award(state, Player2Id);
        Assert.Equal(serverAfterFirstPoint, state.ServingPlayerId);
        state = Award(state, Player1Id);
        Assert.Equal(serverBeforeTieBreak, state.ServingPlayerId);

        state = AwardMany(state, Player1Id, 4);
        state = AwardMany(state, Player2Id, 5);
        Assert.False(state.Sets.Single().Games.Single(game => game.GameNumber == 13).IsCompleted);

        state = Award(state, Player1Id);
        Assert.False(state.Sets.Single().Games.Single(game => game.GameNumber == 13).IsCompleted);
        state = Award(state, Player1Id);
        Assert.True(state.Sets.Single(set => set.SetNumber == 1).IsCompleted);
    }

    [Fact]
    public void Apply_FinalSetSuperTieBreak_CompletesAtElevenNine()
    {
        var format = TraditionalFormat() with { SuperTieBreakForFinalSet = true };
        var state = NewState(format);
        state = WinSet(state, Player1Id);
        state = WinSet(state, Player2Id);

        state = AwardMany(state, Player1Id, 9);
        state = AwardMany(state, Player2Id, 9);
        state = Award(state, Player1Id);

        var atTenNine = state.Sets.Single(set => set.SetNumber == 3).Games.Single();
        Assert.True(atTenNine.IsTiebreak);
        Assert.False(atTenNine.IsCompleted);

        state = Award(state, Player1Id);

        var completedSuperTieBreak = state.Sets.Single(set => set.SetNumber == 3).Games.Single();
        Assert.True(completedSuperTieBreak.IsCompleted);
        Assert.True(state.IsCompleted);
        Assert.Equal(Player1Id, state.WinnerId);
    }

    [Fact]
    public void Apply_TraditionalFinalSetAtSixAll_UsesRegularTieBreak()
    {
        var state = NewState(TraditionalFormat());
        state = WinSet(state, Player1Id);
        state = WinSet(state, Player2Id);
        for (var game = 0; game < 6; game++)
        {
            state = WinGame(state, Player1Id);
            state = WinGame(state, Player2Id);
        }

        var finalSetTieBreak = state.Sets
            .Single(set => set.SetNumber == 3)
            .Games.Single(game => !game.IsCompleted);

        Assert.True(finalSetTieBreak.IsTiebreak);
        Assert.Equal(13, finalSetTieBreak.GameNumber);
    }

    [Fact]
    public void Apply_MatchWinningPoint_SetsWinnerAndUsesCommandTimestamp()
    {
        var state = NewState(TraditionalFormat());
        state = WinSet(state, Player1Id);

        for (var game = 0; game < 5; game++)
            state = WinGame(state, Player1Id);
        for (var point = 0; point < 3; point++)
            state = Award(state, Player1Id);

        var winningPointAt = NextPointAt(state);
        state = _engine.Apply(
            state,
            new AwardPoint(Player1Id, PointType.Winner, winningPointAt)).State;

        Assert.True(state.IsCompleted);
        Assert.Equal(Player1Id, state.WinnerId);
        Assert.Equal(winningPointAt, state.EndTime);
        Assert.Equal(2, state.Sets.Length);
    }

    [Fact]
    public void Apply_CompletedMatch_RejectsPointWithoutChangingState()
    {
        var state = NewState(TraditionalFormat()) with { IsCompleted = true };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _engine.Apply(state, new AwardPoint(Player1Id, PointType.Winner, FirstPointAt)));

        Assert.Equal("Cannot add a point to a completed match.", exception.Message);
        Assert.Empty(state.Sets);
    }

    [Fact]
    public void Apply_WinnerOutsideMatch_RejectsPoint()
    {
        var state = NewState(TraditionalFormat());

        var exception = Assert.Throws<ArgumentException>(() =>
            _engine.Apply(state, new AwardPoint(Guid.NewGuid(), PointType.Winner, FirstPointAt)));

        Assert.Equal("winnerId", exception.ParamName);
    }

    private static ScoringFormat TraditionalFormat()
        => new(
            SetsToWin: 2,
            GamesPerSet: 6,
            TieBreakEnabled: true,
            DecidingPointEnabled: false,
            SuperTieBreakForFinalSet: false,
            TieBreakPoints: 7,
            SuperTieBreakPoints: 10);

    private static ScoringState NewState(ScoringFormat format)
        => new(
            Player1Id,
            Player2Id,
            ServingPlayerId: Player1Id,
            IsCompleted: false,
            WinnerId: null,
            EndTime: null,
            format,
            Sets: ImmutableArray<ScoringSet>.Empty);

    private ScoringState Award(
        ScoringState state,
        Guid winnerId,
        PointType pointType = PointType.Winner)
        => _engine.Apply(
            state,
            new AwardPoint(winnerId, pointType, NextPointAt(state))).State;

    private ScoringState AwardMany(ScoringState state, Guid winnerId, int count)
    {
        for (var point = 0; point < count; point++)
            state = Award(state, winnerId);

        return state;
    }

    private ScoringState WinGame(ScoringState state, Guid winnerId)
        => AwardMany(state, winnerId, 4);

    private ScoringState WinSet(ScoringState state, Guid winnerId)
    {
        for (var game = 0; game < state.Format.GamesPerSet; game++)
            state = WinGame(state, winnerId);

        return state;
    }

    private static DateTime NextPointAt(ScoringState state)
    {
        var pointsPlayed = state.Sets.Sum(set =>
            set.Games.Sum(game => game.Points.Length));
        return FirstPointAt.AddSeconds(pointsPlayed + 1);
    }
}
