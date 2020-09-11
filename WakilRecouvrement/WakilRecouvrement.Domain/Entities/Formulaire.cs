using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public class Formulaire
    {
                
        public int FormulaireId { get; set; }
        
        [Required(ErrorMessage ="Vous devez selectionner une note")]
        public Note EtatClient { get; set; }

        [DataType(DataType.MultilineText)]
        public string DescriptionAutre { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateRDV { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime DateRDVReporte { get; set; }
              
        public int AffectationId { get; set; }
        public Affectation Affectation { get; set; }
    
    }
}
