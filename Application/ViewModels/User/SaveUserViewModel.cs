using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.User
{
    public class SaveUserViewModel : IValidatableObject
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de usuario")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El tipo de usuario es requerido")]  
        [Display(Name = "Tipo de usuario")]
        public string Role { get; set; } = string.Empty;

        [Range(0, 9_999_999.99, ErrorMessage = "El monto inicial no puede ser negativo")]
        [Display(Name = "Monto inicial (RD$)")]
        [DataType(DataType.Currency)]
        public decimal? InitialAmount { get; set; }

        [Range(0, 9_999_999.99, ErrorMessage = "El monto adicional no puede ser negativo")]
        [Display(Name = "Monto adicional (RD$)")]
        [DataType(DataType.Currency)]
        public decimal? AdditionalAmount { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool isCreate = string.IsNullOrEmpty(Id);

            if (isCreate && string.IsNullOrWhiteSpace(Password))
                yield return new ValidationResult(
                    "La contraseña es requerida al crear un usuario.",
                    new[] { nameof(Password) });

            if (!string.IsNullOrWhiteSpace(Password) && Password != ConfirmPassword)
                yield return new ValidationResult(
                    "Las contraseñas no coinciden.",
                    new[] { nameof(ConfirmPassword) });

            if (isCreate && string.IsNullOrWhiteSpace(Role))
                yield return new ValidationResult(
                    "Debe seleccionar un tipo de usuario.",
                    new[] { nameof(Role) });
        }
    }
}