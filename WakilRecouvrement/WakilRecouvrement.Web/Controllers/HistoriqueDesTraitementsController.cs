using MyFinance.Data.Infrastructure;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Service;
using Microsoft.Ajax.Utilities;

namespace WakilRecouvrement.Web.Controllers
{
    public class HistoriqueDesTraitementsController : Controller
    {

        public bool IsDeletable(int idForm, int lastIdFormulaire)
        {

            if (lastIdFormulaire == idForm)
            {
                return true;
            }
            return false;
        }

        public ActionResult HistoriqueTraitements(string numLot, string currentNumLot, string SearchString, string currentFilter, string traite, string currentTraite, string agent, string currentAgent, string traitDate, string currentTraitDate, string type, string currentType, string currentPage, int? page)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    ViewData["list"] = new SelectList(DropdownListController.NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.AgentList = new SelectList(DropdownListController.AgentListForDropDown(EmpService), "Value", "Text");
                    ViewBag.TraiteList = new SelectList(DropdownListController.TraiteListSuiviTraitHistoriqueForDropDown(), "Value", "Text");
                    ViewBag.typeTrait = new SelectList(DropdownListController.typeListForDropDown(), "Value", "Text");

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    List<ClientAffecteViewModel> JoinedList;

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
                                          where (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE) && f.Status == Status.VERIFIE
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = f.AgentUsername,
                                              AffectationId = a.AffectationId,
                                              IsDeletable = IsDeletable(f.FormulaireId, l.FormulaireId ?? 0)

                                          }).OrderByDescending(f => f.Formulaire.VerifieLe).ToList();
                        }

                        else
                        {
                            JoinedList = (from f in FormulaireService.GetAll()
                                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                          join l in LotService.GetAll() on a.LotId equals l.LotId
                                          where (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE) && f.Status == Status.VERIFIE
                                          select new ClientAffecteViewModel
                                          {

                                              Formulaire = f,
                                              Affectation = a,
                                              Lot = l,
                                              Agent = f.AgentUsername,
                                              AffectationId = a.AffectationId, 
                                              IsDeletable = IsDeletable(f.FormulaireId, l.FormulaireId ?? 0)

                                          }).Where(f => f.Formulaire.EtatClient + "" == traite).OrderByDescending(f => f.Formulaire.VerifieLe).ToList();
                        }

                    }
                    else
                    {

                        JoinedList = (from f in FormulaireService.GetAll()
                                      join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                      join l in LotService.GetAll() on a.LotId equals l.LotId
                                      where (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE) && f.Status == Status.VERIFIE
                                      select new ClientAffecteViewModel
                                      {

                                          Formulaire = f,
                                          Affectation = a,
                                          Lot = l,
                                          Agent = f.AgentUsername,
                                          AffectationId = a.AffectationId,
                                          IsDeletable = IsDeletable(f.FormulaireId, l.FormulaireId ?? 0)

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
                        if(agent != "0")
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


                    ViewBag.total = JoinedList.Count();

                    int pageSize = 10;
                    int pageNumber = (page ?? 1);

                    return View(JoinedList.ToPagedList(pageNumber, pageSize));
                }
            }

        }

        public ActionResult deleteHistVerifie(int id, string currentFilter, string currentNumLot, string currentType, string currentTraite, string currentAgent, string currentTraitDate, string currentPage)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {
                    LotService LotService = new LotService(UOW);
                    
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    Formulaire Formulaire = FormulaireService.GetById(id);
                    int idaffectation = Formulaire.AffectationId;

                    Lot Lot = LotService.Get(l => l.FormulaireId == id);
                    
                    if(Lot!=null)
                    {
                        Lot.FormulaireId = null;

                        LotService.Update(Lot);
                        LotService.Commit();

                        FormulaireService.Delete(Formulaire);
                        FormulaireService.Commit();
                    }
                    

                    List<Formulaire> lastNewFormulaireList = FormulaireService.GetMany(f => f.AffectationId == idaffectation).OrderByDescending(f => f.TraiteLe).ToList();
                    Lot updatedLot = LotService.GetById(Lot.LotId);

                    if (lastNewFormulaireList.Count() > 0)
                    {
                        updatedLot.FormulaireId = lastNewFormulaireList.FirstOrDefault().FormulaireId;

                    }
                    else
                    {
                        updatedLot.FormulaireId = null;

                    }
                    LotService.Update(updatedLot);
                    LotService.Commit();

                    return RedirectToAction("HistoriqueTraitements", new { currentFilter, currentNumLot, currentType, currentTraite, currentAgent, currentTraitDate, currentPage });

                }
            }
        }


    }
}