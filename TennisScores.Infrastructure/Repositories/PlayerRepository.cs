using TennisScores.Domain.Entities;
using TennisScores.Domain.Repositories;
using TennisScores.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace TennisScores.Infrastructure.Repositories;
public class PlayerRepository(TennisDbContext context) : Repository<Player>(context), IPlayerRepository
{
    public async Task<Player?> GetByFullNameAsync(string firstName, string lastName)
    {
        return await _context.Players
            .FirstOrDefaultAsync(p =>
                p.FirstName.ToLower() == firstName.ToLower() &&
                p.LastName.ToLower() == lastName.ToLower());
    }

    public async Task<Player> GetOrCreateAsync(string firstName, string lastName)
    {
        var existing = await GetByFullNameAsync(firstName, lastName);
        if (existing != null)
            return existing;

        var player = new Player
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName
        };

        await AddAsync(player);
        return player;
    }
}
