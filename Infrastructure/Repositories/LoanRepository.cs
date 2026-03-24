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
    public class LoanRepository : GenericRepository<Loan>, ILoanRepository
    {
        private readonly ApplicationDbContext _db;

        public LoanRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Loan?> GetByIdWithSharesAsync(string id)
        {
            return await _db.Loans
                .Include(l => l.Shares)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Loan>> GetAllWithSharesAsync()
        {
            return await _db.Loans
                .Include(l => l.Shares)
                .ToListAsync();
        }

        public async Task<bool> IdentifierExistsAsync(string identifierNumber)
        {
            bool inLoans = await _db.Loans
                .AnyAsync(l => l.IdentifierNumber == identifierNumber);

            bool inAccounts = await _db.SavingsAccounts
                .AnyAsync(a => a.AccountNumber == identifierNumber);

            return inLoans || inAccounts;
        }
    }
}
