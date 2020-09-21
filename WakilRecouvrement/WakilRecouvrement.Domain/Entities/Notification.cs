using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public string ToSingle { get; set; }
        public string ToRole { get; set; }
        public string From { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public int FormulaireId { get; set; }

        public Formulaire Formulaire { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AddedIn { get; set; }


    }
}
