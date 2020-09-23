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
    public class EmployeService:Service<Employe>, IEmployeService
    {
        static IDatabaseFactory Factory = new DatabaseFactory();
        static IUnitOfWork UOW = new UnitOfWork(Factory);

        public EmployeService():base(UOW)
        {

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
