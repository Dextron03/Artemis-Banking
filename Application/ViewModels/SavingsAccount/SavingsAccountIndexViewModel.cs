using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SavingsAccount
{
    public class SavingsAccountIndexViewModel
    {
        public List<SavingsAccountRowViewModel> Accounts { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 20;
        public string SearchIdentityNumber { get; set; }
        public string FilterStatus { get; set; }   
        public string FilterType { get; set; }     
    }
}
