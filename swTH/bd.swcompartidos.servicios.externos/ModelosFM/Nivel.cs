﻿using System;
using System.Collections.Generic;

namespace bd.sw.externos.ModelosFM
{
    public partial class Nivel
    {
        public Nivel()
        {
            ComportamientoObservable = new HashSet<ComportamientoObservable>();
        }

        public int IdNivel { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<ComportamientoObservable> ComportamientoObservable { get; set; }
    }
}
