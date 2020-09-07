using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Service
{
    public interface ILotService : IService<Lot>
    {
         Lot GetClientByIDClient(string ID);

    }
}
