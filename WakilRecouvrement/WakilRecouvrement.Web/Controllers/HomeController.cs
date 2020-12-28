using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Web.Models.ViewModel;

namespace WakilRecouvrement.Web.Controllers
{
    public class HomeController : Controller
    {

        EmployeService EmpService;
        RoleService RoleService;

        AffectationService AffectationService;
        LotService LotService;
        FormulaireService FormulaireService;

        public HomeController()
        {
            EmpService = new EmployeService();
            RoleService = new RoleService();
          
            AffectationService = new AffectationService();
            LotService = new LotService();
            EmpService = new EmployeService();
            FormulaireService = new FormulaireService();

        }


        public ActionResult AccountList()
        {
            return View(EmpService.GetAll());
        }



        [HttpPost]
        public ActionResult AccountList(string type)
        {
            if (type == "0")
            {
                return View(EmpService.GetAll());
            }
            else if (type == "1")
            {
                return View(EmpService.GetEmployeByVerified(true));
            }
            else if (type == "2")
            {
                return View(EmpService.GetEmployeByVerified(false));
            }
            else
            {
                return View(EmpService.GetAll());
            }

        }


        public ActionResult UpdateAccount(int id)
        {
            var Roles = RoleService.GetAll();
            ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

            return View(EmpService.GetById(id));

        }

        [HttpPost]
        public ActionResult UpdateAccount(int id, Employe e)
        {
            var Roles = RoleService.GetAll();
            ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

            Employe emp = EmpService.GetById(id);
            emp.RoleId = e.RoleId;
            emp.IsVerified = e.IsVerified;
            emp.ConfirmPassword = emp.Password;

            EmpService.Update(emp);
            EmpService.Commit();

            return View("AccountList", EmpService.GetAll());
        }



        [HttpPost]
        public ActionResult GestionDesComptes()
        {
            return View();
        }

        
        public ActionResult Index()
        {

            List<ClientAffecteViewModel> traiteList = new List<ClientAffecteViewModel>();
            
            List<HomeViewModel> result = new List<HomeViewModel>();
            List<Lot> lots = new List<Lot>();
            string[] lotsLst = { };

            lots = LotService.GetAll().ToList();
            lotsLst = lots.DistinctBy(l => l.NumLot).Select(l => l.NumLot).ToArray();

            List<Affectation> Affectations = new List<Affectation>();
            List<Formulaire> Formulaires = new List<Formulaire>();
            string[] agents = { };

            Affectations = AffectationService.GetAll().ToList();
            Formulaires = FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe).ToList();

            foreach (string numlot in lotsLst)
            {
               
                lots = LotService.GetAll().Where(l => l.NumLot.Equals(numlot)).ToList();
                
                traiteList = (from f in Formulaires
                              join a in Affectations on f.AffectationId equals a.AffectationId
                              join l in lots on a.LotId equals l.LotId
                              select new ClientAffecteViewModel
                              {

                                  Formulaire = f,
                                  Affectation = a,
                                  Lot = l,

                              }).DistinctBy(d => d.Formulaire.AffectationId).ToList();

                var agentLinq = (from a in Affectations
                                 join l in lots on a.LotId equals l.LotId
                                 select new ClientAffecteViewModel
                                 {

                                     Affectation = a

                                 });


                agents = agentLinq.DistinctBy(a => a.Affectation.Employe.Username).Select(a => a.Affectation.Employe.Username).ToArray();

                string agentsStr = string.Join(", ", agents);

                float nbAffTotal = agentLinq.Count();
                float nbTraite = traiteList.Count();
                string avgLot = String.Format("{0:0.00}", (nbTraite / nbAffTotal) * 100);


                HomeViewModel homeViewModel = new HomeViewModel
                {
                    agents = agentsStr,
                    nbAffTotal = nbAffTotal + "",
                    nbTraite = nbTraite + "",
                    numLot = numlot,
                    avancement = avgLot.Replace(",",".")
                };
                result.Add(homeViewModel);

            }

            return View(result);
        }

 
        
      
    }
}
