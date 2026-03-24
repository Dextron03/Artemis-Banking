using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Loan
{
    public class RiskEvaluationDto
    {
        public bool IsHighRisk { get; set; }
        public string RiskType { get; set; } = string.Empty;
        public decimal AverageDebt { get; set; }
        public decimal ClientDebt { get; set; }
    }
}
