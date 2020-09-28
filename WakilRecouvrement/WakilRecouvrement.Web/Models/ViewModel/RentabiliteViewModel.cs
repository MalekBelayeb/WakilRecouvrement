using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Web.Models.ViewModel
{
    public class RentabiliteViewModel
    {


        public Affectation Affectation { get; set; }

        public Formulaire Formulaire { get; set; }

        public Lot Lot { get; set; }

        public double Revenue { get; set; }

    }
}