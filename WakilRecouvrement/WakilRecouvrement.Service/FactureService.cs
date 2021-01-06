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
    public class FactureService:Service<Facture>, IFactureService
    {

        public FactureService(UnitOfWork UOW) : base(UOW)
        {
            //UOW = new UnitOfWork(wakilRecouvContext);
        }
    }
}
