using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Loan.Queries
{
    public class LoanDetailsViewModel
    {
        public string LoanId { get; set; } = string.Empty;
        public string IdentifierNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal LoanAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal InterestRate { get; set; }

        public int TermMonths { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal MonthlyPayment { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal OutstandingAmount { get; set; }

        public List<ShareViewModel> Shares { get; set; } = new();
    }
}
