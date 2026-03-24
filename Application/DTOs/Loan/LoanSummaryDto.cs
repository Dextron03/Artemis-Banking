using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Loan
{
    public class LoanSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string IdentifierNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int Months { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
