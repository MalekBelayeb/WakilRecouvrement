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
        static IDatabaseFactory Factory = new DatabaseFactory();
        static IUnitOfWork UOW = new UnitOfWork(Factory);
        public RoleService() : base(UOW)
        {

        }
    }
}
