﻿namespace bd.swth.entidades.Negocio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PlanCapacitacion
    {
        public int IdPlanCapacitacion { get; set; }
        public int? IdGestionPlanCapacitacion { get; set; }
        public string NumeroPartidaPresupuestaria { get; set; }
        public string Institucion { get; set; }
        public string Pais { get; set; }
        public string Provincia { get; set; }
        public string NombreCiudad { get; set; }
        public string NivelDesconcentracion { get; set; }
        public string UnidadAdministrativa { get; set; }
        public string Cedula { get; set; }
        public string ApellidoNombre { get; set; }
        public string Sexo { get; set; }
        public string GrupoOcupacional { get; set; }
        public string DenominacionPuesto { get; set; }
        public string RegimenLaboral { get; set; }
        public string ModalidadLaboral { get; set; }
        public string TemaCapacitacion { get; set; }
        public string ClasificacionTema { get; set; }
        public string ProductoFinal { get; set; }
        public string Modalidad { get; set; }
        public int? Duracion { get; set; }
        public decimal? PresupuestoIndividual { get; set; }
        public DateTime? FechaCapacitacionPlanificada { get; set; }
        public int? TipoCapacitacion { get; set; }
        public int? EstadoEvento { get; set; }
        public int? AmbitoCapacitacion { get; set; }
        public int? NombreEvento { get; set; }
        public int? TipoEvento { get; set; }
        public int? IdProveedorCapacitaciones { get; set; }
        public int? DuracionEvento { get; set; }
        public int? Anio { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal? ValorReal { get; set; }
        public int? IdCiudad { get; set; }
        public int? TipoEvaluacion { get; set; }
        public string Ubicacion { get; set; }
        public string Observacion { get; set; }
        public int? Estado { get; set; }
        [NotMapped]
        public string Correo { get; set; }

        public virtual ICollection<EvaluacionEvento> EvaluacionEvento { get; set; }
        public virtual GeneralCapacitacion IdAmbitoCapacitacion { get; set; }
        public virtual GeneralCapacitacion IdEstadoEvento { get; set; }
        public virtual GeneralCapacitacion IdNombreEvento{ get; set; }
        public virtual GeneralCapacitacion IdTipoCapacitacion { get; set; }
        public virtual GeneralCapacitacion IdTipoEvaluacion { get; set; }
        public virtual GeneralCapacitacion IdTipoEvento { get; set; }

        public virtual Ciudad Ciudad { get; set; }
        public virtual GestionPlanCapacitacion GestionPlanCapacitacion { get; set; }
        public virtual CapacitacionProveedor ProveedorCapacitaciones { get; set; }

        [NotMapped]
        public bool Valido { get; set; }
        [NotMapped]
        public int IdPresupuesto { get; set; }
        [NotMapped]
        public int IdEmpleado { get; set; }
        [NotMapped]
        public string MensajeError { get; set; }
    }
}
