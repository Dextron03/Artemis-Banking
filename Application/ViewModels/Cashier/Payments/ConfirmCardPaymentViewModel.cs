using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.ViewModels.Cashier.Payments
{
    public class ConfirmCardPaymentViewModel
    {
        public string OriginAccountNumber { get; set; }
        public string CardNumber { get; set; }
        public string DisplayCardNumber { get; set; }
        public decimal Amount { get; set; }
        public string ClientName { get; set; }
        public string ClientLastName { get; set; }
        public bool HasInsufficientFunds { get; set; }
    }
}
