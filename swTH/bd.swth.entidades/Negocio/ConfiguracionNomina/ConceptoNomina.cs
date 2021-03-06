﻿using System;
using System.Collections.Generic;

namespace bd.swth.entidades.Negocio
{
    public partial class ConceptoNomina
    {
        public ConceptoNomina()
        {
            ConceptoConjuntoNomina = new HashSet<ConceptoConjuntoNomina>();
            TeconceptoNomina = new HashSet<TeconceptoNomina>();
        }

        public int IdConcepto { get; set; }
        public int IdProceso { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public string TipoConcepto { get; set; }
        public string RelacionLaboral { get; set; }
        public string Estatus { get; set; }
        public string Abreviatura { get; set; }
        public string FormulaCalculo { get; set; }
        public int Prioridad { get; set; }

        public virtual ICollection<ConceptoConjuntoNomina> ConceptoConjuntoNomina { get; set; }
        public virtual ICollection<TeconceptoNomina> TeconceptoNomina { get; set; }
        public virtual ProcesoNomina ProcesoNomina { get; set; }
    }
}
