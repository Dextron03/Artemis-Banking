using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Loan
{
    public class LoanDetailDto
    {
        public string LoanId { get; set; } = string.Empty;
        public string IdentifierNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal OutstandingAmount { get; set; }
        public List<ShareDto> Shares { get; set; } = new();

    }
}
