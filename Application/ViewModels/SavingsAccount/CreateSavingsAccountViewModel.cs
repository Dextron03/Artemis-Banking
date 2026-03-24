using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SavingsAccount
{
    public class CreateSavingsAccountViewModel
    {
        public string ClientId { get; set; }
        public string ClientFullName { get; set; }
        public string ClientIdentityNumber { get; set; }

        [Required(ErrorMessage = "El balance inicial es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El balance inicial no puede ser negativo.")]
        [Display(Name = "Balance inicial")]
        [DataType(DataType.Currency)]
        public decimal InitialBalance { get; set; } = 0;
    }
}
