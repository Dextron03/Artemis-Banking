using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.CreditCard
{
    public class EditCreditCardViewModel
    {
        public string Id { get; set; }

        [Required]
        public decimal CreditLimit { get; set; }

        public decimal CurrentDebt { get; set; }

    }
}
