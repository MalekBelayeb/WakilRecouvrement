using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public class Formulaire
    {
                
        public int FormulaireId { get; set; }
        

        [Display(Name ="Etat Client")]
        [Required(ErrorMessage ="Vous devez selectionner une note")]
        public Note EtatClient { get; set; }

        [Display(Name = "Description Autre")]

        [DataType(DataType.MultilineText)]
        public string DescriptionAutre { get; set; }
        [Display(Name = "Date RDV")]

        [DataType(DataType.DateTime)]
        public DateTime DateRDV { get; set; }

        [Display(Name = "Date RDV Reporté")]

        [DataType(DataType.DateTime)]
        public DateTime DateRDVReporte { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime TraiteLe { get; set; }
       
        [DataType(DataType.DateTime)]
        public DateTime VerifieLe { get; set; }

        [Display(Name ="Montant declaré/versé en TND")]
        public double MontantVerseDeclare { get; set; }

        [DefaultValue(false)]
        public bool IsVerified { get; set; }
        
        public int AffectationId { get; set; }
        public Affectation Affectation { get; set; }
    
    }
}
