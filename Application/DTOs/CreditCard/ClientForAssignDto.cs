using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CreditCard
{
    public class ClientForAssignDto
    {
        public string UserId { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalDebt { get; set; }
    }
}
