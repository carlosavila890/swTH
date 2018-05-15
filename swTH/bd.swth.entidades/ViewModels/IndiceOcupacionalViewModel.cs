﻿using System;
using System.Collections.Generic;
using System.Text;

namespace bd.swth.entidades.ViewModels
{
  public  class IndiceOcupacionalViewModel
    {
        public int IdIndiceOcupacional { get; set; }
        public string Dependencia { get; set; }
        public int Grado { get; set; }
        public string ManualPuesto { get; set; }
        public string Mision { get; set; }
        public string RelacionesInternasExternas { get; set; }
        public string RolPuesto { get; set; }
        public string EscalaGrado { get; set; }
        public decimal Remuneracion { get; set; }
        public string ModalidadPartida { get; set; }
        public int? PartidaGeneral { get; set; }
        public string PartidaIndividual { get; set; }
        public string Ambito { get; set; }
        public string Nivel { get; set; }
        public string Sucursal { get; set; }
        public string CodigoDepencia { get; set; }
    }
}
