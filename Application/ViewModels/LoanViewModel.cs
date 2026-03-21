using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class LoanViewModel
    {
        public string Id { get; set; }
        public string IdentifierNumber { get; set; }

        public string ClientName { get; set; }
        public string IdentityNumber { get; set; }

        public decimal LoanAmount { get; set; }

        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }

        public decimal PendingAmount { get; set; }

        public decimal InterestRate { get; set; }
        public int Months { get; set; }

        public string PaymentStatus { get; set; }
        public bool IsActive { get; set; }
    }
}
