using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.CreditCard.Management
{
    public class CreateCreditCardViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [Range(1000, 10000000)]
        public decimal CreditLimit { get; set; }
    }
}