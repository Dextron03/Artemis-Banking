using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Loan
{
    public class EligibleClientsResultDto
    {
        public List<EligibleClientDto> Clients { get; set; } = new();
        public decimal AverageDebt { get; set; }
    }
}
