using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Loan
    {
        /// <summary>Identificador único del préstamo (GUID).</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Número identificador de 9 dígitos visible para el cliente.</summary>
        public string IdentifierNumber { get; set; } = string.Empty;

        /// <summary>Monto total del préstamo otorgado.</summary>
        public decimal LoanAmount { get; set; } = 0;

        /// <summary>Plazo del préstamo en meses (ej: 6, 12, 24, 60).</summary>
        public int TermMonths { get; set; } = 0;

        /// <summary>Tasa de interés anual aplicada al préstamo (en porcentaje, ej: 12.5).</summary>
        public decimal InterestRate { get; set; } = 0;

        /// <summary>Monto pendiente por pagar (capital restante).</summary>
        public decimal OutstandingAmount { get; set; } = 0;

        /// <summary>Indica si el préstamo sigue activo. False cuando está completamente pagado.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Estado de pago: "Al Día" si está al corriente, "En Mora" si tiene cuotas vencidas.</summary>
        public string PaymentStatus { get; set; } = "Al Día";

        /// <summary>ID del cliente al que se le asignó el préstamo (referencia a AppUser).</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Fecha y hora en que fue creado/asignado el préstamo.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>Tabla de amortización: lista de todas las cuotas del préstamo.</summary>
        public IList<Share> Shares { get; set; } = new List<Share>();
    }
}