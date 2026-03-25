using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Dashboard
{
    public class DashboardCashierDto
    {
        public int TotalTransactionsToday { get; set; }
        public int TotalPaymentsToday { get; set; }
        public int TotalDepositsToday { get; set; }
        public int TotalWithdrawalsToday { get; set; }
    }
}
