
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Data;

namespace MyFinance.Data.Infrastructure
{
    public interface IDatabaseFactory : IDisposable
    {
        WakilRecouvContext DataContext { get; }
    }

}
