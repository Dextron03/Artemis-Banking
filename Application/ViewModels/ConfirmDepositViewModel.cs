using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class ConfirmDepositViewModel
    {
        public string AccountNumber { get; set; }

        public decimal Amount { get; set; }
        public string ClientName { get; set; }
        public string ClientLastName { get; set; }
    }
}