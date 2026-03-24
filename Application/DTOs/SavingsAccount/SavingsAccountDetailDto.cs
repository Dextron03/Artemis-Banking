using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.SavingsAccount
{
    public class SavingsAccountDetailDto
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string ClientFullName { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string IdentityNumber { get; set; }
        public decimal Balance { get; set; }
        public bool IsPrincipal { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TransactionDto> Transactions { get; set; } = new();
    }
}
