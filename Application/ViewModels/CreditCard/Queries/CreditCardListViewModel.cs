using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.CreditCard.Queries
{
    public class CreditCardListViewModel
    {
        public string Id { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public decimal Limit { get; set; }
        public decimal Debt { get; set; }
        public string ExpireDate { get; set; }
        public bool IsActive { get; set; }
    }
}