using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public class Employe
    {
        public int EmployeId { get; set; }
        
        [Display(Name ="Nom d'utilisateur")]
        [Required(ErrorMessage = "Champ obligatoire")]
        [StringLength(25,ErrorMessage = "Username doit etre inferieure a 25")]        
        public string Username { get; set; }

        [Required(ErrorMessage = "Champ obligatoire")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [ScaffoldColumn(false)]
        [Required(ErrorMessage = "Champ obligatoire")]
        [NotMapped]
        [Compare("Password",ErrorMessage = "Votre mot de passe de confirmation ne correspond pas au nouveau mot de passe")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        [Display(Name = "Verifié")]
        [DefaultValue(false)]   
        public bool IsVerified { get; set; }
        
        public virtual ICollection<Lot> Lots { get; set; }
        
    }
}
