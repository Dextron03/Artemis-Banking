using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Loan
{
    public class ShareViewModel
    {
        public string Id { get; set; } = string.Empty;
        public int QuotaNumber { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal ShareAmount { get; set; }

        public DateTime DatePay { get; set; }
        public bool IsPaid { get; set; }
        public bool IsDelayed { get; set; }
    }
}
