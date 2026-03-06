using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class Transaction
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string UserId { get; set; }
        public TransactionType Type { get; set; } = TransactionType.Deposit;
        public string Description { get; set; } = string.Empty;
        public string SavingAccountId { get; set; } = string.Empty;
        public SavingsAccount SavingsAccount { get; set; }
        public DateTime AtCreate { get; set; } = DateTime.Now;


    }
}