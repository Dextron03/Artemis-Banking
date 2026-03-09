using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Entities
{
    public class AppUser : IdentityUser
    {
        /// <summary>Nombre(s) del usuario.</summary>
        public string FirtsName { get; set; } = string.Empty;

        /// <summary>Apellido(s) del usuario.</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Número de cédula de identidad del usuario (único en el sistema).</summary>
        public string IdentityNumber { get; set; } = string.Empty;

        /// <summary>Indica si el usuario está activo. Los usuarios nuevos deben confirmar su cuenta para activarse.</summary>
        public bool IsActive { get; set; } = false;
    }
}