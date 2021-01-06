using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Domain.Entities;
using MyFinance.Data.Infrastructure;

namespace WakilRecouvrement.Service
{
   public class RoleService : Service<Role>, IRoleService
    {

        public RoleService(UnitOfWork UOW) : base(UOW)
        {
            //UOW = new UnitOfWork(wakilRecouvContext);
        }
    }
}
