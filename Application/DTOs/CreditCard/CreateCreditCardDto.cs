using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CreditCard
{
    public class CreateCreditCardDto
    {
        public string UserId { get; set; } = string.Empty;
        public string AdminId { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
    }
}
