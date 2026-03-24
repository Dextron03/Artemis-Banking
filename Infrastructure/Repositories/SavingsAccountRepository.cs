using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SavingsAccountRepository : GenericRepository<SavingsAccount>, ISavingsAccountRepository
    {
        private readonly ApplicationDbContext _db;

        public SavingsAccountRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<SavingsAccount?> GetPrincipalByUserIdAsync(string userId)
        {
            return await _db.SavingsAccounts
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsPrincipal);
        }

        public async Task<bool> AccountNumberExistsAsync(string accountNumber)
        {
            bool inAccounts = await _db.SavingsAccounts
                .AnyAsync(a => a.AccountNumber == accountNumber);

            bool inLoans = await _db.Loans
                .AnyAsync(l => l.IdentifierNumber == accountNumber);

            return inAccounts || inLoans;
        }

        public async Task<SavingsAccount> GetByAccountNumberAsync(string accountNumber)=> await _dbSet.FirstOrDefaultAsync(s => s.AccountNumber == accountNumber);

        public async Task<IEnumerable<SavingsAccount>> GetByUserIdAsync(string userId)
            => await _dbSet.Where(s => s.UserId == userId) .OrderByDescending(s => s.CreatedAt) .ToListAsync();

        public async Task<SavingsAccount> GetPrincipalAccountByUserIdAsync(string userId)
           => await _dbSet.FirstOrDefaultAsync(s => s.UserId == userId && s.IsPrincipal && s.IsActive);

        public async Task<IEnumerable<SavingsAccount>> GetAllWithUserAsync()
            => await _dbSet.OrderByDescending(s => s.CreatedAt).ToListAsync();

        public async Task<SavingsAccount> GetByIdWithTransactionsAsync(string id)
            => await _dbSet
                .Include(s => s.Transactions)
                .FirstOrDefaultAsync(s => s.Id == id);
    }
}
