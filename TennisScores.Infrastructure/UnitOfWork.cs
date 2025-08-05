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
        {
            // Ensure Match is tracked
            //var match = _context.ChangeTracker.Entries().SingleOrDefault(e => e.Metadata.Name == "TennisScores.Domain.Entities.Match");

            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
