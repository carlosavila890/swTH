﻿using System;
using System.Collections.Generic;
namespace bd.swth.entidades.Negocio
{
    public partial class ReliquidacionViatico
    {
        public int IdReliquidacionViatico { get; set; }
        public int IdItinerarioViatico { get; set; }
        public int? IdTipoTransporte { get; set; }
        public string NombreTransporte { get; set; }
        public int? IdCiudadOrigen { get; set; }
        public int? IdCiudadDestino { get; set; }
        public DateTime? FechaLlegada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public TimeSpan? HoraLlegada { get; set; }
        public TimeSpan? HoraSalida { get; set; }
        public string Descripcion { get; set; }
        public decimal? ValorEstimado { get; set; }

        public virtual Ciudad CiudadDestino { get; set; }
        public virtual Ciudad CiudadOrigen { get; set; }
        public virtual ItinerarioViatico ItinerarioViatico { get; set; }
        public virtual TipoTransporte TipoTransporte { get; set; }
    }
}