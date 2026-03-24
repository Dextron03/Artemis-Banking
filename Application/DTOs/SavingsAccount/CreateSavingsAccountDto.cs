using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.SavingsAccount
{
    public class CreateSavingsAccountDto
    {
        public string UserId { get; set; }
        public decimal InitialBalance { get; set; }
        public string AdminUserId { get; set; }
    }
}
