using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;


namespace WakilRecouvrement.Console
{
    class Program
    {
        static void Main(string[] args)
        {

            WakilRecouvContext wrc = new WakilRecouvContext();
            Role r1 = new Role
            {
                role = "admin",
            };

            Employe emp2 = new Employe
            {
                Username = "admin2",
                Password = "admin2",
                ConfirmPassword = "admin2",
                RoleId = 1
            };
            
        }
    }
}
