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
    public class ShareRepository : GenericRepository<Share>, IShareRepository
    {
        public ShareRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Share>> GetPendingOverdueSharesAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _dbSet
                .Where(s => !s.IsPaid && !s.IsDelayed && s.DatePay.Date < today)
                .ToListAsync();
        }
    }
}
