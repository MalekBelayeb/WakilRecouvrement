using ClosedXML.Excel;
using Microsoft.Ajax.Utilities;
using MyFinance.Data.Infrastructure;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Web.Models.ViewModel;
using Excel = Microsoft.Office.Interop.Excel;

namespace WakilRecouvrement.Web.Controllers
{
    public class FormulaireController : Controller
    {


        public int id = 0;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            log.Error(filterContext.Exception);
        }

        public FormulaireController()
        {

        }

        public ActionResult CreerFormulaire(string id, string msgError,string pageSave,string currentSort,string currentFilterNumLot,string currentFilterTraite)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);


                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");


                    ViewBag.page = pageSave;
                    ViewBag.CurrentSort = currentSort;
                    ViewBag.currentFilterNumLot = currentFilterNumLot;
                    ViewBag.currentFilterTraite = currentFilterTraite;

                    ViewBag.TraiteList = new SelectList(TraiteListForDropDownForCreation(), "Value", "Text");
                    ViewBag.id = id;
                    ViewBag.affectation = AffectationService.GetById(long.Parse(id));
                    ViewBag.errormsg = msgError;

                    ViewBag.TelFN = (from a in AffectationService.GetAll()
                                     join l in LotService.GetAll() on a.LotId equals l.LotId
                                     where a.AffectationId == long.Parse(id)
                                     select new TelFN
                                     {
                                         TelPortableFN = l.TelPortableFN,
                                         TelFixeFN = l.TelFixeFN,
                                         TelPortable = l.TelPortable,
                                         TelFixe = l.TelFixe

                                     }).FirstOrDefault();

                    string soldeDeb = (from a in AffectationService.GetAll()
                                       join l in LotService.GetAll() on a.LotId equals l.LotId
                                       where a.AffectationId == long.Parse(id)
                                       select new
                                       {
                                           SoldeDeb = l.SoldeDebiteur

                                       }).FirstOrDefault().SoldeDeb;

                    ViewBag.soldeDeb = soldeDeb.IfNullOrWhiteSpace("0").Replace(',', '.');


                    return View(FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe).ToList().Where(f => f.AffectationId == int.Parse(id)));
                }
            }
               
        }




        public IEnumerable<SelectListItem> SortOrderSuiviClientForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. decroissant)", Value = "3" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. croissant)", Value = "4" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. decroissant)", Value = "5" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. croissant)", Value = "6" });
         
            return listItems;
        }

        public IEnumerable<SelectListItem> SortOrderSuiviRDVForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. decroissant)", Value = "3" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. croissant)", Value = "4" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date RDV (o. decroissant)", Value = "5" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date RDV (o. croissant)", Value = "6" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date Traitement (o. decroissant)", Value = "7" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date Traitement (o. croissant)", Value = "8" });


            return listItems;
        }
        public IEnumerable<SelectListItem> SortOrderRentabiliteForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. decroissant)", Value = "3" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. croissant)", Value = "4" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. decroissant)", Value = "5" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. croissant)", Value = "6" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Revenue (o. decroissant)", Value = "7" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Revenue (o. croissant)", Value = "8" });


            return listItems;
        }

        public IEnumerable<SelectListItem> SortOrderRentabiliteDateForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Annuel", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Mensuel", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Quotidienne", Value = "2" });



            return listItems;
        }


        public List<Formulaire> getFormulaires(int AffectationId)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);

                    List<Formulaire> formulaires = (from f in FormulaireService.GetAll()
                                                    join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                                    where a.AffectationId == AffectationId
                                                    select f).ToList();

                    return formulaires;

                }
            }

        }



       



        public ActionResult HistoriqueClient(int id)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);
                    
                    List<ClientAffecteViewModel> clientAffecteViewModels = (from f in FormulaireService.GetAll()
                                                                            join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                                                            join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId

                                                                            where a.AffectationId == id
                                                                            select new ClientAffecteViewModel
                                                                            {
                                                                                Formulaire = f,
                                                                                Affectation = a,
                                                                                Agent = e.Username

                                                                            }).ToList();
                    
                    ViewBag.username = clientAffecteViewModels.Select(c=>c.Agent).FirstOrDefault();
                    ViewBag.id = id+"";

                    return View(clientAffecteViewModels);

                }
            }
  
        }

        public ActionResult SuiviClient(string numLot,string SearchString,string traite,string agent,string currentFilterAgent, string currentFilter, string sortOrder,string CurrentSort,string currentFilterNumLot,string currentFilterTraite, int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);
                    
                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    ViewBag.CurrentSort = sortOrder;

                    List<ClientAffecteViewModel> JoinedList;

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.AgentList = new SelectList(AgentListForDropDown(EmpService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(TraiteListForDropDown(), "Value", "Text");
                    ViewData["sortOrder"] = new SelectList(SortOrderSuiviClientForDropDown(), "Value", "Text");

                    if (sortOrder != null)
                    {
                        page = 1;
                    }
                    else
                    {
                        sortOrder = CurrentSort;
                    }


                    ViewBag.CurrentSort = sortOrder;

                    if (SearchString != null)
                    {
                        page = 1;
                    }
                    else
                    {
                        SearchString = currentFilter;
                    }

                    ViewBag.CurrentFilter = SearchString;

                    if (agent != null)
                    {
                        page = 1;
                    }
                    else
                    {
                        agent = currentFilterAgent;
                    }

                    ViewBag.currentFilterAgent = agent;


                    if (numLot != null)
                    {
                        page = 1;
                    }
                    else
                    {
                        numLot = currentFilterNumLot;
                    }

                    ViewBag.currentFilterNumLot = numLot;


                    if (traite != null)
                    {
                        page = 1;
                    }
                    else
                    {
                        traite = currentFilterTraite;
                    }

                    ViewBag.currentFilterTraite = traite;

                    if (!String.IsNullOrEmpty(traite))
                    {

                        if (traite == "ALL")
                        {

                            JoinedList = (from a in AffectationService.GetAll()
                                          join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId

                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = FormulaireService.GetMany(f => f.AffectationId == a.AffectationId).OrderByDescending(f => f.TraiteLe).FirstOrDefault(),
                                              // Formulaire = a.Formulaires.OrderByDescending(o => o.TraiteLe).FirstOrDefault(),
                                              Affectation = a,
                                              Lot = l,
                                              Agent = e.Username

                                          }).DistinctBy(a => a.Affectation.AffectationId).ToList();


                        }
                        else if (traite == "SAUF")
                        {

                            JoinedList = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = e.Username

                                          }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient + "" != "SOLDE" && f.Formulaire.EtatClient + "" != "FAUX_NUM").ToList();
                        }
                        else
                        {
                            JoinedList = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = e.Username

                                          }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient + "" == traite).ToList();
                        }

                    }
                    else
                    {

                        JoinedList = (from a in AffectationService.GetAll()
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                      select new ClientAffecteViewModel
                                      {
                                          Formulaire = FormulaireService.GetMany(f => f.AffectationId == a.AffectationId).OrderByDescending(f => f.TraiteLe).FirstOrDefault(),
                                          //Formulaire = a.Formulaires.OrderByDescending(o => o.TraiteLe).FirstOrDefault(),
                                          Affectation = a,
                                          Lot = l,
                                          Agent = e.Username

                                      }).DistinctBy(a => a.Affectation.AffectationId).ToList();
                    }


                    if (!String.IsNullOrEmpty(agent))
                    {
                        if (int.Parse(agent) != 0)
                        {
                            JoinedList = JoinedList.Where(j => j.Affectation.EmployeId == int.Parse(agent)).ToList();
                        }

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
                                JoinedList = JoinedList.OrderByDescending(s => s.Lot.SoldeDebiteur).ToList();

                            }
                            catch (Exception)
                            {

                            }
                            break;

                        case "2":

                            try
                            {
                                JoinedList = JoinedList.OrderBy(s => s.Lot.SoldeDebiteur).ToList();

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

        public IEnumerable<SelectListItem> NumLotListForDropDown(LotService LotService)
        {
                    
                    List<Lot> Lots = LotService.GetAll().DistinctBy(l => l.NumLot).ToList();
                    List<SelectListItem> listItems = new List<SelectListItem>();

                    listItems.Add(new SelectListItem { Selected = true, Text = "Tous les lots", Value = "0" });

                    Lots.ForEach(l =>
                    {
                        listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
                    });

                    return listItems;
        }


        public IEnumerable<SelectListItem> RDVForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "RDV du jour", Value = "RDV_J" });
            listItems.Add(new SelectListItem { Text = "Mes RDV pour demain", Value = "RDV_DEMAIN" });
            listItems.Add(new SelectListItem { Text = "Mes RDV pour les prochains jours", Value = "RDV_JOURS_PROCHAINE" });
            listItems.Add(new SelectListItem { Text = "Mes RDV pour la semaine prochaine", Value = "RDV_SEMAINE_PROCHAINE" });
            listItems.Add(new SelectListItem { Text = "Tous mes RDV du:", Value = "RDVDate" });
            listItems.Add(new SelectListItem { Text = "Tous mes RDV", Value = "ALL" });


            return listItems;
        }


        public IEnumerable<SelectListItem> AgentListForDropDown(EmployeService EmpService)
        {
                               
                    List<Employe> agents = EmpService.GetMany(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true).ToList();
                    List<SelectListItem> listItems = new List<SelectListItem>();

                    listItems.Add(new SelectListItem { Selected = true, Text = "Tous les agents", Value = "0" });

                    agents.ForEach(l =>
                    {
                        listItems.Add(new SelectListItem { Text = l.Username, Value = l.EmployeId + "" });
                    });

                    return listItems;

                
        }
        public IEnumerable<SelectListItem> TraiteListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les clients affectés", Value = "ALL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous clients traités sauf SOLDE/FAUX_NUM", Value = "SAUF" });

            foreach (var n in Enum.GetValues(typeof(Note)))
            {

                listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString() });

            }


            return listItems;
        }
        public IEnumerable<SelectListItem> TraiteListSuiviTraitForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les clients traités", Value = "ALL" });

            foreach (var n in Enum.GetValues(typeof(Note)))
            {

                listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString() });

            }


            return listItems;
        }

        public IEnumerable<SelectListItem> TraiteListSuiviTraitHistoriqueForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "SOLDE ET TRANCHE", Value = "ALL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "SOLDE", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Selected = true, Text = "TRANCHE", Value = "SOLDE_TRANCHE" });

            return listItems;
        }

        public IEnumerable<SelectListItem>typeListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les traitements", Value = "ALL_TRAIT" });
            listItems.Add(new SelectListItem { Text = "Traitements par date", Value = "DATE_TRAIT" });

            return listItems;
        }
        public IEnumerable<SelectListItem> TraiteListRentabiliteForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Traitement Soldé et tranche", Value = "ALL" });
            listItems.Add(new SelectListItem { Text = "Soldé", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "Tranche", Value = "SOLDE_TRANCHE" });


            return listItems;
        }


        public IEnumerable<SelectListItem> TraiteListForDropDownForCreation()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();

            foreach (var n in Enum.GetValues(typeof(Note)))
            {

                listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString() });

            }


            return listItems;
        }

        [HttpPost]
        public ActionResult CreerFormulaireNote(string id, string DescriptionAutre, string EtatClient, string RDVDateTime, string RappelDateTime, string soldetranche, HttpPostedFileBase[] PostedFile,string pageSave,string CurrentSortSave, string currentFilterNumLotSave,string currentFilterTraiteSave)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);


                    ViewBag.TraiteList = new SelectList(TraiteListForDropDownForCreation(), "Value", "Text");

                    Formulaire Formulaire = new Formulaire();
                    ViewBag.errormsg = "";
                    
                    switch ((Note)Enum.Parse(typeof(Note), EtatClient))
                    {
                        case Note.INJOIGNABLE:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;

                            Formulaire.EtatClient = Note.INJOIGNABLE;

                            break;
                        case Note.NRP:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;

                            Formulaire.EtatClient = Note.NRP;

                            break;
                        case Note.RACCROCHE:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;

                            Formulaire.EtatClient = Note.RACCROCHE;

                            break;
                        case Note.RDV:

                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.EtatClient = Note.RDV;
                            Formulaire.DateRDV = DateTime.Parse(RDVDateTime);
                            Formulaire.Status = Status.VERIFIE;

                            break;

                        case Note.REFUS_PAIEMENT:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;

                            Formulaire.EtatClient = Note.REFUS_PAIEMENT;

                            break;
                        case Note.SOLDE:

                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.EtatClient = Note.SOLDE;
                            Formulaire.Status = Status.EN_COURS;

                            if (soldetranche.IndexOf('.') != -1)
                            {
                                soldetranche = soldetranche.Replace('.', ',');
                            }

                            Formulaire.MontantVerseDeclare = double.Parse(soldetranche.IfNullOrWhiteSpace(Formulaire.MontantDebInitial+""));

                            break;
                        case Note.FAUX_NUM:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;

                            Formulaire.EtatClient = Note.FAUX_NUM;

                            break;
                        case Note.A_VERIFIE:

                            Formulaire.AffectationId = int.Parse(id);

                            Formulaire.TraiteLe = DateTime.Now;

                            Formulaire.EtatClient = Note.A_VERIFIE;

                            Formulaire.ContacteBanque = false;

                            Formulaire.Status = Status.EN_COURS;

                            break;
                        case Note.AUTRE:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.EtatClient = Note.AUTRE;
                            Formulaire.TraiteLe = DateTime.Now;
                            break;
                        case Note.RAPPEL:

                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;
                            Formulaire.RappelLe = DateTime.Parse(RappelDateTime);
                            Formulaire.EtatClient = Note.RAPPEL;

                            break;
                        case Note.SOLDE_TRANCHE:

                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.EtatClient = Note.SOLDE_TRANCHE;
                            Formulaire.Status = Status.EN_COURS;

                            if (soldetranche.IndexOf('.') != -1)
                            {
                                soldetranche = soldetranche.Replace('.', ',');
                            }

                            
                            Formulaire.MontantVerseDeclare = double.Parse(soldetranche.IfNullOrWhiteSpace("0"));

                            break;
                    }

                    Formulaire.DescriptionAutre = DescriptionAutre;
                    Formulaire.NotifieBanque = false;

                    var nbVerfie = (from f in FormulaireService.GetAll()
                                    join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                    where a.AffectationId == int.Parse(id) && f.Status == Status.EN_COURS && f.EtatClient == Note.A_VERIFIE
                                    select new ClientAffecteViewModel
                                    {
                                        Formulaire = f,
                                        Affectation = a
                                    }).Count();

                    var nbSolde = (from f in FormulaireService.GetAll()
                                    join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                    where a.AffectationId == int.Parse(id) && f.EtatClient == Note.SOLDE
                                    select new ClientAffecteViewModel
                                    {
                                        Formulaire = f,
                                        Affectation = a
                                    }).Count();



                    if(nbSolde>=1)
                    {
                        return RedirectToAction("CreerFormulaire", new { id = id, msgError = "Client est deja soldé !" });
                    }

                    if (Formulaire.EtatClient == Note.A_VERIFIE)
                    {
                        if (nbVerfie >= 1)
                        {
                            return RedirectToAction("CreerFormulaire", new { id = id, msgError = "Une verification est deja en attente !" });
                        }
                    }

                    FormulaireService.Add(Formulaire);
                    FormulaireService.Commit();

                    Lot Joinedlot = (from f in FormulaireService.GetAll()
                                     join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                     join l in LotService.GetAll() on a.LotId equals l.LotId
                                     where f.FormulaireId == Formulaire.FormulaireId
                                     select new Lot
                                     {

                                         Compte = l.Compte,
                                         IDClient = l.IDClient,
                                         NumLot = l.NumLot,
                                         SoldeDebiteur = l.SoldeDebiteur

                                     }).FirstOrDefault();


                    if (Joinedlot.SoldeDebiteur == "" || Joinedlot.SoldeDebiteur == null)
                    {
                        Joinedlot.SoldeDebiteur = "0";
                    }

                    if (Joinedlot.SoldeDebiteur.IndexOf('.') != -1)
                    {
                        Joinedlot.SoldeDebiteur = Joinedlot.SoldeDebiteur.Replace('.', ',');
                    }

                    Formulaire.MontantDebInitial = double.Parse(Joinedlot.SoldeDebiteur);

                    if (FormulaireService.GetAll().Where(f => f.AffectationId == int.Parse(id)).Count() == 1)
                    {

                        Formulaire.MontantDebMAJ = double.Parse(Joinedlot.SoldeDebiteur);

                    }
                    else
                    {

                        ClientAffecteViewModel cavm = (from f in FormulaireService.GetAll()
                                                       join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                                       where a.AffectationId == int.Parse(id) && f.MontantDebMAJ != 0
                                                       orderby f.MontantDebMAJ ascending
                                                       select new ClientAffecteViewModel
                                                       {
                                                           Formulaire = f,
                                                           Affectation = a

                                                       }).FirstOrDefault();

                        if(cavm!=null)
                        {
                            Formulaire.MontantDebMAJ = cavm.Formulaire.MontantDebMAJ;
                        }
                        else
                        {
                            Formulaire.MontantDebMAJ = 0;
                        }

                    }

                    FormulaireService.Update(Formulaire);
                    FormulaireService.Commit();

                    if (PostedFile != null)
                    {
                        if (PostedFile.Length > 0)
                        {

                            foreach (HttpPostedFileBase postedFile in PostedFile)
                            {
                                if (postedFile == null)
                                    return RedirectToAction("AffectationList", "Affectation", new { traite = currentFilterTraiteSave, numLot = currentFilterNumLotSave, sortOrder = CurrentSortSave, page = pageSave });
                            }

                            string filePath = string.Empty;
                            string path = Server.MapPath("~/Uploads/Recu/");

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            string recuPath = path + Formulaire.FormulaireId;

                            if (!Directory.Exists(recuPath))
                            {
                                Directory.CreateDirectory(recuPath);
                            }

                            foreach (HttpPostedFileBase postedFile in PostedFile)
                            {
                                string filename = Directory.GetFiles(recuPath).Length + 1 + "_" + Joinedlot.IDClient + "_" + Joinedlot.Compte;
                                filePath = recuPath + "/" + filename + Path.GetExtension(postedFile.FileName);
                                postedFile.SaveAs(filePath);
                            }
                        }
                    }
                    return RedirectToAction("AffectationList", "Affectation", new { traite = currentFilterTraiteSave, numLot = currentFilterNumLotSave, sortOrder = CurrentSortSave, page = pageSave });

                }
            }
        }

        public ActionResult deleteHist(int idHist,int id,string msgError)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    FormulaireService FormulaireService = new FormulaireService(UOW);
                  
                    FormulaireService.Delete(FormulaireService.GetById(idHist));
                    FormulaireService.Commit();

                    return RedirectToAction("CreerFormulaire", "Formulaire", new { id = id, msgError = msgError });

                }
            }
        }


        public ActionResult deleteHistVerifie(int id,string currentFilter ,string currentNumLot ,string currentType ,string currentTraite ,string currentAgent ,string currentTraitDate,string currentPage )
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    
                    FormulaireService.Delete(FormulaireService.GetById(id));
                    FormulaireService.Commit();

                    return RedirectToAction("HistoriqueTraitements", "Formulaire", new {currentFilter, currentNumLot ,currentType , currentTraite ,currentAgent , currentTraitDate, currentPage });

                }
            }
        }

        [HttpPost]
        public ActionResult GetFormulaires(int id)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    FormulaireService FormulaireService = new FormulaireService(UOW);
                 

                    var joinedAffectation = FormulaireService.GetOrderedFormulaireByAffectationList(id).Select(j => new ClientAffecteViewModel { Formulaire = j });

                    var list = joinedAffectation.Select(f => new
                    {
                        etat = f.Formulaire.EtatClient.ToString(),
                        d1 = f.Formulaire.DateRDV.ToString(),
                        tranche = f.Formulaire.MontantVerseDeclare.ToString(),
                        desc = f.Formulaire.DescriptionAutre,
                        verif = f.Formulaire.Status.ToString(),
                        traitele = f.Formulaire.TraiteLe.ToString(),
                        montantDebInitial = f.Formulaire.MontantDebInitial.ToString(),
                        montantDebMaj = f.Formulaire.MontantDebMAJ.ToString()
                    });

                    return Json(new { list = list });
                }
            }
        }

        public string GetEtat(Formulaire formulaire)
        {


            if (formulaire == null)
            {
                return "";
            }
            else
            {

                return formulaire.IfNotNull(i => i.EtatClient).ToString();

            }

        }
        public string GetTraiteLe(Affectation affectation)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    FormulaireService FormulaireService = new FormulaireService(UOW);
                
                    List<Formulaire> formulaires = FormulaireService.GetAll().Where(f => f.AffectationId == affectation.AffectationId).ToList();


                    if (formulaires.Count() == 0)
                    {
                        return "";
                    }
                    else
                    {

                        return formulaires.OrderByDescending(f => f.TraiteLe).FirstOrDefault().TraiteLe.ToString();
                    }

                }
            }

        }

        public ActionResult ValiderTraitement()
        {


            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    string path = Server.MapPath("~/Uploads/Recu/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    LotService LotService = new LotService(UOW);
                    EmployeService EmployeService = new EmployeService(UOW);

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
                    ViewBag.AgentList = new SelectList(AgentListForDropDown(EmployeService), "Value", "Text");

                    if (TempData["IDClient"] == null)
                    {
                        ViewBag.IDClient = "0";
                    }
                    else
                    {
                        ViewBag.IDClient = TempData["IDClient"];
                    }

                    return View();

                }
            }


        }



        public string getUsernameAgent(int idAff)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmployeService = new EmployeService(UOW);

                    string username = (from a in AffectationService.GetAll()
                                      join e in EmployeService.GetAll() on a.EmployeId equals e.EmployeId
                                      where a.AffectationId == idAff
                                       select new {
                                          e.Username
                                      }).Select(e=>e.Username).FirstOrDefault();
                    
                    return username;

             } 
            
            }   
        }


        [HttpPost]
        public ActionResult ValiderTraitement(bool IsValid, string numLot, string traite, string agent)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);
                    string status = "";
                    if (IsValid == true)
                        status = "VERIFIE";
                    if (IsValid == false)
                        status = "EN_COURS";


                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");

                    if (IsValid == false)
                        ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
                    if (IsValid == true)
                        ViewBag.TraiteList = new SelectList(TraiteValidationValideListForDropDown(), "Value", "Text");

                    ViewBag.AgentList = new SelectList(AgentListForDropDown(EmpService), "Value", "Text");

                    if (traite.Equals("ALL"))
                    {
                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                      select new ClientAffecteViewModel
                                      {
                                          Formulaire = f,
                                          Lot = l,
                                          Affectation = a,
                                          Agent = e.Username

                                      }).OrderByDescending(j => j.Formulaire.TraiteLe).Where(j => j.Formulaire.Status == (Status)Enum.Parse(typeof(Status), status)).Where(j => j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE_TRANCHE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "A_VERIFIE")).ToList();

                    }
                    else
                    {
                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId

                                      select new ClientAffecteViewModel
                                      {
                                          Formulaire = f,
                                          Lot = l,
                                          Affectation = a,
                                          Agent = e.Username

                                      }).OrderByDescending(j => j.Formulaire.TraiteLe).Where(j => j.Formulaire.Status == (Status)Enum.Parse(typeof(Status), status)).Where(j => j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), traite)).ToList();

                    }

                    if (numLot != "0")
                    {
                        JoinedList = JoinedList.Where(j => j.Lot.NumLot.Equals(numLot)).ToList();

                    }

                    if (int.Parse(agent) != 0)
                    {

                        JoinedList = JoinedList.Where(j => j.Affectation.EmployeId == int.Parse(agent)).ToList();

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

                        if (order == "0" && orderDir == "asc")
                        {
                            order = "6";
                            orderDir = "DESC";
                        }
                        int totalRecords = JoinedList.Count();

                        if (!string.IsNullOrEmpty(search) &&
                            !string.IsNullOrWhiteSpace(search))
                        {
                            JoinedList = JoinedList.Where(j =>

                                j.Lot.Numero.IfNotNull(n=>n.ToString().ToLower().Contains(search.ToLower()))
                            || j.Lot.Adresse.IfNotNull(n => n.ToString().ToLower().Contains(search.ToLower()))
                            || j.Lot.IDClient.IfNotNull(n => n.ToString().ToLower().Contains(search.ToLower()))
                            || j.Lot.Compte.IfNotNull(n => n.ToString().ToLower().Contains(search.ToLower()))
                            || j.Lot.NomClient.IfNotNull(n => n.ToString().ToLower().Contains(search.ToLower()))
                            || j.Lot.DescIndustry.IfNotNull(n => n.ToString().ToLower().Contains(search.ToLower()))
                            || j.Formulaire.MontantVerseDeclare.IfNotNull(n => n.ToString().ToLower().Contains(search.ToLower()))

                                ).ToList();
                        }

                        if (IsValid == false)
                            JoinedList = SortTableDataForValidate(order, orderDir, JoinedList);
                        if (IsValid == true)
                            JoinedList = SortTableDataForHistory(order, orderDir, JoinedList);

                        int recFilter = JoinedList.Count();

                        JoinedList = JoinedList.Skip(startRec).Take(pageSize).ToList();

                        var modifiedData = JoinedList.Select(j =>
                           new
                           {

                               j.Lot.NumLot,
                               j.Formulaire.MontantVerseDeclare,
                               j.Lot.Compte,
                               j.Lot.IDClient,
                               j.Lot.NomClient,
                               j.Lot.SoldeDebiteur,
                               j.Lot.DescIndustry,
                               Username = j.Agent,
                               j.Affectation.AffectationId,
                               VerifieLe = j.Formulaire.VerifieLe.ToString(),
                               DateAff = j.Affectation.DateAffectation.ToString(),
                               TraiteLe = j.Formulaire.TraiteLe.ToString("dd/MM/yyyy HH:mm:ss"),
                               Etat = GetEtat(j.Formulaire).ToString(),
                               FormulaireId = j.Formulaire.FormulaireId,
                               ContactBanque = j.Formulaire.ContacteBanque,
                               Image = getImagePath(j.Formulaire),
                               descAutre = j.Formulaire.DescriptionAutre,
                               NBRecu = getImagePathNB(j.Formulaire)
                           }
                           );
                        int x = JoinedList.Count();

                        var info = new { nbTotal = totalRecords, draw = Convert.ToInt32(draw) };

                        result = this.Json(new
                        {

                            draw = Convert.ToInt32(draw),
                            recordsTotal = totalRecords,
                            recordsFiltered = recFilter,
                            data = modifiedData,
                            info = info

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

        public IEnumerable<SelectListItem> TraiteValidationListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les traitements non validés", Value = "ALL" });
            listItems.Add(new SelectListItem { Text = "Soldé", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "Tranche", Value = "SOLDE_TRANCHE" });
            listItems.Add(new SelectListItem { Text = "A verifié", Value = "A_VERIFIE" });

            return listItems;
        }
        public IEnumerable<SelectListItem> TraiteTypeForValiderListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Text = "Par date", Value = "P_DATE" });
            listItems.Add(new SelectListItem { Text = "Par interval de temps", Value = "P_INTERVAL" });
            listItems.Add(new SelectListItem { Text = "Tous les traitements", Value = "P_ALL" });

            return listItems;
        }

        public IEnumerable<SelectListItem> EnvoyerTraiteListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Text = "RDV", Value = "RDV" });
            listItems.Add(new SelectListItem { Text = "Versement", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "Verification", Value = "A_VERIFIE" });
            listItems.Add(new SelectListItem { Text = "En cours de traitement", Value = "Autre" });

            return listItems;
        }

        public IEnumerable<SelectListItem> TraiteValidationValideListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les traitements validés", Value = "ALL" });
            listItems.Add(new SelectListItem { Text = "Soldé", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "Tranche", Value = "SOLDE_TRANCHE" });
            listItems.Add(new SelectListItem { Text = "A verifié", Value = "A_VERIFIE" });

            return listItems;
        }

        private List<ClientAffecteViewModel> SortTableDataForValidate(string order, string orderDir, List<ClientAffecteViewModel> data)
        {
            List<ClientAffecteViewModel> lst = new List<ClientAffecteViewModel>();
            try
            {
                switch (order)
                {

                    case "0":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => long.Parse(j.Lot.NumLot)).ToList()
                                                                                                 : data.OrderBy(j => long.Parse(j.Lot.NumLot)).ToList();
                        break;
                    case "5":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Formulaire.MontantVerseDeclare).ToList()
                                                                                                 : data.OrderBy(j => j.Formulaire.MontantVerseDeclare).ToList();
                        break;

                    case "6":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Formulaire.TraiteLe).ToList()
                                                                                                 : data.OrderBy(j => j.Formulaire.TraiteLe).ToList();
                        break;
                     case "7":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Affectation.EmployeId).ToList()
                                                                                                 : data.OrderBy(j => j.Affectation.EmployeId).ToList();
                        break;

                    case "8":

                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList()
                                                                                              : data.OrderBy(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList();

                        break;
                    case "11":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.NomClient).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.NomClient).ToList();
                        break;

                    default:

                        lst = data.OrderByDescending(j => j.Formulaire.TraiteLe).ToList();

                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return lst;
        }


        private List<ClientAffecteViewModel> SortTableDataForHistory(string order, string orderDir, List<ClientAffecteViewModel> data)
        {
            List<ClientAffecteViewModel> lst = new List<ClientAffecteViewModel>();
            try
            {
                switch (order)
                {

                    case "0":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => long.Parse(j.Lot.NumLot)).ToList()
                                                                                                 : data.OrderBy(j => long.Parse(j.Lot.NumLot)).ToList();
                        break;
                    case "2":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Formulaire.MontantVerseDeclare).ToList()
                                                                                                 : data.OrderBy(j => j.Formulaire.MontantVerseDeclare).ToList();
                        break;
                    case "3":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Formulaire.VerifieLe).ToList()
                                                                                                 : data.OrderBy(j => j.Formulaire.VerifieLe).ToList();
                        break;
                    case "4":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => GetTraiteLe(j.Affectation)).ToList()
                                                                                                 : data.OrderBy(j => GetTraiteLe(j.Affectation)).ToList();
                        break;
                    case "5":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Affectation.DateAffectation).ToList()
                                                                                                 : data.OrderBy(j => j.Affectation.DateAffectation).ToList();
                        break;
                    case "6":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Affectation.EmployeId).ToList()
                                                                                                 : data.OrderBy(j => j.Affectation.EmployeId).ToList();
                        break;

                    case "7":

                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList()
                                                                                              : data.OrderBy(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList();
                        break;
                    case "10":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.NomClient).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.NomClient).ToList();
                        break;
                    case "11":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.DescIndustry).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.DescIndustry).ToList();
                        break;

                    default:
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Formulaire.VerifieLe).ToList()
                                                                                                   : data.OrderBy(j => j.Formulaire.VerifieLe).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return lst;
        }

        public bool IsDeletable(int idForm,List<Formulaire> formulaires)
        {
            Formulaire _formulaire = new Formulaire();
            _formulaire = formulaires.OrderByDescending(f => f.TraiteLe).FirstOrDefault();

            if(_formulaire.FormulaireId == idForm)
            {
                return true;
            }
            return false;
        }

        public ActionResult HistoriqueTraitements(string numLot, string currentNumLot, string SearchString, string currentFilter, string traite, string currentTraite, string agent, string currentAgent, string traitDate, string currentTraitDate, string type, string currentType,string currentPage, int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.AgentList = new SelectList(AgentListForDropDown(EmpService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(TraiteListSuiviTraitHistoriqueForDropDown(), "Value", "Text");
                    ViewBag.typeTrait = new SelectList(typeListForDropDown(), "Value", "Text");

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    List<ClientAffecteViewModel> JoinedList;

                    if(page==null)
                    {
                        if(currentPage!=null)
                        page = int.Parse(currentPage);
                    }

                    ViewBag.page= page;
                      
                    if (SearchString != null)
                    {
                        // page = 1;
                    }
                    else
                    {
                        SearchString = currentFilter;
                    }

                    ViewBag.currentFilter = SearchString;

                    if (numLot != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        numLot = currentNumLot;
                    }

                    ViewBag.currentNumLot = numLot;

                    if (type != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        type = currentType;
                    }

                    ViewBag.currentType = type;

                    if (traite != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        traite = currentTraite;
                    }

                    ViewBag.currentTraite = traite;

                    if (agent != null)
                    {
                        ///page = 1;
                    }
                    else
                    {
                        agent = currentAgent;
                    }

                    ViewBag.currentAgent = agent;

                    if (traitDate != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        traitDate = currentTraitDate;
                    }

                    ViewBag.currentTraitDate = traitDate;


                    if (!String.IsNullOrEmpty(traite))
                    {

                        if (traite == "ALL")
                        {

                            JoinedList = (from f in FormulaireService.GetAll()
                                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                          where (f.EtatClient == Note.SOLDE || f.EtatClient== Note.SOLDE_TRANCHE) && f.Status == Status.VERIFIE
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = e.Username,
                                              IsDeletable = IsDeletable(f.FormulaireId,FormulaireService.GetMany(f=>f.AffectationId == a.AffectationId && (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE)).ToList())

                                          }).OrderByDescending(f => f.Formulaire.VerifieLe).ToList();

                        }

                        else
                        {
                            JoinedList = (from f in FormulaireService.GetAll()
                                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                          where (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE) && f.Status == Status.VERIFIE
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = e.Username,
                                              IsDeletable = IsDeletable(f.FormulaireId, FormulaireService.GetMany(f => f.AffectationId == a.AffectationId && (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE)).ToList())

                                          }).Where(f => f.Formulaire.EtatClient + "" == traite).OrderByDescending(f => f.Formulaire.VerifieLe).ToList();
                        }

                    }
                    else
                    {

                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                      where (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE) && f.Status == Status.VERIFIE
                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Affectation = a,
                                          Lot = l,
                                          Agent = e.Username,
                                          IsDeletable = IsDeletable(f.FormulaireId, FormulaireService.GetMany(f => f.AffectationId == a.AffectationId && (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE)).ToList())

                                      }).OrderByDescending(f => f.Formulaire.VerifieLe).ToList();

                    }



                    if (type == "DATE_TRAIT")
                    {

                        if (!String.IsNullOrEmpty(traitDate))
                        {
                            DateTime d = DateTime.Parse(traitDate);
                            JoinedList = JoinedList.Where(j => j.Formulaire.TraiteLe.Date == d.Date).ToList();
                        }

                    }



                    if (!String.IsNullOrEmpty(agent))
                    {
                        if (int.Parse(agent) != 0)
                        {
                            JoinedList = JoinedList.Where(j => j.Affectation.EmployeId == int.Parse(agent)).ToList();
                        }

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

                    ViewBag.total = JoinedList.Count();

                    int pageSize = 10;
                    int pageNumber = (page ?? 1);

                    return View(JoinedList.ToPagedList(pageNumber, pageSize));
                }
            }

        }


        public Formulaire GetFormulaire(int affId)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    var forms = (from f in FormulaireService.GetAll()
                         join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                         where a.AffectationId == affId
                         orderby f.MontantDebMAJ ascending
                         select new ClientAffecteViewModel
                         {
                             Formulaire = f,
                             Affectation = a
                         }).FirstOrDefault();


                    return forms.Formulaire;

                }
            }

        }

        [HttpPost]
        public ActionResult VerifierEtat(int id, bool valid, string montant)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                   

                    var JoinedLot = from f in FormulaireService.GetAll()
                                    join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                    join l in LotService.GetAll() on a.LotId equals l.LotId
                                    where f.FormulaireId == id
                                    select new ClientAffecteViewModel { Lot = l, Formulaire = f };


                    Lot Lot = JoinedLot.Select(j=>j.Lot).FirstOrDefault();
                    Formulaire Formulaire = JoinedLot.Select(j=>j.Formulaire).FirstOrDefault();

                    double DebMaJ = GetFormulaire(Formulaire.AffectationId).MontantDebMAJ;

                    
                    if (valid == false)
                    {

                        Debug.WriteLine("qqqq");
                        DeleteFromulaire(Formulaire, FormulaireService);
                        return Json(new { });
                    }

                    double SoldeDebiteur = double.Parse(Lot.SoldeDebiteur);
                    Decimal NewSolde = 0;

                    switch (Formulaire.EtatClient)
                    {
                        case Note.SOLDE:

                            NewSolde = Decimal.Subtract(decimal.Parse(DebMaJ.ToString()), decimal.Parse(Formulaire.MontantVerseDeclare.ToString()));

                            if (NewSolde <= 0)
                            {

                                Formulaire.MontantDebMAJ = 0;
                                Formulaire.Status = Status.VERIFIE;
                                Formulaire.VerifieLe = DateTime.Now;

                            }
                            break;

                        case Note.SOLDE_TRANCHE:

                            NewSolde = Decimal.Subtract(decimal.Parse(DebMaJ.ToString()), decimal.Parse(Formulaire.MontantVerseDeclare.ToString()));

                            if (NewSolde > 0)
                            {

                                Formulaire.MontantDebMAJ = double.Parse(NewSolde.ToString());

                                Formulaire.Status = Status.VERIFIE;

                                Formulaire.VerifieLe = DateTime.Now;

                            }
                            else if (NewSolde <= 0)
                            {


                                Formulaire.MontantDebMAJ = 0;

                                Formulaire.Status = Status.VERIFIE;
                                Formulaire.VerifieLe = DateTime.Now;
                                Formulaire.EtatClient = Note.SOLDE;

                            }

                            break;

                        case Note.A_VERIFIE:

                            Formulaire.MontantVerseDeclare = double.Parse(montant.Replace('.', ','));

                            NewSolde = Decimal.Subtract(decimal.Parse(DebMaJ.ToString()), decimal.Parse(Formulaire.MontantVerseDeclare.ToString()));


                            if (NewSolde <= 0)
                            {

                                Formulaire.MontantDebMAJ = 0;

                                Formulaire.Status = Status.VERIFIE;
                                Formulaire.VerifieLe = DateTime.Now;
                                Formulaire.EtatClient = Note.SOLDE;

                            }
                            else if (NewSolde > 0)
                            {
                                Formulaire.MontantDebMAJ = double.Parse(NewSolde.ToString());

                                Formulaire.Status = Status.VERIFIE;
                                Formulaire.VerifieLe = DateTime.Now;
                                Formulaire.EtatClient = Note.SOLDE_TRANCHE;

                            }


                            break;
                    }


                    FormulaireService.Update(Formulaire);
                    FormulaireService.Commit();

                    return Json(new { });

                }
            }
        }

        [HttpPost]
        public ActionResult UpdateContactBanque(int id)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
                    ViewBag.AgentList = new SelectList(AgentListForDropDown(EmpService), "Value", "Text");

                    Formulaire formulaire = FormulaireService.GetById(id);

                    formulaire.ContacteBanque = true;

                    FormulaireService.Update(formulaire);
                    FormulaireService.Commit();

                    return Json(new { });

                }
            }

        }

        public DataTable GenerateDatatableFromJoinedList(List<ClientAffecteViewModel> list, string traite)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    List<FormulaireExportable> newList = new List<FormulaireExportable>();
                    DataTable dataTable = new DataTable();

                    if (traite.Equals("RDV"))
                    {
                        newList = list.Select(j =>
                         new FormulaireExportable
                         {
                             NomClient = j.Lot.NomClient,
                             Compte = j.Lot.Compte,
                             IDClient = j.Lot.IDClient,
                             RDV = j.Formulaire.DateRDV.ToString()
                         }).ToList();

                        dataTable.Columns.Add("IDClient", typeof(string));
                        dataTable.Columns.Add("Compte", typeof(string));
                        dataTable.Columns.Add("NomClient", typeof(string));
                        dataTable.Columns.Add("RDV", typeof(string));

                        foreach (FormulaireExportable c in newList)
                        {

                            DataRow row = dataTable.NewRow();
                            row["IDClient"] = c.IDClient;
                            row["Compte"] = c.Compte;
                            row["NomClient"] = c.NomClient;
                            row["RDV"] = c.RDV;
                            dataTable.Rows.Add(row);

                        }
                    }
                    else if (traite.Equals("SOLDE"))
                    {
                        newList = list.Select(j =>
                        new FormulaireExportable
                        {
                            NomClient = j.Lot.NomClient,
                            Compte = j.Lot.Compte,
                            IDClient = j.Lot.IDClient,
                            Versement = j.Formulaire.MontantVerseDeclare+""
                        }

                        ).ToList();

                        dataTable.Columns.Add("IDClient", typeof(string));
                        dataTable.Columns.Add("Compte", typeof(string));
                        dataTable.Columns.Add("NomClient", typeof(string));
                        dataTable.Columns.Add("Versement", typeof(string));

                        foreach (FormulaireExportable c in newList)
                        {

                            DataRow row = dataTable.NewRow();
                            row["IDClient"] = c.IDClient;
                            row["Compte"] = c.Compte;
                            row["NomClient"] = c.NomClient;
                            row["Versement"] = c.Versement;
                            dataTable.Rows.Add(row);

                        }

                    }
                    else if (traite.Equals("A_VERIFIE"))
                    {
                        newList = list.Select(j =>
                        new FormulaireExportable
                        {
                            NomClient = j.Lot.NomClient,
                            Compte = j.Lot.Compte,
                            IDClient = j.Lot.IDClient,
                        }

                        ).ToList();

                        dataTable.Columns.Add("IDClient", typeof(string));
                        dataTable.Columns.Add("Compte", typeof(string));
                        dataTable.Columns.Add("NomClient", typeof(string));
                        dataTable.Columns.Add("Montant", typeof(string));

                        foreach (FormulaireExportable c in newList)
                        {

                            DataRow row = dataTable.NewRow();

                            row["IDClient"] = c.IDClient;
                            row["Compte"] = c.Compte;
                            row["NomClient"] = c.NomClient;

                            dataTable.Rows.Add(row);

                        }

                    }
                    else
                    {
                        newList = list.Select(j =>
                        new FormulaireExportable
                        {
                            NomClient = j.Lot.NomClient,
                            Compte = j.Lot.Compte,
                            IDClient = j.Lot.IDClient,
                        }

                        ).ToList();

                        dataTable.Columns.Add("IDClient", typeof(string));
                        dataTable.Columns.Add("Compte", typeof(string));
                        dataTable.Columns.Add("NomClient", typeof(string));

                        foreach (FormulaireExportable c in newList)
                        {


                            DataRow row = dataTable.NewRow();
                            row["IDClient"] = c.IDClient;
                            row["Compte"] = c.Compte;
                            row["NomClient"] = c.NomClient;
                            dataTable.Rows.Add(row);

                        }

                    }

                    return dataTable;

                }
            }
        }

        public ActionResult EnvoyerBanque()
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    LotService LotService = new LotService(UOW);
                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(EnvoyerTraiteListForDropDown(), "Value", "Text");
                    ViewBag.typeTrait = new SelectList(TraiteTypeForValiderListForDropDown(), "Value", "Text");


                    return View();

                }
            }


        }


        [HttpPost]
        public ActionResult EnvoyerBanqueLoadData(string traite, string numLot, string type, string debutDate, string finDate,string jourdate, string objet, string email, bool send, string to)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);


                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(EnvoyerTraiteListForDropDown(), "Value", "Text");
                    ViewBag.typeTrait = new SelectList(TraiteTypeForValiderListForDropDown(), "Value", "Text");

                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    JoinedList = (from f in FormulaireService.GetAll()
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId

                                  select new ClientAffecteViewModel
                                  {

                                      Formulaire = f,
                                      Lot = l,
                                      Affectation = a

                                  }).Where(j => ((j.Formulaire.Status == Status.VERIFIE || (j.Formulaire.EtatClient == Note.A_VERIFIE && j.Formulaire.Status == Status.EN_COURS)))).ToList();

                    string subject = "";
                    string body = "";
                    string name = "";
                    string To = "";

                    if (traite == "RDV")
                    {

                        JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.RDV).ToList();
                        subject = EmailConstants.RDV_SUBJECT;
                        body = EmailConstants.RDV_BODY;
                        name = "RDV";
                        To = EmailConstants.TO;
                    }
                    else if (traite == "SOLDE")
                    {

                        JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.SOLDE || j.Formulaire.EtatClient == Note.SOLDE_TRANCHE).ToList();
                        subject = EmailConstants.VERSEMENT_SUBJECT;
                        body = EmailConstants.VERSEMENT_BODY;
                        name = "SOLDE";
                        To = EmailConstants.TO;

                    }
                    else if (traite == "A_VERIFIE")
                    {

                        JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.A_VERIFIE).ToList();
                        subject = EmailConstants.AVERIFIE_SUBJECT;
                        body = EmailConstants.AVERIFIE_BODY;
                        name = "A_VERIFIE";
                        To = EmailConstants.TO;

                    }
                    else if (traite == "Autre")
                    {

                        JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.FAUX_NUM || j.Formulaire.EtatClient == Note.NRP || j.Formulaire.EtatClient == Note.RACCROCHE || j.Formulaire.EtatClient == Note.INJOIGNABLE || j.Formulaire.EtatClient == Note.RAPPEL || j.Formulaire.EtatClient == Note.REFUS_PAIEMENT).ToList();
                        subject = EmailConstants.ENCOURS_SUBJECT;
                        body = EmailConstants.ENCOURS_BODY;
                        name = "EN_COURS";
                        To = EmailConstants.TO;

                    }


                    if (numLot != "0")
                    {
                        JoinedList = JoinedList.Where(j => j.Lot.NumLot.Equals(numLot)).ToList();
                    }

                    Debug.WriteLine(jourdate);

                    if(type == "P_INTERVAL")
                    {

                        if(debutDate!=null && debutDate!="" && finDate!="" && finDate!=null)
                        {
                            JoinedList = JoinedList.Where(j => j.Formulaire.TraiteLe.Date >= DateTime.Parse(debutDate).Date && j.Formulaire.TraiteLe.Date <= DateTime.Parse(finDate).Date).ToList();
                        }

                    }else if(type == "P_DATE")
                    {

                        if (jourdate != null && jourdate != "")
                        {
                            JoinedList = JoinedList.Where(j => j.Formulaire.TraiteLe.Date == DateTime.Parse(jourdate).Date).ToList();
                        }

                    }


                    if (send == true)
                    {

                        string path = GetFolderName() + "/" + name + "_MAJ_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + ".xlsx";

                        GenerateExcel(GenerateDatatableFromJoinedList(JoinedList, traite), path);

                        SendMail(to, objet, email, path);

                        foreach (var j in JoinedList)
                        {

                            j.Formulaire.NotifieBanque = true;
                            FormulaireService.Update(j.Formulaire);

                        }
                        FormulaireService.Commit();

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

                             j.Lot.IDClient.ToString().Contains(search)
                            || j.Lot.Compte.ToString().Contains(search)
                            || j.Lot.NomClient.ToString().ToLower().Contains(search.ToLower())

                                ).ToList();
                        
                        }

                        JoinedList = SortTableDataForValidate(order, orderDir, JoinedList);

                        int recFilter = JoinedList.Count();

                        JoinedList = JoinedList.Skip(startRec).Take(pageSize).ToList();

                        var modifiedData = JoinedList.Select(j =>
                           new
                           {

                               j.Lot.NumLot,
                               j.Lot.Compte,
                               j.Lot.IDClient,
                               j.Lot.NomClient,
                               Etat = j.Formulaire.EtatClient.ToString(),

                           }
                           );

                        int x = totalRecords;
                        ViewBag.x = x;
                        var info = new { nbTotal = x, subject = subject, body = body, to = To };

                        result = this.Json(new
                        {

                            draw = Convert.ToInt32(draw),
                            recordsTotal = totalRecords,
                            recordsFiltered = recFilter,
                            data = modifiedData,
                            info = info

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


        public static void GenerateExcel(DataTable dataTable, string path)
        {
            
            
            dataTable.TableName = "Table1";

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);
            // create a excel app along side with workbook and worksheet and give a name to it
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook excelWorkBook = excelApp.Workbooks.Add();

            Excel._Worksheet xlWorksheet = excelWorkBook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            foreach (DataTable table in dataSet.Tables)
            {
                //Add a new worksheet to workbook with the Datatable name
                // Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();

                excelWorkSheet.Cells.EntireColumn.NumberFormat = "@";

                excelWorkSheet.Name = table.TableName;
                
                // add all the columns
                for (int i = 1; i < table.Columns.Count + 1; i++)
                {
                    excelWorkSheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
                    excelWorkSheet.Cells[1, i].Font.Bold = true;
                    excelWorkSheet.Cells[1, i].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    excelWorkSheet.Cells[1, i].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    excelWorkSheet.Cells[1, i].Borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                    excelWorkSheet.Cells[1, i].Borders.Weight = 2;
                    excelWorkSheet.Cells[1, i].Font.Size = 14;
                    excelWorkSheet.Cells[1, i].ColumnWidth = 22;

                }
                // add all the rows
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    for (int k = 0; k < table.Columns.Count; k++)
                    {

                        excelWorkSheet.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();
                        excelWorkSheet.Cells[j + 2, k + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        excelWorkSheet.Cells[j + 2, k + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        excelWorkSheet.Cells[j + 2, k + 1].Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                        excelWorkSheet.Cells[j + 2, k + 1].Borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                        excelWorkSheet.Cells[j + 2, k + 1].Borders.Weight = 2;
                        excelWorkSheet.Cells[j + 2, k + 1].Font.Size = 12;
                        excelWorkSheet.Cells[j + 2, k + 1].ColumnWidth = 22;
                    }
                }
            }
            // excelWorkBook.Save(); -> this will save to its default location

            excelWorkBook.SaveAs(path); // -> this will do the custom

            excelWorkBook.Close();
            excelApp.Quit();
        }

        public void SendMail(string to, string subject, string body, string path)
        {

            MailMessage mm = new MailMessage();
            mm.From = new MailAddress("alwakil.recouvrement@gmail.com");
            foreach (string t in to.Split(',').ToList())
            {
                mm.To.Add(t);
            }

            mm.Subject = subject;
            mm.Body = String.Format(@"Bonjour Mr,
                     <br />
                     <br /> {0}
                     <br />
                     <br />
                     Bien cordialement.                     
                     ", body) ;
                mm.IsBodyHtml = true;

            mm.Attachments.Add(new Attachment(path));
            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.UseDefaultCredentials = false;
            smtp.Port = 587;
            smtp.EnableSsl = true;

            smtp.Credentials = new NetworkCredential("alwakil.recouvrement@gmail.com", "nbm290481");
            smtp.Send(mm);
        }

        public string GetFolderName()
        {
            string folderName = Server.MapPath("~/Uploads/Updates");
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);

            }

            return folderName;
        }

        public string getImagePath(Formulaire formulaire)
        {
            string path = Server.MapPath("~/Uploads/Recu/") + formulaire.FormulaireId;
            List<string> urlImages =new List<string>(); 
            if(!Directory.Exists(path))
            {
                return "";
            }
            else
            {
                string uri = HttpContext.Request.Url.AbsoluteUri.Replace(HttpContext.Request.Url.LocalPath, "/WakilRecouvrement/" + "/Uploads/Recu/" + formulaire.FormulaireId)+"/";
                Debug.WriteLine(uri);
                foreach(string file in Directory.GetFiles(path))
                {
                    urlImages.Add(uri + Path.GetFileName(file) );
                }
                return String.Join(",", urlImages.ToArray());
            }
        }

        public string getImagePathNB(Formulaire formulaire)
        {
            string path = Server.MapPath("~/Uploads/Recu/") + formulaire.FormulaireId;
            if (!Directory.Exists(path))
            {
                return "0";
            }
            else
            {
                return Directory.GetFiles(path).Length + "";
            }
        }


        [HttpPost]
        public ActionResult UploadVerifier(HttpPostedFileBase PostedFile)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
                    ViewBag.AgentList = new SelectList(AgentListForDropDown(EmpService), "Value", "Text");


                    //Nthabtou li fichier mahouch feragh makenesh nabaathou erreur lel client
                    if (PostedFile != null)
                    {
                        //nsobou l fichier aana fel serveur
                        string filePath = string.Empty;
                        string path = Server.MapPath("~/Uploads/");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        filePath = path + Path.GetFileName(PostedFile.FileName);
                        string extension = Path.GetExtension(PostedFile.FileName);
                        string conString = string.Empty;

                        if (PostedFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                        {
                            PostedFile.SaveAs(filePath);

                            //besh nakhtarou connectionString selon l version mtaa excel (xls = 2003 o xlsx mel 2013 o ahna tal3in)
                            //L connectionString predefini fel web.config mteena
                            switch (extension)
                            {
                                case ".xls":
                                    conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                                    break;

                                case ".xlsx":
                                    conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                                    break;

                            }

                            //Taoua besh nebdew nakraw l fichier Excel bel library OleDd
                            DataTable dt = new DataTable();
                            conString = string.Format(conString, filePath);
                            using (OleDbConnection connExcel = new OleDbConnection(conString))
                            {
                                using (OleDbCommand cmdExcel = new OleDbCommand())
                                {
                                    using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                                    {
                                        // Houni nebdew naakraw awel sheet name mtaa l document excel mteena (eli ken jina fi table SQL rahou le nom de la table) 
                                        cmdExcel.Connection = connExcel;
                                        connExcel.Open();
                                        DataTable dtExcelSchema;
                                        dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                        string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                                        connExcel.Close();

                                        //Houni recuperation mtaa les données 
                                        connExcel.Open();
                                        cmdExcel.CommandText = "SELECT * FROM [Table1$]";
                                        odaExcel.SelectCommand = cmdExcel;
                                        odaExcel.Fill(dt);
                                        connExcel.Close();


                                        string argIDClient = "IDClient";
                                        string argCompte = "Compte";
                                        string argNomClient = "NomClient";
                                        string argMontant = "Montant";


                                        foreach (DataRow row in dt.Rows)
                                        {
                                            string IDClient = "";
                                            string Compte = "";
                                            string NomClient = "";
                                            string Montant = "";


                                            IDClient = row[argIDClient].ToString();
                                            Compte = row[argCompte].ToString();
                                            NomClient = row[argNomClient].ToString();
                                            Montant = row[argMontant].ToString();


                                            VerifyClient(IDClient, Montant,FormulaireService,AffectationService,LotService);
                                        }

                                        FormulaireService.Commit();

                                    }
                                }
                            }

                        }
                        else
                        {
                            ModelState.AddModelError("Importer", "Le fichier selectionné n'est pas un fichier Excel");
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("Importer", "Vous devez sélectionner un fichier");
                    }

                    return RedirectToAction("ValiderTraitement");
                }
            }
        }


        public void VerifyClient(string idclient, string montant, FormulaireService FormulaireService, AffectationService AffectationService, LotService LotService)
        {
          
                    if (montant == "")
                    {
                        montant = "0";
                    }

                    var formulaireAtraite = (from f in FormulaireService.GetAll()
                                             join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                             join l in LotService.GetAll() on a.LotId equals l.LotId
                                             where l.IDClient == idclient && f.EtatClient == Note.A_VERIFIE && f.Status == Status.EN_COURS
                                             select new ClientAffecteViewModel
                                             {
                                                 Formulaire = f,
                                                 Affectation = a,
                                                 Lot = l
                                             }).ToList().FirstOrDefault();

                    try
                    {

                        if (formulaireAtraite != null)
                        {
                            formulaireAtraite.Formulaire.MontantVerseDeclare = double.Parse(montant.Replace('.', ','));
                        }
                        else
                        {
                            Debug.WriteLine(idclient);
                        }
                    }
                    catch (FormatException)
                    {
                        if(formulaireAtraite!=null)
                        DeleteFromulaire(formulaireAtraite.Formulaire, FormulaireService);
                    }

                    Decimal NewSolde = 0;
                    if(formulaireAtraite!=null)
                    {
                        NewSolde = Decimal.Subtract(decimal.Parse(formulaireAtraite.Formulaire.MontantDebMAJ.ToString()), decimal.Parse(formulaireAtraite.Formulaire.MontantVerseDeclare.ToString()));
                        double DebMaJ = GetFormulaire(formulaireAtraite.Formulaire.AffectationId).MontantDebMAJ;



                        NewSolde = Decimal.Subtract(decimal.Parse(DebMaJ.ToString()), decimal.Parse(formulaireAtraite.Formulaire.MontantVerseDeclare.ToString()));

                        if (NewSolde <= 0)
                        {

                            formulaireAtraite.Formulaire.MontantDebMAJ = 0;

                            formulaireAtraite.Formulaire.Status = Status.VERIFIE;
                            formulaireAtraite.Formulaire.VerifieLe = DateTime.Now;
                            formulaireAtraite.Formulaire.EtatClient = Note.SOLDE;

                        }
                        else if (NewSolde > 0)
                        {
                            formulaireAtraite.Formulaire.MontantDebMAJ = double.Parse(NewSolde.ToString());

                            formulaireAtraite.Formulaire.Status = Status.VERIFIE;
                            formulaireAtraite.Formulaire.VerifieLe = DateTime.Now;
                            formulaireAtraite.Formulaire.EtatClient = Note.SOLDE_TRANCHE;

                        }


                FormulaireService.Update(formulaireAtraite.Formulaire);
                //FormulaireService.Commit();
            
            }
                    

              

        }


        public void DeleteFromulaire(Formulaire formulaire,FormulaireService FormulaireService)
        {
                    formulaire.Status = Status.NON_VERIFIE;
                    FormulaireService.Update(formulaire);
                FormulaireService.Commit();
        }

        public ActionResult SuiviRDV(string numLot, string RDVType,string RdvDate, string sortOrder, string currentFilterNumLot, string currentFilterRDVType, string CurrentSort, int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.RDVList = new SelectList(RDVForDropDown(), "Value", "Text");
                    ViewData["sortOrder"] = new SelectList(SortOrderSuiviRDVForDropDown(), "Value", "Text");


                    if (numLot != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        numLot = currentFilterNumLot;
                    }

                    ViewBag.currentFilterNumLot = numLot;


                    if (RDVType != null)
                    {
                        // page = 1;
                    }
                    else
                    {
                        RDVType = currentFilterRDVType;
                    }
                    ViewBag.currentFilterRDVType = RDVType;


                    if (sortOrder != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        sortOrder = CurrentSort;
                    }


                    ViewBag.CurrentSort = sortOrder;



                    JoinedList = (from f in FormulaireService.GetMany(f => f.EtatClient == Note.RDV)
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId
                                  where a.Employe.Username.Equals(Session["username"])
                                  orderby f.TraiteLe descending
                                  select new ClientAffecteViewModel
                                  {

                                      Formulaire = f,
                                      Affectation = a,
                                      Lot = l,

                                  }).Where(j => verifMesRDV(j.Affectation.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).ToList();

                    if (!String.IsNullOrEmpty(numLot))
                    {
                        if (numLot != "0")
                        {
                            JoinedList = JoinedList.Where(j => j.Lot.NumLot.Equals(numLot)).ToList();
                        }
                    }

                    if (!String.IsNullOrEmpty(RDVType))
                    {
                        if (RDVType == "RDV_J")
                        {
                            JoinedList = JoinedList.Where(j => j.Formulaire.DateRDV.Date == DateTime.Today.Date).ToList();

                        }
                        else if (RDVType == "RDV_DEMAIN")
                        {

                            JoinedList = JoinedList.Where(j => j.Formulaire.DateRDV.Date == DateTime.Today.AddDays(1).Date).ToList();

                        }
                        else if (RDVType == "RDV_JOURS_PROCHAINE")
                        {

                            JoinedList = JoinedList.Where(j => j.Formulaire.DateRDV.Date >= DateTime.Today.AddDays(2).Date && j.Formulaire.DateRDV.Date < DateTime.Today.AddDays(7).Date).ToList();

                        }
                        else if (RDVType == "RDV_SEMAINE_PROCHAINE")
                        {

                            JoinedList = JoinedList.Where(j => j.Formulaire.DateRDV.Date >= DateTime.Today.AddDays(7).Date && j.Formulaire.DateRDV.Date < DateTime.Today.AddDays(14).Date).ToList();

                        }
                        else if (RDVType == "RDVDate")
                        {
                            JoinedList = JoinedList.Where(j => j.Formulaire.DateRDV.Date == DateTime.Parse(RdvDate).Date).ToList();

                        }
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
                                JoinedList = JoinedList.OrderBy(s => s.Lot.SoldeDebiteur).ToList();

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
                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderByDescending(s => s.Formulaire.DateRDV).ToList();

                            break;
                        case "6":

                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderBy(s => s.Formulaire.DateRDV).ToList();

                            break;
                        case "7":

                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderByDescending(s => s.Formulaire.TraiteLe).ToList();

                            break;
                        case "8":

                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderBy(s => s.Formulaire.TraiteLe).ToList();

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

        public string GetAffectationAgent(Affectation affectation)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                     AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    var username = from a in AffectationService.GetAll()
                                   join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                   select new
                                   {
                                       Username = e.Username
                                   };

                    return username.FirstOrDefault().Username;

                }
            }
        }


        [HttpPost]
        public ActionResult SuiviRDVNB()
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);

                    int nb = 0;
                    int rappelNB = 0;
                    int rejetes = 0;


                    nb = (from f in FormulaireService.GetMany(f => f.EtatClient == Note.RDV)
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where a.Employe.Username.Equals(Session["username"]) && f.DateRDV.Date == DateTime.Now.Date
                          orderby f.TraiteLe descending
                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).Where(j => verifMesRDV(j.Affectation.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).Count();



                    rappelNB = (from f in FormulaireService.GetAll()
                                join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                join l in LotService.GetAll() on a.LotId equals l.LotId
                                where a.Employe.Username.Equals(Session["username"]) && f.RappelLe.Date == DateTime.Now.Date

                                select new ClientAffecteViewModel
                                {
                                    Lot = l,
                                    Affectation = a,
                                    Formulaire = f

                                }).Where(j => verifMesRappels(j.Affectation.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).Count();


                    rejetes = (from f in FormulaireService.GetAll()
                               join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                               join l in LotService.GetAll() on a.LotId equals l.LotId
                               where a.Employe.Username.Equals(Session["username"]) && f.Status == Status.NON_VERIFIE

                               select new ClientAffecteViewModel
                               {
                                   Lot = l,
                                   Affectation = a,
                                   Formulaire = f

                               }).Where(j => verifMesRappels(j.Affectation.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).Count();




                    return Json(new { nb = nb, rappelNB = rappelNB, rejetes = rejetes });
                }
            }
        }

        [HttpPost]
        public ActionResult UpdateTelFN(int lotId,int affectationId,string portable,string fixe)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);

                    bool TelFixe = false;
                    bool TelPortable = false;
                    Lot lot = LotService.GetById(lotId);


                    if (Request.Form["TelPortable"] == "on")
                    {
                        TelPortable = true;
                    }
                    if (Request.Form["TelFixe"] == "on")
                    {
                        TelFixe = true;
                    }

                    lot.TelFixe = fixe;
                    lot.TelPortable = portable;

                    lot.TelFixeFN = TelFixe;
                    lot.TelPortableFN = TelPortable;


                    LotService.Update(lot);
                    LotService.Commit();


                    return RedirectToAction("CreerFormulaire", "Formulaire", new { id = affectationId, msgError = "" });

                }
            }
        }

        public bool verifMesRappels(int affId,DateTime formTraiteLe, FormulaireService FormulaireService)
        {
                
                   
                    int res = FormulaireService.GetMany(f => f.AffectationId == affId && f.TraiteLe > formTraiteLe && (f.EtatClient == Note.A_VERIFIE || f.EtatClient == Note.RDV || f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE)).Count();
                    if (res == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;

                    }
                
        }

        public bool verifMesRDV(int affId, DateTime formTraiteLe, FormulaireService FormulaireService)
        {
            


                    int res = FormulaireService.GetMany(f => f.AffectationId == affId &&  f.TraiteLe > formTraiteLe && (f.EtatClient == Note.A_VERIFIE || f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE || f.EtatClient == Note.RAPPEL)).Count();
                   
                    if (res == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }       

        }



        public ActionResult MesRappels(string numLot,string currentFilterNumLot,string CurrentSort, string RappelDate, string sortOrder, int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    if (Request.Form["RappelDate"] == null)
                    {

                        JoinedList = (from f in FormulaireService.GetMany(f => f.EtatClient == Note.RAPPEL)
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      where a.Employe.Username.Equals(Session["username"])


                                      select new ClientAffecteViewModel
                                      {
                                          Lot = l,
                                          Affectation = a,
                                          Formulaire = f

                                      }).OrderByDescending(o => o.Formulaire.TraiteLe).Where(j => verifMesRappels(j.Affectation.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).ToList();

                    }
                    else
                    {

                        DateTime d = DateTime.Now;

                        if (DateTime.TryParse(RappelDate, out d))
                        {

                            JoinedList = (from f in FormulaireService.GetMany(f => f.EtatClient == Note.RAPPEL)
                                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          where a.Employe.Username.Equals(Session["username"]) && f.RappelLe.Date == d.Date
                                          select new ClientAffecteViewModel
                                          {
                                              Lot = l,
                                              Affectation = a,
                                              Formulaire = f

                                          }).OrderByDescending(o => o.Formulaire.TraiteLe).Where(j => verifMesRappels(j.Affectation.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).ToList();
                        }

                    }



                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    ViewData["sortOrder"] = new SelectList(SortOrderSuiviRDVForDropDown(), "Value", "Text");

                    if (numLot != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        numLot = currentFilterNumLot;
                    }

                    ViewBag.currentFilterNumLot = numLot;

                    if (sortOrder != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        sortOrder = CurrentSort;
                    }


                    ViewBag.CurrentSort = sortOrder;






                    if (!String.IsNullOrEmpty(numLot))
                    {
                        if (numLot != "0")
                        {
                            JoinedList = JoinedList.ToList().Where(j => j.Lot.NumLot.Equals(numLot)).ToList();
                        }
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
                                JoinedList = JoinedList.OrderBy(s => s.Lot.SoldeDebiteur).ToList();

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
                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderByDescending(s => s.Formulaire.DateRDV).ToList();

                            break;
                        case "6":

                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderBy(s => s.Formulaire.DateRDV).ToList();

                            break;
                        case "7":

                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderByDescending(s => s.Formulaire.TraiteLe).ToList();

                            break;
                        case "8":

                            JoinedList = JoinedList.Where(s => s.Formulaire != null).OrderBy(s => s.Formulaire.TraiteLe).ToList();

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
         

        public ActionResult TraitementRejetes(string SearchString, string currentFilter, string sortOrder, int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    ViewBag.CurrentSort = sortOrder;

                    if (SearchString != null)
                    {
                        page = 1;
                    }
                    else
                    {
                        SearchString = currentFilter;
                    }

                    ViewBag.CurrentFilter = SearchString;

                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    JoinedList = (from f in FormulaireService.GetAll()
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId
                                  where a.Employe.Username.Equals(Session["username"]) && f.Status == Status.NON_VERIFIE

                                  select new ClientAffecteViewModel
                                  {
                                      Lot = l,
                                      Affectation = a,
                                      Formulaire = f

                                  }).Where(j => verifMesRappels(j.Affectation.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).ToList();

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


                    ViewBag.total = JoinedList.Count();

                    int pageSize = 10;
                    int pageNumber = (page ?? 1);

                    return View(JoinedList.ToPagedList(pageNumber, pageSize));
                }
            }
        }


        public ActionResult SuiviTraiement(string numLot,string currentNumLot, string SearchString, string currentFilter, string traite,string currentTraite, string agent,string currentAgent, string traitDate,string currentTraitDate,string type,string currentType, int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);


                    List<ClientAffecteViewModel> JoinedList;

                    if (SearchString != null)
                    {
                        // page = 1;
                    }
                    else
                    {
                        SearchString = currentFilter;
                    }

                    ViewBag.currentFilter = SearchString;


                    if (numLot != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        numLot = currentNumLot;
                    }

                    ViewBag.currentNumLot = numLot;

                    if (type != null)
                    {
                        //type = 1;
                    }
                    else
                    {
                        type = currentType;
                    }

                    ViewBag.currentType = type;

                    if (traite != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        traite = currentTraite;
                    }

                    ViewBag.currentTraite = traite;

                    if (agent != null)
                    {
                        ///page = 1;
                    }
                    else
                    {
                        agent = currentAgent;
                    }


                    ViewBag.currentAgent = agent;

                    if (traitDate != null)
                    {
                        //page = 1;
                    }
                    else
                    {
                        traitDate = currentTraitDate;
                    }

                    ViewBag.currentTraitDate = traitDate;

                    ViewBag.page = page;

                    ViewData["list"] = new SelectList(NumLotListForDropDown(LotService), "Value", "Text");
                    
                    if(Session["role"].Equals("admin"))
                    ViewBag.AgentList = new SelectList(AgentListForDropDown(EmpService), "Value", "Text");
                    

                    ViewBag.TraiteList = new SelectList(TraiteListSuiviTraitForDropDown(), "Value", "Text");
                    ViewBag.typeTrait = new SelectList(typeListForDropDown(), "Value", "Text");


                    if (!String.IsNullOrEmpty(traite))
                    {

                        if (traite == "ALL")
                        {

                            JoinedList = (from f in FormulaireService.GetAll()
                                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = e.Username

                                          }).OrderByDescending(f => f.Formulaire.TraiteLe).ToList();

                        }

                        else
                        {
                            JoinedList = (from f in FormulaireService.GetAll()
                                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId

                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = e.Username

                                          }).Where(f => f.Formulaire.EtatClient + "" == traite).OrderByDescending(f => f.Formulaire.TraiteLe).ToList();
                        }

                    }
                    else
                    {

                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId

                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Affectation = a,
                                          Lot = l,
                                          Agent = e.Username

                                      }).OrderByDescending(f => f.Formulaire.TraiteLe).ToList();

                    }



                    if (type == "DATE_TRAIT")
                    {

                        if (!String.IsNullOrEmpty(traitDate))
                        {
                            DateTime d = DateTime.Parse(traitDate);
                            JoinedList = JoinedList.Where(j => j.Formulaire.TraiteLe.Date == d.Date).ToList();
                        }

                    }

                    if(Session["role"].Equals("admin"))
                    {
                        if (!String.IsNullOrEmpty(agent))
                        {
                            if (int.Parse(agent) != 0)
                            {
                                JoinedList = JoinedList.Where(j => j.Affectation.EmployeId == int.Parse(agent)).ToList();
                            }

                        }
                    }
                    else
                    {
                                JoinedList = JoinedList.Where(j => j.Affectation.Employe.Username.Equals(Session["username"])).ToList();
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


                    ViewBag.total = JoinedList.Count();

                    int pageSize = 10;
                    int pageNumber = (page ?? 1);

                    return View(JoinedList.ToPagedList(pageNumber, pageSize));
                }
            }
        }




    }


}