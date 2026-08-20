using System.Collections.Immutable;

namespace TennisScores.Domain.Scoring;

public sealed class ScoringReplay(ScoringEngine scoringEngine)
{
    private readonly ScoringEngine _scoringEngine = scoringEngine;

    public ScoringState Replay(
        ScoringState observedState,
        Guid initialServingPlayerId,
        IEnumerable<AwardPoint> points)
    {
        ArgumentNullException.ThrowIfNull(observedState);
        ArgumentNullException.ThrowIfNull(points);

        if (initialServingPlayerId != observedState.Player1Id &&
            initialServingPlayerId != observedState.Player2Id)
        {
            throw new ArgumentException(
                "Initial server must be one of the match participants.",
                nameof(initialServingPlayerId));
        }

        var state = new ScoringState(
            observedState.Player1Id,
            observedState.Player2Id,
            initialServingPlayerId,
            IsCompleted: false,
            WinnerId: null,
            EndTime: null,
            observedState.Format,
            ImmutableArray<ScoringSet>.Empty);

        foreach (var point in points)
            state = _scoringEngine.Apply(state, point).State;

        return state;
    }

    public Guid InferInitialServingPlayerId(
        ScoringState observedState,
        IEnumerable<AwardPoint> points)
    {
        ArgumentNullException.ThrowIfNull(observedState);
        ArgumentNullException.ThrowIfNull(points);

        var pointList = points.ToList();
        foreach (var candidate in new[]
                 {
                     observedState.Player1Id,
                     observedState.Player2Id
                 })
        {
            if (Replay(observedState, candidate, pointList).ServingPlayerId ==
                observedState.ServingPlayerId)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            "The initial server cannot be inferred from the current match projection.");
    }
}
