using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.SavingsAccount
{
    public class TransactionDto
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Origin { get; set; }
        public string Beneficiary { get; set; }
        public string Status { get; set; }
        public string Concept { get; set; }
    }
}
