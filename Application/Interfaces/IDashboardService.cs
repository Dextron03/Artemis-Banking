using Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        public async Task<DashboardAdminDto> GetDashboardAsync()
        {
            return new DashboardAdminDto
            {
                TotalTransactions = 0,
                TransactionsToday = 0,
                TotalPayments = 0,
                PaymentsToday = 0,
                ActiveClients = 0,
                InactiveClients = 0,
                ActiveLoans = 0,
                ActiveCreditCards = 0,
                TotalSavingsAccounts = 0,
                TotalProducts = 0,
                AverageDebtPerClient = 0
            };
        }
    }
}
