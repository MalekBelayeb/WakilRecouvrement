using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Web.Models.ViewModel
{
    public class ValiderTraitementViewModel
    {
        public Lot Lot { get; set; }
        public string Username { get; set; }
        public int FormulaireId { get; set; }
        public int AffectationId { get; set; }

        public double MontantVerseDeclare { get; set; }
        public bool ContactBanque { get; set; }
        public string VerifieLe { get; set; }
        public string DateAff { get; set; }
        public string TraiteLe { get; set; }
        public string Etat { get; set; }
        
        public string descAutre { get; set; }
    }
}