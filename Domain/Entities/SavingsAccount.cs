using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SavingsAccount
    {
        public string Id { get; set; } = string.Empty;
        public decimal Balance { get; set; } = decimal.MinValue;
        public int UserId { get; set; } = 0;
        public IList<Transaction> Transactions { get; set; } = new List<Transaction>();
        public DateTime AtCreate { get; set; } = DateTime.Now;

    }
}