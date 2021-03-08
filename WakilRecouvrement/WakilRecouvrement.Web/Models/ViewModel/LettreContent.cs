using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WakilRecouvrement.Web.Models.ViewModel
{
    public class LettreContent
    {
        public string NumLot { get; set; }
        public string NomClient { get; set; }
        public string Agence { get; set; }
        public string Adresse { get; set; }
        public string Compte { get; set; }
        
        public string IdAgent { get; set; }


    }
}