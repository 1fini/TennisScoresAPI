using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TennisScores.Domain.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Remove(T entity);
        Task SaveChangesAsync();
        void Update(T entity);
        IQueryable<T> Query(); // Not sure it's needed here, but keeping for consistency
    }
}
