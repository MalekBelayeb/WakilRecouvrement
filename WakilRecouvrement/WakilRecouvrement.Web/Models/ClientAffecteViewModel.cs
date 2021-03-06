﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Web.Models
{
    public class ClientAffecteViewModel
    {
        public Affectation Affectation { get; set; }

        public Formulaire Formulaire { get; set; }

        public Lot Lot { get; set; }

        public string Agent { get; set; }
        public int NbCountHist { get; set; }
        public double vers { get; set; }
        public double recouvre { get; set; }
        public bool IsDeletable { get; set; }
        public int AffectationId { get; set; }
    }
}