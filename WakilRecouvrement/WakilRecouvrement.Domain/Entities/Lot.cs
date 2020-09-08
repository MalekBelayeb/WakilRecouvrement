using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public class Lot
    {

        public int LotId{get;set;}

        public string NumLot { get; set; }

        public string Compte { get; set; }
        
        public string IDClient { get; set; }

        public string NomClient { get; set; }
        
        public string TelPortable { get; set; }

        public string TelFixe { get; set; }

        public string Fax { get; set; }

        public string SoldeDebiteur { get; set; }

        public string DescIndustry { get; set; }

        public string Adresse { get; set; }
        
        public string Type { get; set; }
        
        public string Numero { get; set; }

        public string PostCode { get; set; }


        public int EmployeId { get; set; }
        public virtual Employe Employe { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Lot lot &&
                   NomClient == lot.NomClient &&
                   TelPortable == lot.TelPortable &&
                   TelFixe == lot.TelFixe &&
                   Fax == lot.Fax &&
                   SoldeDebiteur == lot.SoldeDebiteur &&
                   DescIndustry == lot.DescIndustry &&
                   Adresse == lot.Adresse &&
                   Type == lot.Type &&
                   Numero == lot.Numero &&
                   PostCode == lot.PostCode &&
                   NumLot == lot.NumLot &&
                   EmployeId == lot.EmployeId;
        }
    }
}
