using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.ViewModels.Cashier.Payments
{
    public class ConfirmPaymentViewModel
    {
        public string LoanId { get; set; }
        public string DisplayIdentifier { get; set; }
        public decimal Amount { get; set; }
        public string ClientName { get; set; }
        public string ClientLastName { get; set; }
    }
}