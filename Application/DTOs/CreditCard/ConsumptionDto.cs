using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CreditCard
{
    public class ConsumptionDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime ConsumptionDate { get; set; }
        public decimal Amount { get; set; }
        public string CommerceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
