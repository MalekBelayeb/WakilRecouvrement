
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Data;

namespace MyFinance.Data.Infrastructure
{
    public class DatabaseFactory : Disposable, IDatabaseFactory
    {
        private WakilRecouvContext dataContext; 
        public WakilRecouvContext DataContext { get { return dataContext; } }

        public DatabaseFactory()
        {
            dataContext = new WakilRecouvContext();
        }

        protected override void DisposeCore()
        {
            if (DataContext != null)
                DataContext.Dispose();
        }
    }

}
