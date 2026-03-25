using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Loan.Queries
{
    public class LoanIndexViewModel
    {
        public List<LoanViewModel> Loans { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string? Identity { get; set; }
        public bool? Status { get; set; }
    }
}
