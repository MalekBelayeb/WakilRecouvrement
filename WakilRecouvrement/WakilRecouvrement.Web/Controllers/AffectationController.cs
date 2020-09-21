using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;

namespace WakilRecouvrement.Web.Controllers
{
    public class AffectationController : Controller
    {

        EmployeService EmpService;
        LotService LotService;
        AffectationService AffectationService;
        FormulaireService FormulaireService;

        public AffectationController()
        {
            
            LotService = new LotService();
            AffectationService = new AffectationService();
            EmpService = new EmployeService();
            FormulaireService = new FormulaireService();

        }

        public ActionResult ChoisirLot()
        {
            List<Lot> Lots = LotService.GetAll().ToList();

            return View(Lots.DistinctBy(l => l.NumLot));
        }

        public ActionResult AffecterAgent(int numLot)
        {

            List<Lot> listClients = LotService.GetClientsByLot(numLot + "").ToList();

            var listAffectations = AffectationService.GetAffectationByLot(numLot+"");
            
            var agents = EmpService.GetAll().Where(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true);

            ViewBag.AgentList = new SelectList(agents, "EmployeId", "Username");

            int totalClientParLot = listClients.Count();

            var clientsNonAffecteList = from lot in listClients
                                        where !(from aff in listAffectations
                                                select aff.LotId).Contains(lot.LotId)
                                        select lot;

            int nbClientsNonAffecteParLots = clientsNonAffecteList.Count() ;

            int totalAffectationParLot = listAffectations.Count();

            float pourcentageAffectationParLot = ((float)totalAffectationParLot / (float)totalClientParLot) * 100;

            ViewData["numLot"] = numLot;
            ViewData["totalClientsParLots"] = totalClientParLot;
            ViewData["totalAffectationParLots"] = totalAffectationParLot;
            ViewData["totalClientsRestantParLots"] = nbClientsNonAffecteParLots;
            ViewData["pourcentageAffectationParLot"] = (int)pourcentageAffectationParLot;
         
            return View();
        }
    
        
        public void updateViewForAgent(int numLot,string agent)
        {


        }

        [HttpPost]
        public ActionResult AffecterAgents(int numLot, int nbClient,int agent)
        {

            List<Lot> listClients = LotService.GetClientsByLot(numLot + "").ToList();

            List<Affectation> listAffectations = AffectationService.GetAffectationByLot(numLot+"").ToList();

            var clientsNonAffecteList = from lot in listClients
                        where !(from aff in listAffectations 
                                select aff.LotId).Contains(lot.LotId)
                        select lot;

            int nbClientsNonAffecteParLots = clientsNonAffecteList.Count();
           
            for (int i=0;i<nbClient;i++)
            {
             
                AffectationService.Add(new Affectation { EmployeId = agent, AffectePar = "", DateAffectation = DateTime.Now, LotId = clientsNonAffecteList.ToList()[i].LotId });

            }

            AffectationService.Commit();

            var result = UpdateView(numLot);

            return result;
           }

        public ActionResult UpdateView(int numLot)
        {

            List<Lot> listClientsUpdated = LotService.GetClientsByLot(numLot + "").ToList();

            var listAffectationsUpdated = AffectationService.GetAffectationByLot(numLot + "");

            int totalClientParLotUpdated = listClientsUpdated.Count();

            var clientsNonAffecteListUpdated = from lot in listClientsUpdated
                                               where !(from aff in listAffectationsUpdated
                                                       select aff.LotId).Contains(lot.LotId)
                                               select lot;

            int nbClientsNonAffecteParLotsUpdated = clientsNonAffecteListUpdated.Count();

            int totalAffectationParLotUpdated = listAffectationsUpdated.Count();

            float pourcentageAffectationParLotUpdated = ((float)totalAffectationParLotUpdated / (float)totalClientParLotUpdated) * 100;


            return Json(new { totalClientParLotUpdated = totalClientParLotUpdated, nbClientsNonAffecteParLots = nbClientsNonAffecteParLotsUpdated, pourcentageAffectationParLotUpdated = pourcentageAffectationParLotUpdated, totalAffectationParLotUpdated = totalAffectationParLotUpdated });

        }

        public ActionResult AffectationList()
        {
            if(Session["username"]==null || Session["username"].ToString().Length<1)
                return RedirectToAction("Login","Authentification");
            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");

            ViewBag.TraiteList = new SelectList(TraiteListForDropDown(), "Value", "Text");


            return View();
        }

        public IEnumerable<SelectListItem> TraiteListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Touts les clients affectés", Value = "ALL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Tout les traités sauf SOLDE/FAUX_NUM", Value = "SAUF" });

            foreach (var n in Enum.GetValues(typeof(Note)))
            {

                listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString() });

            }


            return listItems;
        }
        public IEnumerable<SelectListItem> TraiteListModifierAffForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "NON_TRAITES", Value = "NON_TRAITES" });
            listItems.Add(new SelectListItem { Selected = true, Text = "RDV (d'aujourd'hui)", Value = "RDV" });
            listItems.Add(new SelectListItem { Selected = true, Text = "RDV_REPORTE (d'aujourd'hui)", Value = "RDV_REPORTE" });
            listItems.Add(new SelectListItem { Selected = true, Text = "RAPPEL", Value = "RAPPEL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "RACCROCHE", Value = "RACCROCHE" });
            listItems.Add(new SelectListItem { Selected = true, Text = "NRP", Value = "NRP" });
            listItems.Add(new SelectListItem { Selected = true, Text = "INJOIGNABLE", Value = "INJOIGNABLE" });

            return listItems;
        }

        public IEnumerable<SelectListItem> NumLotListForDropDown()
        {

            List<Lot> Lots = LotService.GetAll().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Touts les lots", Value = "0" });

            Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
            });


            List<Affectation> listAffectation = AffectationService.GetAll().ToList().Where(a => a.Employe.Username.Equals(Session["username"])).ToList();

           var JoinedList = (from a in listAffectation
                          join l in Lots on a.LotId equals l.LotId
                          
                          select new ClientAffecteViewModel
                          {
                              Affectation = a,
                              Lot = l,
                          }).ToList();

            ViewBag.affectationRestant = JoinedList.Count();
            ViewBag.totalAffectation = listAffectation.Count();


            return listItems;
        }

        public IEnumerable<SelectListItem> AgentListForDropDown()
        {

            List<Employe> agents = EmpService.GetAll().Where(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true).ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Touts les agents", Value = "0" });

            agents.ForEach(l => {
                listItems.Add(new SelectListItem { Text = l.Username, Value = l.EmployeId + "" });
            });

            return listItems;
        }


        [HttpPost]
        public ActionResult LoadData(string numLot, string traite)
        {
            List<Lot> listLot = new List<Lot>();
            List<Affectation> listAffectation = new List<Affectation>();
            List<Formulaire> listFormulaire = new List<Formulaire>();
            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteListForDropDown(), "Value", "Text");

            if (traite == "ALL")
            {
                JoinedList = (from a in AffectationService.GetAll()
                              join l in LotService.GetAll() on a.LotId equals l.LotId

                              select new ClientAffecteViewModel
                              {
                                  Formulaire = FormulaireService.GetOrderedFormulaireByAffectation(a.AffectationId),
                                  Affectation = a,
                                  Lot = l,

                              }).ToList().OrderByDescending(o => o.Affectation.DateAffectation).DistinctBy(a => a.Affectation.AffectationId).ToList().Where(j => j.Affectation.Employe.Username.Equals(Session["username"])).ToList();

            }
            else if (traite == "SAUF")
            {

                JoinedList = (from f in FormulaireService.GetAll()
                              join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                              join l in LotService.GetAll() on a.LotId equals l.LotId
                              where f.EtatClient != (Note)Enum.Parse(typeof(Note), "SOLDE") && f.EtatClient != (Note)Enum.Parse(typeof(Note), "FAUX_NUM")

                              select new ClientAffecteViewModel
                              {

                                  Formulaire = f,
                                  Affectation = a,
                                  Lot = l,

                              }).ToList().OrderByDescending(o => o.Formulaire.TraiteLe).DistinctBy(d => d.Formulaire.AffectationId).ToList().Where(j => j.Affectation.Employe.Username.Equals(Session["username"])).ToList();

            }
            else
            {
                JoinedList = (from f in FormulaireService.GetAll()
                              join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                              join l in LotService.GetAll() on a.LotId equals l.LotId
                              orderby f.TraiteLe descending
                              select new ClientAffecteViewModel
                              {

                                  Formulaire = f,
                                  Affectation = a,
                                  Lot = l,

                              }).ToList().OrderByDescending(o => o.Formulaire.TraiteLe).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), traite)).ToList().Where(j => j.Affectation.Employe.Username.Equals(Session["username"])).ToList(); ;

            }


            if (numLot != "0")
            {
                JoinedList = JoinedList.ToList().Where(l => l.Lot.NumLot.Equals(numLot)).ToList();
            }
           

            JsonResult result = new JsonResult();

            try
            {

                string search = Request.Form.GetValues("search[value]")[0];
                string draw = Request.Form.GetValues("draw")[0];
                string order = Request.Form.GetValues("order[0][column]")[0];
                string orderDir = Request.Form.GetValues("order[0][dir]")[0];
                int startRec = Convert.ToInt32(Request.Form.GetValues("start")[0]);
                int pageSize = Convert.ToInt32(Request.Form.GetValues("length")[0]);


                int totalRecords = JoinedList.Count();

                if (!string.IsNullOrEmpty(search) &&
                    !string.IsNullOrWhiteSpace(search))
                {
                    JoinedList = JoinedList.Where(j =>
                      j.Lot.Numero.ToString().Contains(search)
                    || j.Lot.Adresse.ToString().ToLower().Contains(search.ToLower())
                    || j.Lot.TelFixe.ToString().Contains(search)
                    || j.Lot.TelPortable.ToString().Contains(search)
                    || j.Lot.IDClient.ToString().Contains(search)
                    || j.Lot.Compte.ToString().Contains(search)
                    || j.Lot.LotId.ToString().Contains(search)
                    || j.Lot.NomClient.ToString().ToLower().Contains(search.ToLower())
                    || j.Lot.DescIndustry.ToString().ToLower().Contains(search.ToLower())

                        ).ToList();
                }


                JoinedList = SortTableData(order, orderDir, JoinedList);

                int recFilter = JoinedList.Count();

                JoinedList = JoinedList.Skip(startRec).Take(pageSize).ToList();
                var modifiedData = JoinedList.Select(j =>
                   new
                   {
                       j.Lot.LotId,
                       j.Lot.NumLot,
                       j.Lot.Compte,
                       j.Lot.IDClient,
                       j.Lot.NomClient,
                       j.Lot.TelPortable,
                       j.Lot.TelFixe,
                       j.Lot.SoldeDebiteur,
                       j.Lot.DescIndustry,
                       j.Lot.Adresse,
                       j.Lot.Type,
                       j.Lot.Numero,
                       j.Affectation.AffectationId,

                       V = j.Affectation.DateAffectation.ToString(),
                    }
                   );
                result = this.Json(new
                {
                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecords,
                    recordsFiltered = recFilter,
                    data = modifiedData
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            return result;

        }


        private List<ClientAffecteViewModel> SortTableData(string order, string orderDir, List<ClientAffecteViewModel> data)
        {
            List<ClientAffecteViewModel> lst = new List<ClientAffecteViewModel>();
            try
            {
                switch (order)
                {
                    case "0":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Affectation.DateAffectation).ToList()
                                                                                                 : data.OrderBy(j => j.Affectation.DateAffectation).ToList();
                        break;
                    case "1":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.LotId).ToList()
                                                                                                 : data.OrderBy(l => l.Lot.LotId).ToList();
                        break;

                    case "2":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => long.Parse(j.Lot.NumLot)).ToList()
                                                                                                 : data.OrderBy(j => long.Parse(j.Lot.NumLot)).ToList();
                        break;
                    case "3":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.Compte).ToList()
                                                                                                 : data.OrderBy(j => j.Lot.Compte).ToList();
                        break;
                    case "4":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => long.Parse(j.Lot.IDClient)).ToList()
                                                                                                 : data.OrderBy(j => long.Parse(j.Lot.IDClient)).ToList();
                        break;
                    case "5":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.NomClient).ToList()
                                                                                                 : data.OrderBy(j => j.Lot.NomClient).ToList();
                        break;
                    case "6":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.TelPortable).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.TelPortable).ToList();
                        break;
                    case "7":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.TelFixe).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.TelFixe).ToList();
                        break;
                    case "8":

                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList()
                                                                                              : data.OrderBy(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList();

                        break;
                    case "9":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.DescIndustry).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.DescIndustry).ToList();
                        break;
                    case "10":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.Adresse).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.Adresse).ToList();
                        break;
                    case "11":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.Type).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.Type).ToList();
                        break;

                    case "12":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.Numero).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.Numero).ToList();
                        break;

                    default:
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.LotId).ToList()
                                                                                                 : data.OrderBy(j => j.Lot.LotId).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return lst;
        }


        public ActionResult ModifierAffectation(int numLot)
        {
            var agents = EmpService.GetAll().Where(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true);
            ViewBag.AgentList = new SelectList(agents, "EmployeId", "Username");
            ViewData["numLot"] = numLot;
            ViewBag.TraiteList = new SelectList(TraiteListModifierAffForDropDown(), "Value", "Text");

            return View();
        }

        [HttpPost]
        public ActionResult GetInfoAgent(string agentDe,string numLot,string traite)
        {
            int nbTotalAffectation = 0;
            int nbTotLots = 0;
            float pourcentageAff = 0;
            int pourcentageAgentDe = 0;



            if(traite.Equals("NON_TRAITES"))
            {
                nbTotalAffectation = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Count() == 0).Count();
                nbTotLots = LotService.GetAll().Where(l => l.NumLot.Equals(numLot)).Count();
                pourcentageAff = ((float)nbTotalAffectation / (float)nbTotLots) * 100;
                pourcentageAgentDe = (int)pourcentageAff;

                return Json(new { nbTotalAffectation = nbTotalAffectation, pourcentageAgentDe = pourcentageAgentDe, nbTotLots = nbTotLots });

            }
            else if(traite.Equals("RDV"))
            {

                nbTotalAffectation = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "RDV")&& a.Formulaires.Last().DateRDV.Date == DateTime.Today.Date).ToList().Count();
                nbTotLots = LotService.GetAll().Where(l => l.NumLot.Equals(numLot)).Count();
                pourcentageAff = ((float)nbTotalAffectation / (float)nbTotLots) * 100;
                pourcentageAgentDe = (int)pourcentageAff;

                return Json(new { nbTotalAffectation = nbTotalAffectation, pourcentageAgentDe = pourcentageAgentDe, nbTotLots = nbTotLots });

            }
            else if(traite.Equals("RDV_REPORTE"))
            {
                nbTotalAffectation = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "RDV_REPORTE")&& a.Formulaires.Last().DateRDVReporte.Date == DateTime.Today.Date).ToList().Count();
                nbTotLots = LotService.GetAll().Where(l => l.NumLot.Equals(numLot)).Count();
                pourcentageAff = ((float)nbTotalAffectation / (float)nbTotLots) * 100;
                pourcentageAgentDe = (int)pourcentageAff;

                return Json(new { nbTotalAffectation = nbTotalAffectation, pourcentageAgentDe = pourcentageAgentDe, nbTotLots = nbTotLots });

            }
            else
            {
                nbTotalAffectation = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), traite)).Count();
                nbTotLots = LotService.GetAll().Where(l => l.NumLot.Equals(numLot)).Count();
                pourcentageAff = ((float)nbTotalAffectation / (float)nbTotLots) * 100;
                pourcentageAgentDe = (int)pourcentageAff;

                return Json(new { nbTotalAffectation = nbTotalAffectation, pourcentageAgentDe = pourcentageAgentDe, nbTotLots = nbTotLots });

            }


        }

        [HttpPost]
        public ActionResult Reaffecter(string agentA,string agentDe, string numLot, string traite,int num)
        {

            List<Affectation> affectations = new List<Affectation>();
            if (traite.Equals("NON_TRAITES"))
            {

                affectations = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).ToList();
                                   
            }
            else if(traite.Equals("RDV"))
            {

                affectations = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "RDV")).Where(a=> a.Formulaires.Last().DateRDV.Date == DateTime.Today.Date).ToList();

            }
            else if(traite.Equals("RDV_REPORTE"))
            {
               
                affectations = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "RDV_REPORTE")).Where(a => a.Formulaires.Last().DateRDVReporte.Date == DateTime.Today.Date).ToList();

            }
            else
            {
                affectations = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), traite)).ToList();
            }


            for (int i = 0; i < num; i++)
            {
                Affectation aff = affectations[i];
                aff.EmployeId = int.Parse(agentA);

                AffectationService.Update(aff);
            }
            AffectationService.Commit();

            return Json(new { });
        }


}

    }

