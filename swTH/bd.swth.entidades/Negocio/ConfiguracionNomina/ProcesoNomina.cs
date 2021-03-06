﻿using System.Collections.Generic;

namespace bd.swth.entidades.Negocio
{
    public partial class ProcesoNomina
    {
        public ProcesoNomina()
        {
            ConceptoNomina = new HashSet<ConceptoNomina>();
        }

        public int IdProceso { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }

        public virtual ICollection<ConceptoNomina> ConceptoNomina { get; set; }
        public virtual ICollection<CalculoNomina> CalculoNomina { get; set; }
    }
}
