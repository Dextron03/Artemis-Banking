using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SavingsAccount
{
    public class CancelSavingsAccountViewModel
    {
        public string Id { get; set; }

        [Required]
        public string AccountNumber { get; set; }

        public string ClientFullName { get; set; }
        public decimal Balance { get; set; }
        public bool? IsPrincipal { get; set; }
    }
}
