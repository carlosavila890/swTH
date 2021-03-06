namespace bd.swth.entidades.Negocio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Proceso
    {
        [Key]
        public int IdProceso { get; set; }

        //Propiedades Virtuales Referencias a otras clases
        [Required(ErrorMessage = "Debe introducir {0}")]
        [Display(Name = "Proceso:")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "El {0} no puede tener m�s de {1} y menos de {2}")]
        public string Nombre { get; set; }

        public virtual ICollection<ProcesoDetalle> ProcesoDetalle { get; set; }
        public virtual ICollection<Dependencia> Dependencia { get; set; }
        public virtual ICollection<SituacionPropuesta> SituacionPropuesta { get; set; }
    }
}