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
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Web.Models.ViewModel;

namespace WakilRecouvrement.Web.Controllers
{

    public class ValiderTraitementController : Controller
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            log.Error(filterContext.Exception);
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

        public ActionResult Valider(string numLot, string currentNumLot, string SearchString, string currentFilter, string traite, string currentTraite, string agent, string currentAgent, string currentPage, int? page,string messageFromExcelVerifier)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    if (page == null)
                    {
                        if (currentPage != null)
                            page = int.Parse(currentPage);
                    }

                    ViewBag.page = page;

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

                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    ViewData["list"] = new SelectList(DropdownListController.NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(DropdownListController.TraiteValidationListForDropDown(), "Value", "Text");
                    ViewBag.AgentList = new SelectList(DropdownListController.AgentListForDropDown(EmpService), "Value", "Text");

                    if (!String.IsNullOrEmpty(traite))
                    {
                        if (traite.Equals("ALL"))
                        {
                            JoinedList = (from f in FormulaireService.GetAll()
                                          join l in LotService.GetAll() on f.FormulaireId equals l.FormulaireId
                                          select new ClientAffecteViewModel
                                          {
                                              Formulaire = f,
                                              Lot = l,
                                              AffectationId = f.AffectationId,
                                              Agent = f.AgentUsername

                                          }).OrderByDescending(j => j.Formulaire.TraiteLe).Where(j => j.Formulaire.Status == Status.EN_COURS).Where(j => j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE_TRANCHE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "A_VERIFIE")).ToList();

                        }
                        else
                        {
                            JoinedList = (from f in FormulaireService.GetAll()
                                          join l in LotService.GetAll() on f.FormulaireId equals l.FormulaireId

                                          select new ClientAffecteViewModel
                                          {
                                              Formulaire = f,
                                              Lot = l,
                                              AffectationId = f.AffectationId,
                                              Agent = f.AgentUsername

                                          }).OrderByDescending(j => j.Formulaire.TraiteLe).Where(j => j.Formulaire.Status == Status.EN_COURS).Where(j => j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), traite)).ToList();

                        }

                    }
                    else
                    {
                        JoinedList = (from f in FormulaireService.GetAll()
                                      join l in LotService.GetAll() on f.FormulaireId equals l.FormulaireId
                                      select new ClientAffecteViewModel
                                      {
                                          Formulaire = f,
                                          Lot = l,
                                          AffectationId = f.AffectationId,
                                          Agent = f.AgentUsername

                                      }).OrderByDescending(j => j.Formulaire.TraiteLe).Where(j => j.Formulaire.Status == Status.EN_COURS).Where(j => j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE_TRANCHE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "A_VERIFIE")).ToList();

                    }

                    if (!String.IsNullOrEmpty(numLot))
                    {
                        if (numLot != "0")
                        {
                            JoinedList = JoinedList.Where(j => j.Lot.NumLot.Equals(numLot)).ToList();

                        }
                    }

                    if (!String.IsNullOrEmpty(agent))
                    {
                        if (int.Parse(agent) != 0)
                        {

                            JoinedList = JoinedList.Where(j => j.Agent == agent).ToList();

                        }
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


                    var modifiedData = JoinedList.Select(j =>
                            new ValiderTraitementViewModel
                            {

                                Lot = j.Lot,
                                Username = j.Agent,
                                VerifieLe = j.Formulaire.VerifieLe.ToString(),
                                DateAff = "",
                                TraiteLe = j.Formulaire.TraiteLe.ToString("dd/MM/yyyy HH:mm:ss"),
                                Etat = GetEtat(j.Formulaire).ToString(),
                                FormulaireId = j.Formulaire.FormulaireId,
                                ContactBanque = j.Formulaire.ContacteBanque,
                                MontantVerseDeclare = j.Formulaire.MontantVerseDeclare,
                                descAutre = j.Formulaire.DescriptionAutre,
                                AffectationId = j.Formulaire.AffectationId
                            });

                    ViewBag.total = JoinedList.Count();

                    if(messageFromExcelVerifier.IsNullOrWhiteSpace()== false)
                    {
                        ViewBag.messageFromExcelVerifier = messageFromExcelVerifier;
                    }


                    int pageSize = 10;
                    int pageNumber = (page ?? 1);

                    return View(modifiedData.ToPagedList(pageNumber, pageSize));

                }
            }

        }

    }
}