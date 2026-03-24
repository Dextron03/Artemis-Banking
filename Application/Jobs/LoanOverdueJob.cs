using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Jobs
{
    public class LoanOverdueJob
    {
        private readonly IShareRepository _shareRepo;

        public LoanOverdueJob(IShareRepository shareRepo)
        {
            _shareRepo = shareRepo;
        }

        public async Task MarkOverdueSharesAsync()
        {
            var overdueShares = (await _shareRepo.GetPendingOverdueSharesAsync()).ToList();

            foreach (var share in overdueShares)
            {
                share.IsDelayed = true;
                _shareRepo.Update(share);
            }

            if (overdueShares.Any())
                await _shareRepo.SaveChangesAsync();
        }

        public static void ClearDelayOnPaidShare(Share share)
        {
            if (share.IsPaid)
                share.IsDelayed = false;
        }
    }
}
