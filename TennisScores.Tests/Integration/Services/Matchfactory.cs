using TennisScores.Domain.Entities;

namespace TennisScores.Tests.Integration.Services;

public static class MatchFactory
{
    public static Match CreateBestOf3MatchWith2SetsEachWon()
    {
        var p1 = new Player { Id = Guid.NewGuid(), FirstName = "A", LastName = "Player1" };
        var p2 = new Player { Id = Guid.NewGuid(), FirstName = "B", LastName = "Player2" };

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Player1Id = p1.Id,
            Player2Id = p2.Id,
            Player1 = p1,
            Player2 = p2,
            BestOfSets = 3
        };

        var set1 = new TennisSet
        {
            Id = Guid.NewGuid(),
            Match = match,
            MatchId = match.Id,
            SetNumber = 1,
            Player1Games = 6,
            Player2Games = 3,
            WinnerId = p1.Id,
            Winner = p1
        };

        var set2 = new TennisSet
        {
            Id = Guid.NewGuid(),
            Match = match,
            MatchId = match.Id,
            SetNumber = 2,
            Player1Games = 3,
            Player2Games = 6,
            WinnerId = p2.Id,
            Winner = p2
        };

        var set3 = new TennisSet
        {
            Id = Guid.NewGuid(),
            Match = match,
            MatchId = match.Id,
            SetNumber = 3
        };

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Set = set3,
            SetId = set3.Id,
            GameNumber = 1,
            IsTiebreak = true
        };

        set3.Games.Add(game);

        match.Sets.Add(set1);
        match.Sets.Add(set2);
        match.Sets.Add(set3);

        return match;
    }

    public static Match CreateMatchWithWinnerIn2StraightSets()
    {
        var p1 = new Player { Id = Guid.NewGuid(), FirstName = "A", LastName = "Player1" };
        var p2 = new Player { Id = Guid.NewGuid(), FirstName = "B", LastName = "Player2" };

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Player1Id = p1.Id,
            Player2Id = p2.Id,
            Player1 = p1,
            Player2 = p2,
            BestOfSets = 3
        };

        var set1 = new TennisSet
        {
            Id = Guid.NewGuid(),
            Match = match,
            MatchId = match.Id,
            SetNumber = 1,
            Player1Games = 6,
            Player2Games = 2,
            WinnerId = p1.Id,
            Winner = p1
        };

        var set2 = new TennisSet
        {
            Id = Guid.NewGuid(),
            Match = match,
            MatchId = match.Id,
            SetNumber = 2,
            Player1Games = 6,
            Player2Games = 4,
            WinnerId = p1.Id,
            Winner = p1
        };

        match.Sets.Add(set1);
        match.Sets.Add(set2);

        return match;
    }

    public static Match CreateNewMatchNotYetStarted()
    {
        var p1 = new Player { Id = Guid.NewGuid(), FirstName = "Carlos", LastName = "Alcaraz" };
        var p2 = new Player { Id = Guid.NewGuid(), FirstName = "Jannik", LastName = "Sinner" };

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Player1Id = p1.Id,
            Player2Id = p2.Id,
            Player1 = p1,
            Player2 = p2,
            BestOfSets = 3
        };

        return match;
    }
}