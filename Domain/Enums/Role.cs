using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum Role
    {
        /// <summary>Administrador del sistema: gestiona usuarios, préstamos, tarjetas y comercios.</summary>
        Administrador = 1,

        /// <summary>Cajero: realiza operaciones de depósito y retiro en efectivo.</summary>
        Cajero = 2,

        /// <summary>Cliente: accede al portal bancario para ver cuentas, transferir y pagar.</summary>
        Cliente = 3
    }
}
