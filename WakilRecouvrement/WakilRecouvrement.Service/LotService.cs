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
    public class LotService : Service<Lot>, ILotService
    {

        public LotService(UnitOfWork UOW) : base(UOW)
        {
            //UOW = new UnitOfWork(wakilRecouvContext);
        }

        public Lot GetClientByIDClient(string ID)
        {
            return Get(l => l.IDClient.Equals(ID));
        }

        public IEnumerable<Lot> GetClientsByLot(string numLot)
        {
            return GetMany(l => l.NumLot.Equals(numLot));
        }

    }
}
