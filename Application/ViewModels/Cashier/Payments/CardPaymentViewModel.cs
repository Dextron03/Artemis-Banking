using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Application.ViewModels.Cashier.Payments
{
    public class CardPaymentViewModel
    {
        [Required(ErrorMessage = "El número de cuenta de origen es obligatorio")]
        public string OriginAccountNumber { get; set; }

        [Required(ErrorMessage = "El número de la tarjeta de crédito es obligatorio")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "El número de tarjeta debe tener 16 dígitos")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "El monto a pagar es obligatorio")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }
    }
}
