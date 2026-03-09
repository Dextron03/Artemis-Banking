using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Beneficiary
    {
        /// <summary>Identificador único del beneficiario (GUID).</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Nombre del beneficiario (dueño de la cuenta destino).</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Apellido del beneficiario.</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Número de cuenta de 9 dígitos del beneficiario al que se le transferirá.</summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>ID del usuario que registró este beneficiario en su lista (referencia a AppUser).</summary>
        public string UserId { get; set; } = string.Empty;
    }
}