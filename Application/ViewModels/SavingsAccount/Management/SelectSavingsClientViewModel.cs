using Application.DTOs.SavingsAccount;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SavingsAccount.Management
{
    public class SelectSavingsClientViewModel
    {
        public List<ClientForSavingsAccountDto> Clients { get; set; } = new();

        [Display(Name = "Buscar por cédula")]
        public string SearchIdentityNumber { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        public string SelectedClientId { get; set; }
    }
}
