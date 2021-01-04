using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WakilRecouvrement.Web.Models.ViewModel
{
    public class FactureContent
    {
        public string FacNum { get; set; }
        public DateTime Date { get; set; }
        public string Beneficiere { get; set; }
        public string Lots { get; set; }
        public double PrixHT { get; set; } 
        public double PrixTVA { get; set; } 
        public double PrixTTC { get; set; } 
        public double TimbreFiscal { get; set; }
    } 
}