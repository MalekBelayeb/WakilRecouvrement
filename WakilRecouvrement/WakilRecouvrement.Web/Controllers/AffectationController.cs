using Microsoft.Ajax.Utilities;
using MyFinance.Data.Infrastructure;
using PagedList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;

namespace WakilRecouvrement.Web.Controllers
{
    public class AffectationController : Controller
    {


        public int id = 0;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            log.Error(filterContext.Exception);
        }

        public ActionResult ChoisirLot()
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    List<Lot> Lots = LotService.GetAll().ToList();

                    return View(Lots.DistinctBy(l => l.NumLot));

                }
            }
                    
        }


        public ActionResult ChoisirAgent()
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    EmployeService EmpService = new EmployeService(UOW);
                    RoleService RoleService = new RoleService(UOW);

                    var agents = (from e in EmpService.GetAll()
                                  join r in RoleService.GetAll() on e.RoleId equals r.RoleId
                                  where e.IsVerified = true && r.role == "agent"
                                  select new
                                  {
                                      Emp = e

                                  }).Select(e => e.Emp).ToList();

                    return View(agents);

                }
            }



        }



        /*  public bool isReaffectable(int idAff, FormulaireService FormulaireService, AffectationService AffectationService, LotService LotService)
          {
              List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

              JoinedList = (from f in FormulaireService.GetAll()
                            join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                            join l in LotService.GetAll() on a.LotId equals l.LotId
                            where f.AffectationId == idAff

                            select new ClientAffecteViewModel
                            {

                                Formulaire = f,
                                Affectation = a,
                                Lot = l,

                            }).Where(j=>j.Formulaire.EtatClient == Note.A_VERIFIE || j.Formulaire.EtatClient == Note.SOLDE || j.Formulaire.EtatClient == Note.SOLDE_TRANCHE).ToList();


              if(JoinedList.Count == 0)
              {
                  return true;
              }
              else
              {
                  return false;
              }

          }*/


        public IEnumerable<SelectListItem> AgentListForDropDown(string idEmp,EmployeService EmpService)
        {

            List<Employe> agents = EmpService.GetMany(emp => emp.Role.role.Equals("agent") && emp.EmployeId+"" != idEmp  && emp.IsVerified == true).ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();


            agents.ForEach(l =>
            {
                listItems.Add(new SelectListItem { Text = l.Username, Value = l.EmployeId + "" });
            });

            return listItems;


        }

        public ActionResult reaffecterClients(string empId,string agent,string numLot,string affec, int? page, string currentFilterNumLot)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    DateTime dateGold = DateTime.Now.AddDays(-4).Date;
                    string usernameAgent = "";
                    ViewBag.dateGold = dateGold.ToString("dd/MM/yyyy");
                    
                    ViewBag.empId = empId;
                    if (numLot == null)
                        numLot = currentFilterNumLot;
                    ViewBag.numLot = numLot;

                    LotService LotService = new LotService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);
                    
                    if (empId != null)
                         usernameAgent  = EmpService.GetById(int.Parse(empId)).Username;
                    ViewBag.agent = usernameAgent;

                    ViewData["list"] = new SelectList(NumLotListForDropDownForReaffecter(LotService), "Value", "Text");
                    ViewBag.AgentList = new SelectList(AgentListForDropDown(empId, EmpService), "Value", "Text");

                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();
                    AffectationService AffectationService = new AffectationService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    
                    if (numLot != null)
                    {
                        //page = 1;
                        //ViewBag.page = page;
                    }
                    else
                    {
                        numLot = currentFilterNumLot;
                    }

                    ViewBag.currentFilterNumLot = numLot;

                    JoinedList = (from e in EmpService.GetAll()
                                  join a in AffectationService.GetAll() on e.EmployeId equals a.EmployeId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId
                                  where a.DateAffectation<=dateGold && l.NumLot.Equals(numLot) && !a.Formulaires.Any(f => f.EtatClient == Note.A_VERIFIE || f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE) && e.EmployeId+"" == empId

                                  select new ClientAffecteViewModel
                                  {
                                      Agent = e.Username,
                                      AffectationId = a.AffectationId,
                                      Affectation = a,
                                      Lot = l,

                                  }).ToList();

                    ViewBag.total = JoinedList.Count();

                    if (affec == "1")
                    {
                    
                        //Debug.WriteLine("Affecter nouveau agent");
                        //Debug.WriteLine(JoinedList.Count());

                        if(agent!=null)
                        {
                            foreach (ClientAffecteViewModel cavm in JoinedList)
                            {

                                cavm.Affectation.AncienAgent = usernameAgent;
                                cavm.Affectation.EmployeId = int.Parse(agent);
                                cavm.Affectation.DateAffectation = DateTime.Now;
                                AffectationService.Update(cavm.Affectation);

                            }
                            AffectationService.Commit();

                            JoinedList = (from e in EmpService.GetAll()
                                          join a in AffectationService.GetAll() on e.EmployeId equals a.EmployeId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          where a.DateAffectation <= dateGold && l.NumLot.Equals(numLot) && !a.Formulaires.Any(f => f.EtatClient == Note.A_VERIFIE || f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE) && e.EmployeId + "" == empId

                                          select new ClientAffecteViewModel
                                          {
                                              Agent = e.Username,
                                              AffectationId = a.AffectationId,
                                              Affectation = a,
                                              Lot = l,

                                          }).ToList();

                            ViewBag.total = JoinedList.Count();


                        }
                        
                    }

                    int pageSize = 10;
                    int pageNumber = (page ?? 1);

                    return View(JoinedList.ToPagedList(pageNumber, pageSize));
                }
            }

       }


        public ActionResult AffecterAgent(int numLot)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);
                    RoleService RoleService = new RoleService(UOW);

                    List<Lot> listClients = LotService.GetAll().Where(l => l.NumLot == numLot+"").ToList();

                    var listAffectations = (from a in AffectationService.GetAll()
                                            join l in LotService.GetAll() on a.LotId equals l.LotId
                                            where l.NumLot == numLot+""
                                            select new
                                            {
                                                Affectation = a,
                                                Lot = l
                                            }).Select(a => a.Affectation).ToList();

                    var agents  = (from e in EmpService.GetAll()
                                               join r in RoleService.GetAll() on e.RoleId equals r.RoleId
                                               where e.IsVerified = true && r.role == "agent"
                                               select new
                                               {
                                                   Emp = e

                                               }).Select(e => e.Emp).ToList();

                    ViewBag.AgentList = new SelectList(agents, "EmployeId", "Username");

                    int totalClientParLot = listClients.Count();

                    var clientsNonAffecteList = from lot in listClients
                                                where !(from aff in listAffectations
                                                        select aff.LotId).Contains(lot.LotId)
                                                select lot;

                    int nbClientsNonAffecteParLots = clientsNonAffecteList.Count();

                    int totalAffectationParLot = listAffectations.Count();

                    float pourcentageAffectationParLot = ((float)totalAffectationParLot / (float)totalClientParLot) * 100;

                    ViewData["numLot"] = numLot;

                    ViewData["totalClientsParLots"] = totalClientParLot;

                    ViewData["totalAffectationParLots"] = totalAffectationParLot;

                    ViewData["totalClientsRestantParLots"] = nbClientsNonAffecteParLots;

                    ViewData["pourcentageAffectationParLot"] = (int)pourcentageAffectationParLot;

                    return View();

                }
            }



                    
        }
    
        
        public void updateViewForAgent(int numLot,string agent)
        {


        }

        [HttpPost]
        public ActionResult AffecterAgents(int numLot, int nbClient,int agent)
        {


            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);

                    List<Lot> listClients = LotService.GetAll().Where(l => l.NumLot == numLot + "").ToList();

                    var listAffectations = (from a in AffectationService.GetAll()
                                            join l in LotService.GetAll() on a.LotId equals l.LotId
                                            where l.NumLot == numLot + ""
                                            select new
                                            {
                                                Affectation = a,
                                                Lot = l
                                            }).Select(a => a.Affectation).ToList();

                    var clientsNonAffecteList = from lot in listClients
                                                where !(from aff in listAffectations
                                                        select aff.LotId).Contains(lot.LotId)
                                                select lot;

                    int nbClientsNonAffecteParLots = clientsNonAffecteList.Count();

                    for (int i = 0; i < nbClient; i++)
                    {

                        AffectationService.Add(new Affectation { EmployeId = agent, AffectePar = "", DateAffectation = DateTime.Now, LotId = clientsNonAffecteList.ToList()[i].LotId });

                    }

                    AffectationService.Commit();

                    var result = UpdateView(numLot);

                    return result;

                }
            }

            
           }

        public ActionResult UpdateView(int numLot)
        {


            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
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
            }



        
        }
        public IEnumerable<SelectListItem> SortOrderSuiviClientForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde supérieur à", Value = "7" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde inférieur à", Value = "8" });

            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });

            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. decroissant)", Value = "3" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. croissant)", Value = "4" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. decroissant)", Value = "5" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. croissant)", Value = "6" });
            
            
            return listItems;
        }


        public ActionResult AffectationList(string numLot, string SearchString, string traite,  string currentFilter,string currentFilterNumLot,string currentFilterTraite,string CurrentSort,string currentSoldeFilter, string sortOrder,string soldeFilter, int? page)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(TraiteListForDropDown(), "Value", "Text");
                    ViewData["sortOrder"] = new SelectList(SortOrderSuiviClientForDropDown(), "Value", "Text");

                    if (sortOrder != null)
                    {
                        ///page = 1;
                    }
                    else
                    {
                        sortOrder = CurrentSort;
                    }

                    ViewBag.CurrentSort = sortOrder;


                    if (SearchString != null)
                    {
                        // page = 1;
                    }
                    else
                    {
                        SearchString = currentFilter;
                    }

                    ViewBag.CurrentFilter = SearchString;


                    if (numLot != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        numLot = currentFilterNumLot;
                    }

                    ViewBag.currentFilterNumLot = numLot;

                    if (traite != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        traite = currentFilterTraite;
                    }

                    ViewBag.currentFilterTraite = traite;

                    if (soldeFilter != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        soldeFilter = currentSoldeFilter;
                    }

                    ViewBag.currentSoldeFilter = soldeFilter;
      
                    ViewBag.page = page;
                    // page = 8 & CurrentSort = 5 & currentFilterNumLot = 6 & currentFilterTraite = SAUF

                    Employe emp = EmpService.GetEmployeByUername(Session["username"] + "");

                    if (!String.IsNullOrEmpty(traite))
                    {
                        if (traite == "ALL")
                        {

                            JoinedList = (from a in AffectationService.GetMany(a => a.EmployeId == emp.EmployeId)
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          select new ClientAffecteViewModel
                                          {
                                              Formulaire = FormulaireService.GetMany(f => f.AffectationId == a.AffectationId).OrderByDescending(f => f.TraiteLe).FirstOrDefault(),
                                              //Formulaire = a.Formulaires.OrderByDescending(o => o.TraiteLe).FirstOrDefault(),
                                              Affectation = a,
                                              Lot = l,

                                          }).DistinctBy(a => a.Affectation.AffectationId).ToList();

                        }
                        else if (traite == "SAUF")
                        {

                            JoinedList = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                          join a in AffectationService.GetMany(a => a.EmployeId == emp.EmployeId) on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId

                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,

                                          }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient + "" != "SOLDE" && f.Formulaire.EtatClient + "" != "FAUX_NUM").ToList();

                        
                        }else if(traite== "VERS")
                        {

                            JoinedList = (from f in FormulaireService.GetAll()
                                          join a in AffectationService.GetMany(a => a.EmployeId == emp.EmployeId) on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                          
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,

                                          }).Where(f => f.Formulaire.EtatClient == Note.SOLDE || f.Formulaire.EtatClient == Note.SOLDE_TRANCHE || f.Formulaire.EtatClient == Note.A_VERIFIE).OrderByDescending(f => f.Formulaire.TraiteLe).ToList();


                        }
                        else
                        {

                            JoinedList = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                          join a in AffectationService.GetMany(a => a.EmployeId == emp.EmployeId) on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId

                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,

                                          }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient + "" == traite).ToList();

                        }

                    }
                    else
                    {
                        JoinedList = (from a in AffectationService.GetMany(a => a.EmployeId == emp.EmployeId)
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      select new ClientAffecteViewModel
                                      {
                                          Formulaire = FormulaireService.GetMany(f => f.AffectationId == a.AffectationId).OrderByDescending(f => f.TraiteLe).FirstOrDefault(),

                                          Affectation = a,
                                          Lot = l,

                                      }).DistinctBy(a => a.Affectation.AffectationId).ToList();
                    }


                    if (!String.IsNullOrEmpty(numLot))
                    {
                        if (numLot.Equals("0") == false)
                            JoinedList = JoinedList.Where(j => j.Lot.NumLot.Equals(numLot)).ToList();

                    }

                    if (!String.IsNullOrEmpty(SearchString))
                    {
                        JoinedList = JoinedList.Where(s => s.Lot.Adresse.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Lot.Compte.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Lot.DescIndustry.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Lot.IDClient.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Lot.NomClient.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Lot.Numero.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Lot.SoldeDebiteur.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Lot.TelFixe.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())
                                               || s.Lot.TelPortable.IfNullOrWhiteSpace("").ToLower().Contains(SearchString.ToLower())

                                               ).ToList();
                    }


                    switch (sortOrder)
                    {
                        case "0":
                            JoinedList = JoinedList.OrderBy(s => s.Lot.NomClient).ToList();
                            break;
                        case "1":
                            try
                            {
                                JoinedList = JoinedList.OrderByDescending(s => double.Parse(s.Lot.SoldeDebiteur)).ToList();

                            }
                            catch (Exception)
                            {

                            }
                            break;

                        case "2":

                            try
                            {
                                
                                JoinedList = JoinedList.OrderBy(s => double.Parse(s.Lot.SoldeDebiteur)).ToList();

                            }
                            catch (Exception)
                            {

                            }

                            break;
                        case "3":
                            JoinedList = JoinedList.OrderByDescending(s => s.Affectation.DateAffectation).ToList();
                            break;
                        case "4":
                            JoinedList = JoinedList.OrderBy(s => s.Affectation.DateAffectation).ToList();
                            break;
                        case "5":
                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderByDescending(s => s.Formulaire.TraiteLe).ToList();
                            break;
                        case "6":
                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderBy(s => s.Formulaire.TraiteLe).ToList();
                            break;

                        case "7":


                            if (soldeFilter != null)
                            {
                                if (soldeFilter != "")
                                {
                                    if (double.TryParse(soldeFilter, out double filter))
                                    {
                                        try
                                        {
                                            JoinedList = JoinedList.Where(j => double.TryParse(j.Lot.SoldeDebiteur, out double soldedeb) && soldedeb > filter).OrderBy(j=> double.Parse(j.Lot.SoldeDebiteur)).ToList();
                                            
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine(e.Message);
                                        }

                                    }
                                }
                            }

                            break;
                        
                        case "8":

                            if (soldeFilter != null)
                            {
                                if (soldeFilter != "")
                                {
                                    if (double.TryParse(soldeFilter, out double filter))
                                    {

                                        try
                                        {
                                            JoinedList = JoinedList.Where(j => double.TryParse(j.Lot.SoldeDebiteur, out double soldedeb) && soldedeb < filter).OrderByDescending(j=> double.Parse(j.Lot.SoldeDebiteur)).ToList();

                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine(e.Message);

                                        }
                                    }
                                }
                            }
                            //JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderBy(s => s.Formulaire.TraiteLe).ToList();

                            break;

                        default:


                            break;
                    }


                    ViewBag.total = JoinedList.Count();
                    int pageSize = 10;
                    int pageNumber = (page ?? 1);

                    return View(JoinedList.ToPagedList(pageNumber, pageSize));

                }
            }



               

            
        }

        public IEnumerable<SelectListItem> TraiteListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les clients affectés", Value = "ALL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les clients traités sauf SOLDE/FAUX_NUM", Value = "SAUF" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous mes traitements (VERSEMENT/VERIFIE)", Value = "VERS" });

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
            listItems.Add(new SelectListItem { Selected = true, Text = "RAPPEL", Value = "RAPPEL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "RACCROCHE", Value = "RACCROCHE" });
            listItems.Add(new SelectListItem { Selected = true, Text = "NRP", Value = "NRP" });
            listItems.Add(new SelectListItem { Selected = true, Text = "INJOIGNABLE", Value = "INJOIGNABLE" });

            return listItems;
        }

        public IEnumerable<SelectListItem> NumLotListForDropDown(LotService LotService)
        {
            
            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            List<Lot> Lots = LotService.GetAll().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les lots", Value = "0" });

            Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
            });

            return listItems;
        }

        public IEnumerable<SelectListItem> NumLotListForDropDownForReaffecter(LotService LotService)
        {

            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            List<Lot> Lots = LotService.GetAll().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();


            Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
            });

            return listItems;
        }

        [HttpPost]
        public ActionResult LoadData(string numLot, string traite)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    AffectationService AffectationService = new AffectationService(UOW);
                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);

                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
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

                                      }).OrderByDescending(o => o.Affectation.DateAffectation).DistinctBy(a => a.Affectation.AffectationId).Where(j => j.Affectation.Employe.Username.Equals(Session["username"])).ToList();

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

                                      }).OrderByDescending(o => o.Formulaire.TraiteLe).DistinctBy(d => d.Formulaire.AffectationId).Where(j => j.Affectation.Employe.Username.Equals(Session["username"])).ToList();

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

                                      }).OrderByDescending(o => o.Formulaire.TraiteLe).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), traite)).Where(j => j.Affectation.Employe.Username.Equals(Session["username"])).ToList();

                    }


                    if (numLot != "0")
                    {
                        JoinedList = JoinedList.Where(l => l.Lot.NumLot.Equals(numLot)).ToList();
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
            }

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


        /*public ActionResult ModifierAffectation(int numLot)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    EmployeService EmpService = new EmployeService(UOW);
                    RoleService RoleService = new RoleService(UOW);
                  
                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    //var agents = EmpService.GetAll().Where(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true);
                    var agents = (from e in EmpService.GetAll()
                                  join r in RoleService.GetAll() on e.RoleId equals r.RoleId
                                  where e.IsVerified = true 
                                  select new { 
                                    Emp = e    

                                  }).Select(e=>e.Emp).ToList();


                    ViewBag.AgentList = new SelectList(agents, "EmployeId", "Username");
                    ViewData["numLot"] = numLot;
                    ViewBag.TraiteList = new SelectList(TraiteListModifierAffForDropDown(), "Value", "Text");

                    return View();

                }
            }
 
        }*/

        [HttpPost]
        public ActionResult GetInfoAgent(string agentDe,string numLot,string traite)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    AffectationService AffectationService = new AffectationService(UOW);
                    LotService LotService = new LotService(UOW);


                    int nbTotalAffectation = 0;
                    int nbTotLots = 0;
                    float pourcentageAff = 0;
                    int pourcentageAgentDe = 0;



                    if (traite.Equals("NON_TRAITES"))
                    {
                        nbTotalAffectation = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Count() == 0).Count();
                        nbTotLots = LotService.GetAll().Where(l => l.NumLot.Equals(numLot)).Count();
                        pourcentageAff = ((float)nbTotalAffectation / (float)nbTotLots) * 100;
                        pourcentageAgentDe = (int)pourcentageAff;

                        return Json(new { nbTotalAffectation = nbTotalAffectation, pourcentageAgentDe = pourcentageAgentDe, nbTotLots = nbTotLots });

                    }
                    else if (traite.Equals("RDV"))
                    {

                        nbTotalAffectation = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "RDV") && a.Formulaires.Last().DateRDV.Date == DateTime.Today.Date).ToList().Count();
                        nbTotLots = LotService.GetAll().Where(l => l.NumLot.Equals(numLot)).Count();
                        pourcentageAff = ((float)nbTotalAffectation / (float)nbTotLots) * 100;
                        pourcentageAgentDe = (int)pourcentageAff;

                        return Json(new { nbTotalAffectation = nbTotalAffectation, pourcentageAgentDe = pourcentageAgentDe, nbTotLots = nbTotLots });

                    }
                    /*else if (traite.Equals("RDV_REPORTE"))
                    {
                        nbTotalAffectation = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "RDV_REPORTE") && a.Formulaires.Last().DateRDVReporte.Date == DateTime.Today.Date).ToList().Count();
                        nbTotLots = LotService.GetAll().Where(l => l.NumLot.Equals(numLot)).Count();
                        pourcentageAff = ((float)nbTotalAffectation / (float)nbTotLots) * 100;
                        pourcentageAgentDe = (int)pourcentageAff;

                        return Json(new { nbTotalAffectation = nbTotalAffectation, pourcentageAgentDe = pourcentageAgentDe, nbTotLots = nbTotLots });

                    }*/
                    else
                    {
                        nbTotalAffectation = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), traite)).Count();
                        nbTotLots = LotService.GetAll().Where(l => l.NumLot.Equals(numLot)).Count();
                        pourcentageAff = ((float)nbTotalAffectation / (float)nbTotLots) * 100;
                        pourcentageAgentDe = (int)pourcentageAff;

                        return Json(new { nbTotalAffectation = nbTotalAffectation, pourcentageAgentDe = pourcentageAgentDe, nbTotLots = nbTotLots });

                    }
                }
            }

        }

        [HttpPost]
        public ActionResult Reaffecter(string agentA,string agentDe, string numLot, string traite,int num)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    EmployeService EmpService = new EmployeService(UOW);

                    AffectationService AffectationService = new AffectationService(UOW);
                    LotService LotService = new LotService(UOW);

                    List<Affectation> affectations = new List<Affectation>();
                    if (traite.Equals("NON_TRAITES"))
                    {

                        affectations = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).ToList();

                    }
                    else if (traite.Equals("RDV"))
                    {

                        affectations = AffectationService.GetAll().Where(a => a.Lot.NumLot.Equals(numLot) && a.EmployeId == int.Parse(agentDe)).Where(a => a.Formulaires.Any()).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "RDV")).Where(a => a.Formulaires.Last().DateRDV.Date == DateTime.Today.Date).ToList();

                    }
                    else if (traite.Equals("RDV_REPORTE"))
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
    }

}

