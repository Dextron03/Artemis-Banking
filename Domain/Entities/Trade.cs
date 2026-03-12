using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{

    public enum TradeStatus
    {
        Aprovado,
        Rechazado
    }

    public class Trade
    {
        /// <summary>Identificador único del comercio (GUID).</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Nombre del comercio (ej: "Supermercado Nacional").</summary>
        public string Name { get; set; }

        /// <summary>Descripción breve del comercio y sus servicios.</summary>
        public string Description { get; set; }

        /// <summary>Ruta o URL del logotipo del comercio.</summary>
        public string PathLogo { get; set; }

        /// <summary>Estado del comercio: Aprobado (activo) o Rechazado (inactivo).</summary>
        public TradeStatus Status { get; set; }

        /// <summary>ID del usuario asociado a este comercio (referencia a AppUser con rol Comercio).</summary>
        public string UserId { get; set; } = string.Empty;
    }
}