using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WakilRecouvrement.Web.Models
{
    public class FormulaireExportable
    {
       public string NumLot { get; set; }
       public string Compte { get; set; }
       public string NomClient { get; set; }
       public string IDClient { get; set; }
       public string Etat { get; set; }
       public string Versement { get; set; }
       public string MontantDebInitial { get; set; }
       public string MontantRecouvre { get; set; }
       public string RDV { get; set; }
       public string TotRecouvre { get; set; }
    


    }
}