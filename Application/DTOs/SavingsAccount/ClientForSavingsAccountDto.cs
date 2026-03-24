using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.SavingsAccount
{
    public class ClientForSavingsAccountDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string Email { get; set; }
        public decimal TotalDebt { get; set; }
    }
}
