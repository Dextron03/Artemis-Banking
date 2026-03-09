using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class Transaction
    {
        /// <summary>Identificador único de la transacción (GUID).</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Monto de la transacción.</summary>
        public decimal Amount { get; set; }

        /// <summary>Tipo de transacción: Depósito, Retiro, Transferencia o Pago.</summary>
        public TransactionType Type { get; set; }

        /// <summary>Origen del dinero: número de cuenta, tarjeta, o el texto "DEPÓSITO" si es en efectivo.</summary>
        public string Origin { get; set; } = string.Empty;

        /// <summary>Destino del dinero: número de cuenta, préstamo, tarjeta, o el texto "RETIRO" si es en efectivo.</summary>
        public string Beneficiary { get; set; } = string.Empty;

        /// <summary>Estado de la transacción: "APROBADA" o "RECHAZADA".</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>FK hacia la cuenta de ahorro a la que pertenece esta transacción.</summary>
        public string SavingAccountId { get; set; } = string.Empty;

        /// <summary>Propiedad de navegación hacia la cuenta de ahorro asociada.</summary>
        public SavingsAccount SavingsAccount { get; set; }

        /// <summary>Fecha y hora en que se realizó la transacción.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}