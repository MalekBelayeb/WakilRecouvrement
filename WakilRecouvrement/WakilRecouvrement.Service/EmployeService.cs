using MyFinance.Data.Infrastructure;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Service
{
    public class EmployeService:Service<Employe>, IEmployeService
    {

        public EmployeService(UnitOfWork UOW) :base(UOW)
        {
            //UOW = new UnitOfWork(wakilRecouvContext);
        }

        public IEnumerable<Employe> GetEmployeByRole(string role)
        {
            return GetMany(e => e.Role.role.Equals(role));
        }

        public IEnumerable<Employe> GetEmployeByVerified(bool isVerified)
        {
            return GetMany(e => e.IsVerified.Equals(isVerified));
        }

        public Employe GetEmployeByUername(string username)
        {
            return Get(e => e.Username.Equals(username));
        }


    }
}
