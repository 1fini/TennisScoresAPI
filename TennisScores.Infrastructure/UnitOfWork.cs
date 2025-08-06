// TennisScores.Infrastructure/UnitOfWork.cs
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TennisScores.Domain;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Infrastructure
{
    public class UnitOfWork(TennisDbContext context) : IUnitOfWork
    {
        private readonly TennisDbContext _context = context;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
