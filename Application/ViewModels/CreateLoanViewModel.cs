using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class CreateLoanViewModel
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public int Months { get; set; }
        public decimal InterestRate { get; set; }

        public List<SelectListItem> MonthOptions { get; set; } = new();
    }
}
