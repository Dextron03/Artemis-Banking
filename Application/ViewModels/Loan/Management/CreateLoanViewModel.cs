using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Loan.Management
{
    public class CreateLoanViewModel
    {
        [Required(ErrorMessage = "El cliente es requerido.")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
        [Display(Name = "Monto a prestar (RD$)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El plazo es requerido.")]
        [Display(Name = "Plazo (meses)")]
        public int Months { get; set; }

        [Required(ErrorMessage = "La tasa es requerida.")]
        [Range (typeof(decimal), "0.01", "100", ErrorMessage = "La tasa debe estar entre 0.01% y 100%.")]
        [Display(Name = "Tasa de interés anual (%)")]
        public decimal InterestRate { get; set; }
        public List<SelectListItem> MonthOptions { get; set; } = new()
        {
            new SelectListItem("6 meses",  "6"),
            new SelectListItem("12 meses", "12"),
            new SelectListItem("18 meses", "18"),
            new SelectListItem("24 meses", "24"),
            new SelectListItem("30 meses", "30"),
            new SelectListItem("36 meses", "36"),
            new SelectListItem("42 meses", "42"),
            new SelectListItem("48 meses", "48"),
            new SelectListItem("54 meses", "54"),
            new SelectListItem("60 meses", "60"),
        };
    }
}
