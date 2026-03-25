using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Application.ViewModels.Cashier.Withdrawals
{
    public class WithdrawalViewModel
    {
        [Required(ErrorMessage = "El numero de cuenta es obligatorio")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set;}
    }
}