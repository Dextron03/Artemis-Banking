using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CreditCard
{
    public class ClientListResultDto
    {
        public IEnumerable<ClientForAssignDto> Clients { get; set; } = new List<ClientForAssignDto>();
        public decimal AverageDebt { get; set; }
    }
}
