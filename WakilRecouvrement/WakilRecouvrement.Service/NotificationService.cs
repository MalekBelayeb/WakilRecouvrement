using MyFinance.Data.Infrastructure;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Service
{
    public class NotificationService : Service<Notification>, INotificationService
    {

        public NotificationService(UnitOfWork UOW) : base(UOW)
        {
            //UOW = new UnitOfWork(wakilRecouvContext);
        }

    }
}
