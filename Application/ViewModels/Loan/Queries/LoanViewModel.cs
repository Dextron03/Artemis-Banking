using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Loan.Queries
{
    public class LoanViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string IdentifierNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LoanAmount { get; set; }

        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal PendingAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal InterestRate { get; set; }

        public int Months { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}