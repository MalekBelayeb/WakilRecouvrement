using Microsoft.Ajax.Utilities;
using MyFinance.Data.Infrastructure;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Service;
using System.Diagnostics;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Web.Controllers
{
    public class TousClientsAffecteController : Controller
    {


        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");


       

        public ActionResult SuiviClient(string numLot, string SearchString, string traite, string agent, string currentFilterAgent, string currentFilter, string sortOrder, string CurrentSort, string currentFilterNumLot, string currentFilterTraite, int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    try
                    {


                        LotService LotService = new LotService(UOW);
                        FormulaireService FormulaireService = new FormulaireService(UOW);
                        AffectationService AffectationService = new AffectationService(UOW);
                        EmployeService EmpService = new EmployeService(UOW);

                        if (Session["username"] == null || Session["username"].ToString().Length < 1)
                            return RedirectToAction("Login", "Authentification");

                        ViewBag.CurrentSort = sortOrder;

                        List<ClientAffecteViewModel> JoinedList;

                        ViewData["list"] = new SelectList(DropdownListController.NumLotListForDropDown(LotService), "Value", "Text");
                        ViewBag.AgentList = new SelectList(DropdownListController.AgentListForDropDown(EmpService), "Value", "Text");
                        ViewBag.TraiteList = new SelectList(DropdownListController.TraiteListForDropDown(), "Value", "Text");
                        ViewData["sortOrder"] = new SelectList(DropdownListController.SortOrderSuiviTousClientForDropDown(), "Value", "Text");



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

                                JoinedList = (from l in LotService.GetAll()
                                              join f in FormulaireService.GetAll() on l.FormulaireId equals f.FormulaireId

                                              select new ClientAffecteViewModel
                                              {

                                                  Formulaire = f,
                                                  Lot = l,
                                                  AffectationId = f.AffectationId,
                                                  Agent = f.AgentUsername

                                              }).ToList();

                            }
                            else if (traite == "NON_TRAITE")
                            {

                                JoinedList = (from a in AffectationService.GetAll()
                                              join l in LotService.GetAll() on a.LotId equals l.LotId
                                              join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                                              where l.FormulaireId == null
                                              select new ClientAffecteViewModel
                                              {
                                                  AffectationId = a.AffectationId,
                                                  Lot = l,
                                                  Agent = e.Username
                                              }).DistinctBy(a => a.AffectationId).ToList();

                            }
                            else if (traite == "SAUF")
                            {

                                JoinedList = (from l in LotService.GetAll()
                                              join f in FormulaireService.GetAll() on l.FormulaireId equals f.FormulaireId
                                              select new ClientAffecteViewModel
                                              {

                                                  Formulaire = f,
                                                  Lot = l,
                                                  AffectationId = f.AffectationId,
                                                  Agent = f.AgentUsername,

                                              }).Where(f => f.Formulaire.EtatClient + "" != "SOLDE" && f.Formulaire.EtatClient + "" != "FAUX_NUM").ToList();
                            }
                            else
                            {
                                JoinedList = (from l in LotService.GetAll()
                                              join f in FormulaireService.GetAll() on l.FormulaireId equals f.FormulaireId
                                              select new ClientAffecteViewModel
                                              {

                                                  Formulaire = f,
                                                  Lot = l,
                                                  AffectationId = f.AffectationId,
                                                  Agent = f.AgentUsername

                                              }).Where(f => f.Formulaire.EtatClient + "" == traite).ToList();
                            }

                        }
                        else
                        {

                            JoinedList = (from l in LotService.GetAll()
                                          join f in FormulaireService.GetAll() on l.FormulaireId equals f.FormulaireId
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Lot = l,
                                              AffectationId = f.AffectationId,
                                              Agent = f.AgentUsername,

                                          }).ToList();
                        }


                        if (!String.IsNullOrEmpty(agent))
                        {
                            if (agent != "0")
                            {
                                JoinedList = JoinedList.Where(j => j.Agent == agent).ToList();
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


                            default:


                                break;
                        }


                        ViewBag.total = JoinedList.Count();

                        int pageSize = 10;
                        int pageNumber = (page ?? 1);


                        LotService.Dispose();
                        FormulaireService.Dispose();
                        AffectationService.Dispose();
                        EmpService.Dispose();
                        return View(JoinedList.ToPagedList(pageNumber, pageSize));


                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        return View("~/Views/Shared/Error.cshtml", null);

                    }

                }
            }
        }

    }
}