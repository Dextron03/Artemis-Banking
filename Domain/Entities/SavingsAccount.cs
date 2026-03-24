using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SavingsAccount
    {
        /// <summary>Identificador único de la cuenta (GUID).</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Número de cuenta de 9 dígitos visible para el cliente.</summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>Saldo disponible en la cuenta.</summary>
        public decimal Balance { get; private set; } = 0;

        /// <summary>ID del usuario dueño de esta cuenta (referencia a AppUser).</summary>
        public string UserId { get; set; } = string.Empty;
        /// <summary>ID del administrador que creó esta cuenta (aplica a cuentas secundarias).</summary>
        public string? AdminId { get; set; }
        /// <summary>Indica si esta cuenta es la cuenta principal del cliente (solo puede tener una).</summary>
        public bool IsPrincipal { get; set; } = false;

        /// <summary>Indica si la cuenta está activa. Se puede desactivar pero no eliminar.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Historial de transacciones asociadas a esta cuenta.</summary>
        public IList<Transaction> Transactions { get; set; } = new List<Transaction>();

        /// <summary>Fecha y hora en que fue creada la cuenta.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public void SetBalance(decimal balance)
        {
            if(balance < 0)
            {
                throw new ArgumentException("El saldo no debe ser negativo");
            }

            Balance = balance;
        }
    }
}