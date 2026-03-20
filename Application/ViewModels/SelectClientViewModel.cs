using Infrastructure.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class SelectClientViewModel
    {
        public List<ClientLoanViewModel> Clients { get; set; } = new();
        public decimal AverageDebt { get; set; }
    }
}
