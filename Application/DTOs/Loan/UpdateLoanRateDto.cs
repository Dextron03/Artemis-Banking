using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Loan
{
    public class UpdateLoanRateDto
    {
        public string LoanId { get; set; } = string.Empty;
        public decimal NewRate { get; set; }
    }
}
