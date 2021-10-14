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
using Microsoft.Ajax.Utilities;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Web.Controllers
{
    public class SuiviMesTraitementController : Controller
    {


        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            log.Error(filterContext.Exception);
        }

        public ActionResult SuiviTraitement(string numLot, string currentNumLot, string SearchString, string currentFilter, string traite, string currentTraite, string agent, string currentAgent, string traitDate, string currentTraitDate, string type, string currentType, int? page)
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

                    ViewData["list"] = new SelectList(DropdownListController.NumLotListForDropDown(LotService), "Value", "Text");

                    if (Session["role"].Equals("admin"))
                        ViewBag.AgentList = new SelectList(DropdownListController.AgentListForDropDown(EmpService), "Value", "Text");


                    ViewBag.TraiteList = new SelectList(DropdownListController.TraiteListSuiviTraitForDropDown(), "Value", "Text");
                    ViewBag.typeTrait = new SelectList(DropdownListController.typeListForDropDown(), "Value", "Text");


                    List<Affectation> affectations = new List<Affectation>();
                    
                    if (Session["role"].Equals("admin"))
                    {
                        if (!String.IsNullOrEmpty(agent))
                        {
                            if(agent == "0")
                            {
                                affectations = AffectationService.GetAll().ToList();

                            }
                            else
                            {
                                Employe emp = EmpService.GetEmployeByUername(agent + "");

                                affectations = AffectationService.GetMany(a => a.EmployeId == emp.EmployeId).ToList();
                            }
                           
                        
                        
                        }
                    }
                    else
                    {
                        Employe emp = EmpService.GetEmployeByUername(Session["username"] + "");

                        affectations = AffectationService.GetMany(a => a.EmployeId == emp.EmployeId).ToList();
                    }


                    if (!String.IsNullOrEmpty(traite))
                    {

                        if (traite == "ALL")
                        {

                            JoinedList = (from f in FormulaireService.GetAll()
                                          join a in affectations on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId

                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = f.AgentUsername,
                                              AffectationId = a.AffectationId

                                          }).OrderByDescending(f => f.Formulaire.TraiteLe).ToList();

                        }

                        else
                        {
                            JoinedList = (from f in FormulaireService.GetAll()
                                          join a in affectations on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId

                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = f.AgentUsername,
                                              AffectationId = a.AffectationId

                                          }).Where(f => f.Formulaire.EtatClient + "" == traite).OrderByDescending(f => f.Formulaire.TraiteLe).ToList();
                        }

                    }
                    else
                    {

                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in affectations on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId

                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Affectation = a,
                                          Lot = l,
                                          Agent = f.AgentUsername,
                                          AffectationId = a.AffectationId

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