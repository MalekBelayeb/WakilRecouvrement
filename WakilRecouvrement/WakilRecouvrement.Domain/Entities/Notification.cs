using System;
using System.Collections.Generic;
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
        public int AffectationId { get; set; }
        public string Message { get; set; }
        public Affectation Affectation { get; set; }       
    }
}
