using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Data.Conventions;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Data
{
    public class WakilRecouvContext: DbContext
    {
        public WakilRecouvContext():base("name= WRConnectionStrings")
        {
           
        }
        
        public DbSet<Employe> Employes { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Formulaire> Formulaires { get; set; }
        public DbSet<Affectation> Affectationss { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new DateTime2Convention());
            
        }

    }
}
