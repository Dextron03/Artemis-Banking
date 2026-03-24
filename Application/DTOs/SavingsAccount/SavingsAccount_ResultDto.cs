using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.SavingsAccount
{
    public class SavingsAccount_ResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccountId { get; set; }
    }
}
