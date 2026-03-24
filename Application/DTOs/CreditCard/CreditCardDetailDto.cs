using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CreditCard
{
    public class CreditCardDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string IdentifierNumber { get; set; } = string.Empty;
        public string ClientFullName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public string ExpireDate { get; set; } = string.Empty;
        public decimal AmountDebt { get; set; }
        public bool IsActive { get; set; }
        public IList<ConsumptionDto> Consumptions { get; set; } = new List<ConsumptionDto>();
        public string LastFourDigits => IdentifierNumber.Length >= 4
            ? IdentifierNumber[^4..] : IdentifierNumber;
    }
}
