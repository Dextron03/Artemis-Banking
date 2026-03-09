using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CreditCard
    {
        /// <summary>Identificador único de la tarjeta (GUID).</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Número de tarjeta de 16 dígitos único en el sistema.</summary>
        public string IdentifierNumber { get; set; } = string.Empty;

        /// <summary>Límite de crédito asignado a la tarjeta.</summary>
        public decimal CreditLimit { get; set; }

        /// <summary>Fecha de expiración en formato "MM/AA" (ej: "03/28"). Se calcula como fecha actual + 3 años.</summary>
        public string ExpireDate { get; set; } = string.Empty;

        /// <summary>Deuda acumulada actual de la tarjeta. No puede superar el CreditLimit.</summary>
        public decimal AmountDebt { get; set; } = 0;

        /// <summary>Código de seguridad de 3 dígitos almacenado como hash SHA-256.</summary>
        public string CvcCode { get; set; } = string.Empty;

        /// <summary>Indica si la tarjeta está activa. Solo se puede cancelar si la deuda es 0.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>ID del cliente dueño de la tarjeta (referencia a AppUser).</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Lista de consumos/pagos realizados con esta tarjeta.</summary>
        public IList<Consumption> Consumptions { get; set; } = new List<Consumption>();
    }
}