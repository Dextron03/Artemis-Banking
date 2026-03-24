using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CreditCardRepository : GenericRepository<CreditCard>, ICreditCardRepository
    {
        public CreditCardRepository(ApplicationDbContext context) : base(context) { }

        public async Task<(IEnumerable<CreditCard> Items, int TotalCount)>
            GetActivePagedAsync(int page, int pageSize)
        {
            var query = _dbSet
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.ExpireDate); 

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<IEnumerable<CreditCard>> GetByIdentityNumberAsync(string identityNumber)
        {
            var userIds = await _context.Set<AppUser>()
                .Where(u => u.IdentityNumber == identityNumber)
                .Select(u => u.Id)
                .ToListAsync();

            if (!userIds.Any())
                return Enumerable.Empty<CreditCard>();

            var cards = await _dbSet
                .Where(c => userIds.Contains(c.UserId))
                .ToListAsync();
            return cards
                .OrderByDescending(c => c.IsActive)
                .ThenByDescending(c => c.ExpireDate)
                .ToList();
        }

        public async Task<(IEnumerable<CreditCard> Items, int TotalCount)>
            GetByStatusPagedAsync(bool isActive, int page, int pageSize)
        {
            var query = _dbSet
                .Where(c => c.IsActive == isActive)
                .OrderByDescending(c => c.ExpireDate);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<bool> IdentifierNumberExistsAsync(string number)
            => await _dbSet.AnyAsync(c => c.IdentifierNumber == number);

        public async Task<CreditCard?> GetWithConsumptionsAsync(string id)
            => await _dbSet
                .Include(c => c.Consumptions)
                .FirstOrDefaultAsync(c => c.Id == id);
    }
}
