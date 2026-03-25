using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Application.ViewModels.Cashier.Transfers
{
    public class TransferViewModel
    {
        [Required(ErrorMessage = "La cuenta origen es obligatoria")]
        public string OriginAccount { get; set; }

        [Required(ErrorMessage = "La cuenta destino es obligatoria")]
        public string DestinationAccount { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }
    
    }
}