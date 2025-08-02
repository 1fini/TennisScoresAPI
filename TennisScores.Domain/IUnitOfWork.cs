using System.Threading;
using System.Threading.Tasks;

namespace TennisScores.Domain
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
