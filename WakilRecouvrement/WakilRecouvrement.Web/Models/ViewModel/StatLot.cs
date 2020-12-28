using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WakilRecouvrement.Web.Models.ViewModel
{
    public class StatLot
    {

            public int nb { get; set; }
            public int fn { get; set; }
            public int rdv { get; set; }
            public int versement { get; set; }
            public int encours { get; set; }
            public string avgRdv { get; set; }
            public string avgFn { get; set; }
            public string avgVers { get; set; }
            public string avgencours { get; set; }
            
    }
}