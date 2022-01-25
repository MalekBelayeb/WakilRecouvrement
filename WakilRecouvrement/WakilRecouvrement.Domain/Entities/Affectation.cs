using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public class Affectation
    {

        public int AffectationId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateAffectation { get; set; }

        public string AffectePar { get; set; }

        public string AncienAgent { get; set; }

        public int LotId { get; set; }
        [ForeignKey("LotId")]
        public virtual Lot Lot { get; set; }

        public int EmployeId { get; set; }

        [ForeignKey("EmployeId")]
        public virtual Employe Employe {get;set;}

        public virtual ICollection<Formulaire> Formulaires { get; set; }    


    }
}
