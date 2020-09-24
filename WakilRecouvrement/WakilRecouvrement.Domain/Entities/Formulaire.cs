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

        [Display(Name = "Traite Le:")]

        [DataType(DataType.DateTime)]
        public DateTime TraiteLe { get; set; }
        [Display(Name = "Verifié Le:")]

        [DataType(DataType.DateTime)]
        public DateTime VerifieLe { get; set; }

        [Display(Name ="Montant declaré/versé en TND")]
        public double MontantVerseDeclare { get; set; }

        [Display(Name = "Montant debiteur initial en TND")]
        public double MontantDebInitial { get; set; }

        [Display(Name = "Montant debiteur MAJ en TND")]
        public double MontantDebMAJ { get; set; }

        [Display(Name = "Verification")]

        [DefaultValue(Status.EN_COURS)]
        public Status Status { get; set; }
       
        [DefaultValue(false)]
        public bool ContacteBanque { get; set; }

        [DefaultValue(false)]
        public bool NotifieBanque { get; set; }

        public int AffectationId { get; set; }
        public Affectation Affectation { get; set; }
    
    }
}
