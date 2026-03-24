using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels
{
    public class EditCreditCardViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        public string LastFourDigits { get; set; } = string.Empty;

        public decimal CurrentDebt { get; set; }

        [Required(ErrorMessage = "El límite de la tarjeta es obligatorio.")]
        [Range(0.01, 9_999_999.99, ErrorMessage = "El límite debe ser mayor a 0.")]
        [Display(Name = "Límite de la tarjeta")]
        public decimal CreditLimit { get; set; }
    }
}
