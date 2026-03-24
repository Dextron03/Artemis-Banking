using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels
{
    public class AssignCreditCardViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        public string ClientFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El límite de crédito es obligatorio.")]
        [Range(1, 9_999_999.99, ErrorMessage = "El límite debe ser mayor a 0.")]
        [Display(Name = "Límite de crédito")]
        public decimal CreditLimit { get; set; }
    }
}
