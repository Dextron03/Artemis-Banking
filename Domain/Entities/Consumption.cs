using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Consumption
    {
        /// <summary>Identificador único del consumo (GUID).</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Fecha y hora en que se realizó el consumo.</summary>
        public DateTime ConsumptionDate { get; set; } = DateTime.Now;

        /// <summary>Monto del consumo o pago realizado.</summary>
        public decimal Amount { get; set; } = 0;

        /// <summary>Estado del consumo: true = APROBADO, false = RECHAZADO.</summary>
        public bool Status { get; set; } = false;

        /// <summary>Nombre del comercio donde se realizó el consumo.</summary>
        public string CommerceName { get; set; } = string.Empty;

        /// <summary>FK hacia la tarjeta de crédito con la que se realizó el consumo.</summary>
        public string CreditCardId { get; set; } = string.Empty;

        /// <summary>Propiedad de navegación hacia la tarjeta de crédito asociada.</summary>
        public CreditCard CreditCard { get; set; }
    }
}