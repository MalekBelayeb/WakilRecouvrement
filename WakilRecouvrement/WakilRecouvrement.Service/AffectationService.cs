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
    public class AffectationService : Service<Affectation>, IAffectationService
    {


        public AffectationService(UnitOfWork UOW) : base(UOW)
        {
            //UOW = new UnitOfWork(wakilRecouvContext);
        }
        public IEnumerable<Affectation> GetAffectationByLot(string NumLot)
        {
            return GetMany(a=>a.Lot.NumLot == NumLot);
        }

    }
}
