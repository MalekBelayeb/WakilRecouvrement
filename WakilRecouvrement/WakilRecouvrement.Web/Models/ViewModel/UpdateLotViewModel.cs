using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Web.Models.ViewModel
{
    public class UpdateLotViewModel
    {

        public int LotId { get; set; }
        public int AffectationId { get; set; }
        public int EmployeId { get; set; }

        public string NumLot { get; set; }
        public string Compte { get; set; }

        public string IDClient { get; set; }

        public string NomClient { get; set; }

        public string TelPortable { get; set; }
        public bool TelPortableFN { get; set; }

        public string TelFixe { get; set; }

        public bool TelFixeFN { get; set; }

        public string Emploi { get; set; }

        public double SoldeDebiteur { get; set; }

        public string DescIndustry { get; set; }

        public string Adresse { get; set; }

        public string Numero { get; set; }

        public string PostCode { get; set; }


    }
}
