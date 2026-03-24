using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CreditCard
{
    public class UpdateCreditCardDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal NewCreditLimit { get; set; }
    }
}
