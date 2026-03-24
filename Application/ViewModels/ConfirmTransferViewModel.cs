using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class ConfirmTransferViewModel
    {
        public string OriginAccount { get; set; }
        public string DestinationAccount { get; set; }
        public decimal Amount { get; set; }
        public string OriginClientName { get; set; }
        public string DestinationClientName { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}