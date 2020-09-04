using System.ComponentModel.DataAnnotations;

namespace WakilRecouvrement.Web.Models
{
    public class Compte
    {

        [Required(ErrorMessage = "Champ obligatoire")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Champ obligatoire")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}