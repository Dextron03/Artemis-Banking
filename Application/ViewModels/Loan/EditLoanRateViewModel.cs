using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Loan
{
    public class EditLoanRateViewModel
    {
        public string LoanId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La tasa es requerida.")]
        [Range(typeof(decimal), "0.001", "100",
            ErrorMessage = "La tasa debe estar entre 0.001% y 100%.")]
        [Display(Name = "Nueva tasa de interés anual (%)")]
        public decimal InterestRate { get; set; }
    }
}