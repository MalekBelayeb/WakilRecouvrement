using MyFinance.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Service;
using Microsoft.Ajax.Utilities;
using PagedList;

namespace WakilRecouvrement.Web.Controllers
{
    public class MesAffectationListController : Controller
    {


        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            log.Error(filterContext.Exception);
        }

        public ActionResult MesAffectation(string numLot, string SearchString, string traite, string currentFilter, string currentFilterNumLot, string currentFilterTraite, string CurrentSort, string currentSoldeFilter, string sortOrder, string soldeFilter, int? page)
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

                    ViewData["list"] = new SelectList(DropdownListController.NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(DropdownListController.TraiteListForDropDown(), "Value", "Text");
                    ViewData["sortOrder"] = new SelectList(DropdownListController.SortOrderSuiviClientForDropDown(), "Value", "Text");

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

                                              Formulaire = FormulaireService.GetMany(f => f.FormulaireId == l.FormulaireId ).FirstOrDefault(),
                                              Lot = l,
                                              AffectationId = a.AffectationId

                                          }).DistinctBy(a => a.AffectationId).ToList();

                        }
                        else if (traite == "NON_TRAITE")
                        {

                            JoinedList = (from a in AffectationService.GetMany(a => a.EmployeId == emp.EmployeId)
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          where l.FormulaireId == null
                                          select new ClientAffecteViewModel
                                          {
                                              AffectationId = a.AffectationId,
                                              Lot = l,

                                          }).DistinctBy(a => a.AffectationId).ToList();

                        }
                        else if (traite == "SAUF")
                        {

                            JoinedList = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                          join l in LotService.GetAll() on f.FormulaireId equals l.FormulaireId
                                          where f.AgentUsername == emp.Username
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Lot = l,
                                              AffectationId = f.AffectationId


                                          }).Where(f => f.Formulaire.EtatClient + "" != "SOLDE" && f.Formulaire.EtatClient + "" != "FAUX_NUM").ToList();


                        }
                        else if (traite == "VERS")
                        {

                            JoinedList = (from f in FormulaireService.GetAll()
                                          join l in LotService.GetAll() on f.FormulaireId equals l.FormulaireId
                                          where f.AgentUsername == emp.Username
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Lot = l,
                                              AffectationId = f.AffectationId

                                          }).Where(f => f.Formulaire.EtatClient == Note.SOLDE || f.Formulaire.EtatClient == Note.SOLDE_TRANCHE || f.Formulaire.EtatClient == Note.A_VERIFIE).OrderByDescending(f => f.Formulaire.TraiteLe).ToList();


                        }
                        else
                        {

                            JoinedList = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                          join l in LotService.GetAll() on f.FormulaireId equals l.FormulaireId
                                          where f.AgentUsername == emp.Username
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Lot = l,
                                              AffectationId = f.AffectationId

                                          }).Where(f => f.Formulaire.EtatClient + "" == traite).ToList();

                        }

                    }
                    else
                    {
                        JoinedList = (from a in AffectationService.GetMany(a => a.EmployeId == emp.EmployeId)
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      
                                      select new ClientAffecteViewModel
                                      {
                                          Formulaire = FormulaireService.GetMany(f => f.FormulaireId == l.FormulaireId ).FirstOrDefault(),
                                          Affectation = a,
                                          Lot = l,
                                          AffectationId = a.AffectationId

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
                                            JoinedList = JoinedList.Where(j => double.TryParse(j.Lot.SoldeDebiteur, out double soldedeb) && soldedeb > filter).OrderBy(j => double.Parse(j.Lot.SoldeDebiteur)).ToList();

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
                                            JoinedList = JoinedList.Where(j => double.TryParse(j.Lot.SoldeDebiteur, out double soldedeb) && soldedeb < filter).OrderByDescending(j => double.Parse(j.Lot.SoldeDebiteur)).ToList();

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

    }
}