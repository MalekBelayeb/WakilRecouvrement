using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public class Lot
    {

        public int LotId{get;set;}
        
        public long IDClient { get; set; }

        public string NomClient { get; set; }
        
        public long TelPortable { get; set; }

        public long TelFixe { get; set; }

        public long Fax { get; set; }

        public long SoldeDebiteur { get; set; }

        public string DescIndustry { get; set; }

        public string Adresse { get; set; }

        public int PostCode { get; set; }

        public int EmployeId { get; set; }
        public virtual Employe Employe { get; set; }

    }
}
