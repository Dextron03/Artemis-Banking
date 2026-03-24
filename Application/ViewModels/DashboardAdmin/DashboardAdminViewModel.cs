using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.DashboardAdmin
{
    public class DashboardAdminViewModel
    {
        public int TotalTransactions { get; set; }
        public int TransactionsToday { get; set; }
        public int TotalPayments { get; set; }
        public int PaymentsToday { get; set; }
        public int ActiveClients { get; set; }
        public int InactiveClients { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveLoans { get; set; }
        public int ActiveCreditCards { get; set; }
        public int TotalSavingsAccounts { get; set; }
        public decimal AverageDebtPerClient { get; set; }
    }
}
