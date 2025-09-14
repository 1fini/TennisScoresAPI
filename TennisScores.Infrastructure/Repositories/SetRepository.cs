using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Infrastructure.Repositories;

public class SetRepository : Repository<TennisSet>, ISetRepository
{
    public SetRepository(TennisDbContext context) : base(context)
    {
    }

    public void AddSet(TennisSet tennisSet)
    {
        _context.Set<TennisSet>().Add(tennisSet);
    }
}