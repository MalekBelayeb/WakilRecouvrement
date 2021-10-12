using Microsoft.Ajax.Utilities;
using MyFinance.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Web.Models.ViewModel;

namespace WakilRecouvrement.Web.Controllers
{
    public class HomeController : Controller
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");


        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            log.Error(filterContext.Exception); 
        }

        public HomeController()
        {

        }

        public IEnumerable<SelectListItem> AgentListForDropDown(EmployeService EmpService)
        {

            Employe userConnected = EmpService.GetEmployeByUername(Session["username"] + "");
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "", Value = "-1" });

            if (userConnected != null)
                {
                if (userConnected.RoleId == 2)
                {

                    List<Employe> agents = EmpService.GetMany(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true).ToList();

                    listItems.Add(new SelectListItem {Text = "Tous les agents", Value = "0" });

                    agents.ForEach(l =>
                    {
                        listItems.Add(new SelectListItem { Text = l.Username, Value = l.Username + "" });
                    });

                    return listItems;
                }
                else
                {


                    listItems.Add(new SelectListItem { Text = userConnected.Username, Value = userConnected.Username + "" });
                    listItems.Add(new SelectListItem { Text = "Tous les agents", Value = "0" });

                    return listItems;

                }
            }
            else
            {
                return listItems;
            }
            
          


        }

        public ActionResult AccountList()
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    EmployeService EmpService = new EmployeService(UOW);

                    RoleService RoleService = new RoleService(UOW);

                   
                    var Roles = RoleService.GetAll();
                    ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

                    return View(EmpService.GetAll());

                }
            }
                   



        }



        [HttpPost]
        public ActionResult AccountList(string type)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    EmployeService EmpService = new EmployeService(UOW);
                    RoleService RoleService = new RoleService(UOW);


                    var Roles = RoleService.GetAll();
                    ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

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
            }

        }


        public ActionResult UpdateAccount(int id)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    EmployeService EmpService = new EmployeService(UOW);
                    RoleService RoleService = new RoleService(UOW);
                
                    var Roles = RoleService.GetAll();
                    ViewBag.RoleList = new SelectList(Roles, "RoleId", "role");

                    return View(EmpService.GetById(id));
                }
            }

        }

        [HttpPost]
        public ActionResult UpdateAccount(int id, Employe e)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    EmployeService EmpService = new EmployeService(UOW);
                    RoleService RoleService = new RoleService(UOW);
                 
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
            }


                    
        }



        [HttpPost]
        public ActionResult GestionDesComptes()
        {
            return View();
        }

        
        public ActionResult Index(string agent)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    EmployeService EmpService = new EmployeService(UOW);
                    RoleService RoleService = new RoleService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);

                    List<ClientAffecteViewModel> traiteList = new List<ClientAffecteViewModel>();
                    ViewBag.AgentList = new SelectList(AgentListForDropDown(EmpService), "Value", "Text");

                    List<HomeViewModel> result = new List<HomeViewModel>();
                    Debug.WriteLine("agent "+agent);

                    if(string.IsNullOrEmpty(agent)==false)
                    {
                        if (agent != "-1")
                        {

                            List<Lot> lots = new List<Lot>();

                            string[] lotsLst = { };

                            lots = LotService.GetAll().ToList();

                            List<Affectation> Affectations = new List<Affectation>();
                            List<Formulaire> Formulaires = new List<Formulaire>();
                            string[] agents = { };


                            Employe emp = EmpService.GetEmployeByUername(agent + "");

                            if (emp == null)
                            {
                                emp = EmpService.GetEmployeByUername(Session["Username"] + "");
                            }

                            if (agent != "0")
                            {

                                if (emp.RoleId == 1)
                                {
                                    Affectations = AffectationService.GetMany(a => a.EmployeId == emp.EmployeId).ToList();
                                }
                                else if (emp.RoleId == 2)
                                {
                                    Affectations = AffectationService.GetAll().ToList();
                                }

                            }
                            else
                            {

                                Affectations = AffectationService.GetAll().ToList();

                            }

                            Formulaires = FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe).ToList();

                            lotsLst = (from a in Affectations
                                       join l in lots on a.LotId equals l.LotId
                                       select new ClientAffecteViewModel
                                       {

                                           Lot = l

                                       }).Select(j => j.Lot).DistinctBy(l => l.NumLot).Select(l => l.NumLot).ToArray();

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
                                    avancement = avgLot.Replace(",", ".")
                                };

                                result.Add(homeViewModel);

                            }
                        }
                        
                    }
                    ViewBag.total = result.Count();
                    return View(result.OrderBy(j=>double.Parse(j.avancement.Replace(".", ","))));

                }
            }


        }

 
        
      
    }
}
