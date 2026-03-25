using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.CreditCard.Queries
{
    public class CreditCardFilterViewModel
    {
        public string Cedula { get; set; }
        public bool? IsActive { get; set; }

        public int Page { get; set; } = 1;
    }
}
