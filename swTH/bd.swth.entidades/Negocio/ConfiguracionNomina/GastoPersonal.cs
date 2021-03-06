﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace bd.swth.entidades.Negocio
{
  public  class GastoPersonal
    {
        [Key]
        public int IdGastoPersonal { get; set; }

        public int Ano { get; set; }

        public double Valor { get; set; }

        public int IdTipoGastoPersonal { get; set; }
        public virtual TipoDeGastoPersonal TipoDeGastoPersonal { get; set; }

        public int IdEmpleado { get; set; }
        public virtual Empleado Empleado { get; set; }




    }
}
