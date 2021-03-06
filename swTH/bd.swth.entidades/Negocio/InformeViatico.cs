namespace bd.swth.entidades.Negocio
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class InformeViatico
    {
        [Key]
        public int IdInformeViatico { get; set; }
        public int IdSolicitudViatico { get; set; }
        public int IdTipoTransporte { get; set; }
        public string NombreTransporte { get; set; }
        public int IdCiudadOrigen { get; set; }
        public int IdCiudadDestino { get; set; }
        public DateTime FechaLlegada { get; set; }
        public DateTime FechaSalida { get; set; }
        public TimeSpan HoraLlegada { get; set; }
        public TimeSpan HoraSalida { get; set; }
        public decimal ValorEstimado { get; set; }

        public virtual Ciudad CiudadDestino{ get; set; }
        public virtual Ciudad CiudadOrigen { get; set; }
        public virtual SolicitudViatico SolicitudViatico { get; set; }
        public virtual TipoTransporte TipoTransporte { get; set; }
    }
}
