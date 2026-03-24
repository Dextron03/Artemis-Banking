using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Login
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Debes colocar un nombre de usuario")]
        [DataType(DataType.Text)]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
