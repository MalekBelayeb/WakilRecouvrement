using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public class RecuImage
    {
        public int RecuImageId { get; set; }
        public string ImageName { get; set; }

        public int FormulaireId { get; set; }
        [ForeignKey("FormulaireId")]
        public virtual Formulaire Formulaire { get; set; }


    }
}
