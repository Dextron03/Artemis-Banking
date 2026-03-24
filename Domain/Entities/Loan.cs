using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum PaymentStatusType
    {
        AlDia,
        EnMora
    }


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
        public PaymentStatusType PaymentStatus { get; set; } = PaymentStatusType.AlDia;

        /// <summary>ID del cliente al que se le asignó el préstamo (referencia a AppUser).</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Fecha y hora en que fue creado/asignado el préstamo.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public decimal MonthlyPayment { get; set; }

        // <sumary> ID del administrador que asigna el préstamo
        public string CreatedByUserId { get; set; }

        /// <summary>Tabla de amortización: lista de todas las cuotas del préstamo.</summary>
        public IList<Share> Shares { get; set; } = new List<Share>();
    
        public void GenerateAmortizationTable()
        {
            if( LoanAmount <= 0 || TermMonths <=0 || InterestRate <= 0){
                throw new InvalidOperationException("Faltan datos para generar la tabla."); 
            }

            double P = (double) LoanAmount; // P: Monto del prestamo (Capital)

            double r = (double)(InterestRate / 100 / 12); // r = Tasa de interés periódica (mensual).
            //  Dividimos entre 100 para decimal y entre 12 para meses.+

            double n = TermMonths;
            
            // 2. Aplicar la fórmula del Sistema Francés
            double mathPower = Math.Pow(1 + r, n);
            double C = P*(r * mathPower) / (mathPower - 1); // C = Cuota constante

            MonthlyPayment = Math.Round((decimal)C, 2);

            decimal fixedQuota = (decimal)C;

            // 3. Generar las cuotas (Shares)
            // La primera cuota es el mismo día del mes siguiente a la creación
            DateTime nexPaymentDate = CreatedAt.AddMonths(1);

            for(int i = 1; i <= n; i++)
            {
                Shares.Add(new Share
                {
                    QuotaNumber = i,
                    ShareAmount = Math.Round(fixedQuota, 2), // Redondeamos a 2 decimales
                    DatePay = nexPaymentDate,
                    IsPaid = false,
                    IsDelayed = false
                });

                nexPaymentDate = nexPaymentDate.AddMonths(1);
            }

        }

        /// <summary> Recalcula las cuotas FUTURAS no pagadas con una nueva tasa de interés, 
        /// las cuotas ya pagadas o vencidas NO se modifican.</summary>
        public void RecalculateFutureShares(decimal newAnnualRate)
        {
            InterestRate = newAnnualRate;

            var futureUnpaid = new List<Share>();
            foreach (var s in Shares)
            {
                if (!s.IsPaid && s.DatePay > DateTime.UtcNow)
                    futureUnpaid.Add(s);
            }

            if (futureUnpaid.Count == 0) return;

            double P = (double)OutstandingAmount;
            double r = (double)(newAnnualRate / 100m / 12m);
            int n = futureUnpaid.Count;

            double factor = Math.Pow(1 + r, n);
            double rawC = P * (r * factor) / (factor - 1);
            decimal newQuota = Math.Round((decimal)rawC, 2, MidpointRounding.AwayFromZero);

            MonthlyPayment = newQuota;

            foreach (var s in futureUnpaid)
                s.ShareAmount = newQuota;
        }

        private static readonly Random _random = new Random();

        public static string GenerateUniqueNumber()
        {
            return _random.Next(100000000, 999999999).ToString();
        }

    }
}