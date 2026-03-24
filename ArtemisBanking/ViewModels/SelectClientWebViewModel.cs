using Application.DTOs.CreditCard;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels
{
    public class SelectClientWebViewModel
    {
        public IEnumerable<ClientForAssignDto> Clients { get; set; }
            = new List<ClientForAssignDto>();

        public decimal AverageDebt { get; set; }

        [Display(Name = "Buscar por cédula")]
        public string? SearchIdentity { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        public string? SelectedUserId { get; set; }
    }
}
