using System;
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
    }
}