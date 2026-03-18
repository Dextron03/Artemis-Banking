using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class SaveUserViewModel
    {
        public string? Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Las contraseñas deben coincidir")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Las contraseñas deben coincidir")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Las contraseñas deben coincidir")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Cedula { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public decimal? InitialAmount { get; set; }
        public decimal? AdditionalAmount { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();
    }
}
