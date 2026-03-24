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
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Transaction>> GetBySavingsAccountIdAsync(string savingsAccountId)
            => await _dbSet
                .Where(t => t.SavingAccountId == savingsAccountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
    }
}
