using System.Collections.Immutable;
using TennisScores.Domain.Events;

namespace TennisScores.Domain.Scoring;

public sealed class ScoringEngine
{
    public ScoringResult Apply(ScoringState state, AwardPoint command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);

        Validate(state, command);

        var sets = state.Sets;
        var setIndex = FindCurrentSetIndex(sets);
        if (setIndex < 0)
        {
            if (sets.Length >= state.Format.BestOfSets)
                throw new InvalidOperationException("Sets count is greater than the match format allows.");

            sets = sets.Add(NewSet(sets.Length + 1));
            setIndex = sets.Length - 1;
        }

        var currentSet = sets[setIndex];
        var gameIndex = FindCurrentGameIndex(currentSet.Games);
        if (gameIndex < 0)
        {
            currentSet = currentSet with
            {
                Games = currentSet.Games.Add(NewGame(state, currentSet))
            };
            gameIndex = currentSet.Games.Length - 1;
        }

        var currentGame = currentSet.Games[gameIndex];
        var shouldBeTiebreak = ShouldGameBeTiebreak(state, currentSet, currentGame);
        currentGame = currentGame with
        {
            IsTiebreak = shouldBeTiebreak,
            Points = currentGame.Points.Add(new ScoredPoint(
                command.WinnerId,
                command.PointType,
                command.OccurredAt))
        };

        var serverBeforePoint = state.ServingPlayerId;
        var servingPlayerId = serverBeforePoint;
        if (currentGame.IsTiebreak && currentGame.Points.Length % 2 == 1)
            servingPlayerId = OtherPlayer(state, servingPlayerId);

        var gameCompleted = IsGameOver(state, currentSet, currentGame);
        if (gameCompleted)
        {
            currentGame = currentGame with
            {
                IsCompleted = true,
                WinnerId = command.WinnerId
            };

            if (!currentGame.IsTiebreak)
                servingPlayerId = OtherPlayer(state, servingPlayerId);
        }

        currentSet = currentSet with
        {
            Games = currentSet.Games.SetItem(gameIndex, currentGame)
        };
        sets = sets.SetItem(setIndex, currentSet);

        var matchCompleted = state.IsCompleted;
        var matchWinnerId = state.WinnerId;
        var endTime = state.EndTime;

        var setCompleted = false;
        if (gameCompleted)
        {
            setCompleted = IsSetOver(state, currentSet);
            if (setCompleted)
            {
                currentSet = currentSet with
                {
                    IsCompleted = true,
                    WinnerId = command.WinnerId
                };
                sets = sets.SetItem(setIndex, currentSet);

                var completedState = state with { Sets = sets };
                matchWinnerId = GetMatchWinner(completedState);
                matchCompleted = matchWinnerId.HasValue;

                if (matchCompleted)
                {
                    endTime ??= command.OccurredAt;
                }
                else
                {
                    sets = sets.Add(NewSet(sets.Length + 1));
                }
            }
            else
            {
                currentSet = currentSet with
                {
                    Games = currentSet.Games.Add(NewGame(state with { Sets = sets }, currentSet))
                };
                sets = sets.SetItem(setIndex, currentSet);
            }
        }

        var newState = state with
        {
            ServingPlayerId = servingPlayerId,
            IsCompleted = matchCompleted,
            WinnerId = matchWinnerId,
            EndTime = endTime,
            Sets = sets
        };

        var events = ImmutableArray.CreateBuilder<IMatchDomainEvent>();
        events.Add(new PointWon(
            command.WinnerId,
            serverBeforePoint,
            command.PointType,
            currentSet.SetNumber,
            currentGame.GameNumber,
            command.OccurredAt));

        if (gameCompleted)
        {
            events.Add(new GameWon(
                command.WinnerId,
                currentSet.SetNumber,
                currentGame.GameNumber,
                command.OccurredAt));
        }

        if (setCompleted)
        {
            events.Add(new SetWon(
                command.WinnerId,
                currentSet.SetNumber,
                command.OccurredAt));
        }

        if (matchCompleted && !state.IsCompleted)
            events.Add(new MatchWon(command.WinnerId, command.OccurredAt));

        if (servingPlayerId != serverBeforePoint)
        {
            events.Add(new ServerChanged(
                serverBeforePoint,
                servingPlayerId,
                command.OccurredAt));
        }

        return new ScoringResult(
            newState,
            currentSet.SetNumber,
            currentGame.GameNumber,
            events.ToImmutable());
    }

    private static void Validate(ScoringState state, AwardPoint command)
    {
        if (state.IsCompleted)
            throw new InvalidOperationException("Cannot add a point to a completed match.");

        if (command.WinnerId != state.Player1Id && command.WinnerId != state.Player2Id)
        {
            throw new ArgumentException(
                "Point winner must be one of the match participants.",
                "winnerId");
        }
    }

    private static int FindCurrentSetIndex(ImmutableArray<ScoringSet> sets)
    {
        for (var index = sets.Length - 1; index >= 0; index--)
        {
            if (!sets[index].IsCompleted)
                return index;
        }

        return -1;
    }

    private static int FindCurrentGameIndex(ImmutableArray<ScoringGame> games)
    {
        for (var index = games.Length - 1; index >= 0; index--)
        {
            if (!games[index].IsCompleted)
                return index;
        }

        return -1;
    }

    private static ScoringSet NewSet(int setNumber)
        => new(
            setNumber,
            IsCompleted: false,
            WinnerId: null,
            Games: []);

    private static ScoringGame NewGame(ScoringState state, ScoringSet set)
        => new(
            set.Games.Length + 1,
            ShouldGameBeTiebreak(state, set),
            IsCompleted: false,
            WinnerId: null,
            Points: []);

    private static bool IsGameOver(
        ScoringState state,
        ScoringSet set,
        ScoringGame game)
    {
        var player1Points = game.Points.Count(point => point.WinnerId == state.Player1Id);
        var player2Points = game.Points.Count(point => point.WinnerId == state.Player2Id);

        if (game.IsTiebreak)
        {
            var pointsToWin = IsFinalSetSuperTieBreak(state, set)
                ? state.Format.SuperTieBreakPoints
                : state.Format.TieBreakPoints;

            return Math.Max(player1Points, player2Points) >= pointsToWin &&
                Math.Abs(player1Points - player2Points) >= 2;
        }

        if (state.Format.DecidingPointEnabled)
            return player1Points == 4 || player2Points == 4;

        return (player1Points >= 4 || player2Points >= 4) &&
            Math.Abs(player1Points - player2Points) >= 2;
    }

    private static bool IsSetOver(ScoringState state, ScoringSet set)
    {
        var completedGames = set.Games.Where(game => game.IsCompleted).ToList();
        var player1Games = completedGames.Count(game => game.WinnerId == state.Player1Id);
        var player2Games = completedGames.Count(game => game.WinnerId == state.Player2Id);
        var gameDifference = Math.Abs(player1Games - player2Games);

        if ((player1Games >= state.Format.GamesPerSet ||
                player2Games >= state.Format.GamesPerSet) &&
            gameDifference >= 2)
        {
            return true;
        }

        if (state.Format.TieBreakEnabled &&
            player1Games >= state.Format.GamesPerSet &&
            player2Games >= state.Format.GamesPerSet)
        {
            var tieBreak = set.Games.LastOrDefault(game => game.IsTiebreak);
            return tieBreak is not null && tieBreak.IsCompleted;
        }

        if (state.Format.SuperTieBreakForFinalSet && IsFinalSet(state, set))
        {
            var lastGame = set.Games.LastOrDefault();
            return lastGame is not null && lastGame.IsTiebreak && lastGame.IsCompleted;
        }

        return false;
    }

    private static Guid? GetMatchWinner(ScoringState state)
    {
        var player1SetWins = state.Sets.Count(set =>
            set.IsCompleted && set.WinnerId == state.Player1Id);
        if (player1SetWins >= state.Format.SetsToWin)
            return state.Player1Id;

        var player2SetWins = state.Sets.Count(set =>
            set.IsCompleted && set.WinnerId == state.Player2Id);
        return player2SetWins >= state.Format.SetsToWin
            ? state.Player2Id
            : null;
    }

    private static bool ShouldGameBeTiebreak(
        ScoringState state,
        ScoringSet set,
        ScoringGame? game = null)
    {
        if (!state.Format.TieBreakEnabled)
            return false;

        if (IsFinalSetSuperTieBreak(state, set))
            return true;

        var completedGames = set.Games.Where(candidate => candidate.IsCompleted).ToList();
        var player1Games = completedGames.Count(candidate => candidate.WinnerId == state.Player1Id);
        var player2Games = completedGames.Count(candidate => candidate.WinnerId == state.Player2Id);

        return player1Games == state.Format.GamesPerSet &&
            player2Games == state.Format.GamesPerSet &&
            (game is null || game.GameNumber == completedGames.Count + 1);
    }

    private static bool IsFinalSetSuperTieBreak(ScoringState state, ScoringSet set)
        => state.Format.SuperTieBreakForFinalSet && IsFinalSet(state, set);

    private static bool IsFinalSet(ScoringState state, ScoringSet set)
        => set.SetNumber == state.Format.BestOfSets;

    private static Guid OtherPlayer(ScoringState state, Guid playerId)
        => playerId == state.Player1Id
            ? state.Player2Id
            : state.Player1Id;
}
