using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public enum Note
    {
        [Display(Name ="Injoignable")]
        INJOIGNABLE,
        [Display(Name ="Faux Numero")]
        FAUX_NUM,
        [Display(Name = "Ne Répond Pas")]
        NRP,
        [Display(Name ="Rendez-vous")]
        RDV,
        [Display(Name = "Rendez-vous Reporté")]
        RDV_REPORTE,
        [Display(Name = "Soldé")]
        SOLDE,
        [Display(Name = "Soldé partiellement")]
        SOLDE_TRANCHE,
        [Display(Name = "Refus De Paiement")]
        REFUS_PAIEMENT,
        [Display(Name = "A Verifié")]
        A_VERIFIE,
        [Display(Name = "Rappel")]
        RAPPEL,
        [Display(Name = "Raccroche")]
        RACCROCHE,
        [Display(Name = "Autre")]
        AUTRE
    }
}
