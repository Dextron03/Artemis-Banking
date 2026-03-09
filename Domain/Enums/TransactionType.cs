namespace Domain.Enums
{
    public enum TransactionType
    {
        /// <summary>Depósito de dinero en efectivo a una cuenta.</summary>
        Deposit = 1,

        /// <summary>Retiro de dinero en efectivo de una cuenta.</summary>
        Withdrawal,

        /// <summary>Transferencia de dinero entre cuentas del sistema.</summary>
        Transfer,

        /// <summary>Pago a un préstamo o tarjeta de crédito.</summary>
        Payment
    }
}