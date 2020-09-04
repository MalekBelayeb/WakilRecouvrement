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

           // wrc.Roles.Add(r1);
            // wrc.Employes.Add(emp);
         //   wrc.SaveChanges();

            // IEmployeService IEmpService = new EmployeService();
            //IRoleService IRoleService = new RoleService();
            //IRoleService.Add(r1);
            //IRoleService.Commit();
            ////IEmpService.Add(emp2);
            //IEmpService.Commit();
            //foreach(var emp in IEmpService.GetEmployeByRole("agent"))
            //{
            //    System.Console.WriteLine(emp.Username);
            //}

            //System.Console.WriteLine("Fin");


        }
    }
}
