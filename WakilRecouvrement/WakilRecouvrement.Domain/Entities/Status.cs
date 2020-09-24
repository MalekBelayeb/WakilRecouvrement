using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public enum Status
    {
        [Display(Name = "Verifié")]
        VERIFIE,
        [Display(Name = "En attente...")]
        EN_COURS,
        [Display(Name = "Non verfié")]
        NON_VERIFIE
    }
}
