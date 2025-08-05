using TennisScores.Domain.Entities;

namespace TennisScores.Domain.Repositories;

public interface ISetRepository : IRepository<TennisSet>
{
        void AddSet(TennisSet tennisSet);
}
