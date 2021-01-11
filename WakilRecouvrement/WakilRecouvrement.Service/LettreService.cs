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
    public class LettreService : Service<Lettre>, ILettreService
    {
        public LettreService(UnitOfWork UOW) : base(UOW)
        {

        }

    }
}
