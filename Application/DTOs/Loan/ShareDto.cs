using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Loan
{
    public class ShareDto
    {
        public string Id { get; set; } = string.Empty;
        public int QuotaNumber { get; set; }
        public decimal ShareAmount { get; set; }
        public DateTime DatePay { get; set; }
        public bool IsPaid { get; set; }
        public bool IsDelayed { get; set; }
    }
}
