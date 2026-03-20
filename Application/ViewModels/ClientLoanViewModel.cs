using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class ClientLoanViewModel
    {
        public string Id { get; set; }
        public string FirtsName { get; set; }
        public string LastName { get; set; }
        public string IdentityNumber { get; set; }
        public string Email { get; set; }
        public decimal Debt { get; set; } 
    }
}
