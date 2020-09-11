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

        static IDatabaseFactory Factory = new DatabaseFactory();
        static IUnitOfWork UOW = new UnitOfWork(Factory);

        public AffectationService() : base(UOW)
        {

        }
        public IEnumerable<Affectation> GetAffectationByLot(string NumLot)
        {
            return GetMany(a=>a.Lot.NumLot == NumLot);
        }

    }
}
