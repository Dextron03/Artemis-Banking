using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Loan
{
    public class CreateLoanDto
    {
        public string UserId { get; set; } = string.Empty;
        public string AdminId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Months { get; set; }
        public decimal InterestRate { get; set; }
    }
}
