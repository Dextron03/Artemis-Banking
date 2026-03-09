using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Share
    {
        /// <summary>Identificador único de la cuota (GUID).</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Número de cuota dentro del préstamo (ej: 1 de 24, 2 de 24...).</summary>
        public int QuotaNumber { get; set; }

        /// <summary>Monto a pagar en esta cuota, calculado con la fórmula francesa.</summary>
        public decimal ShareAmount { get; set; } = 0;

        /// <summary>Indica si esta cuota ya fue pagada por el cliente.</summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>Indica si la cuota está en mora (fecha de pago ya pasó y no fue pagada).</summary>
        public bool IsDelayed { get; set; } = false;

        /// <summary>Fecha límite en que el cliente debe pagar esta cuota.</summary>
        public DateTime DatePay { get; set; }

        /// <summary>FK hacia el préstamo al que pertenece esta cuota.</summary>
        public string LoanId { get; set; } = string.Empty;

        /// <summary>Propiedad de navegación hacia el préstamo asociado.</summary>
        public Loan Loan { get; set; }
    }
}