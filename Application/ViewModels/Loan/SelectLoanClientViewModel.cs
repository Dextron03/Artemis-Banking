using Infrastructure.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Loan
{
    public class SelectLoanClientViewModel
    {
        public List<ClientLoanViewModel> Clients { get; set; } = new();

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal AverageDebt { get; set; }

        public string? SearchIdentity { get; set; }
    }
}
