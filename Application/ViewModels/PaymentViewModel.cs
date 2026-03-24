using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class PaymentViewModel
    {
        [Required(ErrorMessage = "El numero del prestamo es obligatorio")]
        public string LoanId { get; set; }

        [Required(ErrorMessage = "El monto a pagar es obligatiro")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }
    
    }
}