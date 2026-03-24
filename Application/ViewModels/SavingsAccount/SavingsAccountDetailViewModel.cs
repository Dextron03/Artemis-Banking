using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SavingsAccount
{
    public class SavingsAccountDetailViewModel
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string ClientFullName { get; set; }
        public string UserId { get; set; }
        public decimal Balance { get; set; }
        public bool IsPrincipal { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AccountType => IsPrincipal ? "Principal" : "Secundaria";
        public List<TransactionRowViewModel> Transactions { get; set; } = new();
    }
}
