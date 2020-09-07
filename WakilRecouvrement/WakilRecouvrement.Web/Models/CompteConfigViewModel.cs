using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WakilRecouvrement.Web.Models.ValidationModelsAttribute;

namespace WakilRecouvrement.Web.Models
{
    public class CompteConfigViewModel
    {

        [Required(ErrorMessage = "Champ obligatoire")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Champ obligatoire")]
        [DataType(DataType.Password)]
        [NotEqual("Password",ErrorMessage = " le nouveau mot de passe est identique a l'actuel ")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Champ obligatoire")]
        [Compare("NewPassword",ErrorMessage = "Votre mot de passe de confirmation ne correspond pas au nouveau mot de passe")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}