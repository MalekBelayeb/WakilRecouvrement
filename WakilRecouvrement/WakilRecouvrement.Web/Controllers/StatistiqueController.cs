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
    public class StatistiqueController : Controller
    {



        public StatistiqueController()
        {


        }

        public IEnumerable<SelectListItem> NumLotListForDropDown()
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                  
                    List<Lot> Lots = LotService.GetAll().ToList();
                    List<SelectListItem> listItems = new List<SelectListItem>();
                    listItems.Add(new SelectListItem { Text = "Tous les lots", Value = "0" });

                    Lots.DistinctBy(l => l.NumLot).ForEach(l =>
                    {
                        listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
                    });

                    return listItems;
                }
            }
        }

        public IEnumerable<SelectListItem> YearListForDropDown()
        {
            int startYear = 2015;

            List<int> years = Enumerable.Range(startYear, DateTime.Now.Year - startYear + 1).Reverse().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            years.ForEach(l => {

                listItems.Add(new SelectListItem { Text = l.ToString(), Value = l.ToString() });

            });

            return listItems;

        }

        public IEnumerable<SelectListItem> MonthListForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Text = "Janvier", Value = "1" });
            listItems.Add(new SelectListItem { Text = "Fevrier", Value = "2" });
            listItems.Add(new SelectListItem { Text = "Mars", Value = "3" });
           
            listItems.Add(new SelectListItem { Text = "Avril", Value = "4" });
            listItems.Add(new SelectListItem { Text = "Mai", Value = "5" });
            listItems.Add(new SelectListItem { Text = "Juin", Value = "6" });
            
            listItems.Add(new SelectListItem { Text = "Juillet", Value ="7" });
            listItems.Add(new SelectListItem { Text = "Aout", Value = "8" });
            listItems.Add(new SelectListItem { Text = "Septembre", Value = "9" });
            
            listItems.Add(new SelectListItem { Text = "Octobre", Value = "10" });
            listItems.Add(new SelectListItem { Text = "Novembre", Value = "11" });
            listItems.Add(new SelectListItem { Text = "Decembre", Value = "12" });

            return listItems;

        }


        public IEnumerable<SelectListItem> TypeStatForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Lot complet", Value = "1" });
            listItems.Add(new SelectListItem {  Text = "Par Date", Value = "2" });
            listItems.Add(new SelectListItem {  Text = "Par Agent", Value = "3" });
            listItems.Add(new SelectListItem {  Text = "Par Date et agent", Value = "4" });


            return listItems;
        }


        public ActionResult Index()
        {

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewData["sortOrder"] = new SelectList(TypeStatForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

            
            
            ViewData["years"] = new SelectList(YearListForDropDown(), "Value", "Text");
            ViewData["month"] = new SelectList(MonthListForDropDown(), "Value", "Text");

            StatLot statLot = new StatLot()
            {
                nb = 0,
                rdv = 0,
                fn = 0,
                versement = 0,
                encours = 0,
                avgRdv = 0+"",
                avgFn = 0+"",
                avgVers = 0+"",
                avgencours = 0+""
            };
            return View(statLot);
        }



        [HttpPost]
        public ActionResult StatLot(int numLot, string typeStat, string dateStat,string agent)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
                    ViewData["sortOrder"] = new SelectList(TypeStatForDropDown(), "Value", "Text");
                    ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

                    int nb = 0;
                    int rdv = 0;
                    int fn = 0;
                    int versement = 0;
                    int encours = 0;

                    string avgRdv = 0 + "";
                    string avgVers = 0 + "";
                    string avgFn = 0 + "";
                    string avgencours = 0 + "";


                    ViewData["years"] = new SelectList(YearListForDropDown(), "Value", "Text");
                    ViewData["month"] = new SelectList(MonthListForDropDown(), "Value", "Text");

                    List<Affectation> affectations = new List<Affectation>();
                    List<Lot> lots = new List<Lot>();
                    List<ClientAffecteViewModel> rdvCAVm = new List<ClientAffecteViewModel>();
                    List<ClientAffecteViewModel> fnCAVm = new List<ClientAffecteViewModel>();
                    List<ClientAffecteViewModel> versCAVm = new List<ClientAffecteViewModel>();
                    List<ClientAffecteViewModel> encoursCAVm = new List<ClientAffecteViewModel>();
                    List<ClientAffecteViewModel> tot = new List<ClientAffecteViewModel>();
                        
                    if (numLot == 0)
                    {

                        tot = (from a in AffectationService.GetAll()
                               join l in LotService.GetAll() on a.LotId equals l.LotId
                               select new ClientAffecteViewModel
                               {

                                   Affectation = a,
                                   Lot = l,

                               }).ToList();


                        rdvCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                   join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                   join l in LotService.GetAll() on a.LotId equals l.LotId

                                   select new ClientAffecteViewModel
                                   {

                                       Formulaire = f,
                                       Affectation = a,
                                       Lot = l,

                                   }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == Note.RDV).ToList();


                        fnCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId

                                  select new ClientAffecteViewModel
                                  {

                                      Formulaire = f,
                                      Affectation = a,
                                      Lot = l,

                                  }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == Note.FAUX_NUM).ToList();


                        versCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                    join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                    join l in LotService.GetAll() on a.LotId equals l.LotId

                                    select new ClientAffecteViewModel
                                    {

                                        Formulaire = f,
                                        Affectation = a,
                                        Lot = l,

                                    }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == Note.SOLDE_TRANCHE || f.Formulaire.EtatClient == Note.SOLDE).ToList();


                        encoursCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                       join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                       join l in LotService.GetAll() on a.LotId equals l.LotId

                                       select new ClientAffecteViewModel
                                       {

                                           Formulaire = f,
                                           Affectation = a,
                                           Lot = l,

                                       }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == Note.REFUS_PAIEMENT || f.Formulaire.EtatClient == Note.RAPPEL || f.Formulaire.EtatClient == Note.RACCROCHE || f.Formulaire.EtatClient == Note.NRP || f.Formulaire.EtatClient == Note.INJOIGNABLE || f.Formulaire.EtatClient == Note.AUTRE || f.Formulaire.EtatClient == Note.A_VERIFIE).ToList();



                    }
                    else
                    {
                        tot = (from a in AffectationService.GetAll()
                               join l in LotService.GetAll() on a.LotId equals l.LotId
                               where l.NumLot == numLot.ToString()
                               select new ClientAffecteViewModel
                               {

                                   Affectation = a,
                                   Lot = l,

                               }).ToList();


                        rdvCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                   join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                   join l in LotService.GetAll() on a.LotId equals l.LotId
                                   where l.NumLot == numLot + ""

                                   select new ClientAffecteViewModel
                                   {

                                       Formulaire = f,
                                       Affectation = a,
                                       Lot = l,

                                   }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == Note.RDV).ToList();


                        fnCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId
                                  where l.NumLot == numLot + ""

                                  select new ClientAffecteViewModel
                                  {

                                      Formulaire = f,
                                      Affectation = a,
                                      Lot = l,

                                  }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == Note.FAUX_NUM).ToList();


                        versCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                    join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                    join l in LotService.GetAll() on a.LotId equals l.LotId
                                    where l.NumLot == numLot + ""

                                    select new ClientAffecteViewModel
                                    {

                                        Formulaire = f,
                                        Affectation = a,
                                        Lot = l,

                                    }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == Note.SOLDE_TRANCHE || f.Formulaire.EtatClient == Note.SOLDE).ToList();


                        encoursCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                                       join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                       join l in LotService.GetAll() on a.LotId equals l.LotId
                                       where l.NumLot == numLot + ""

                                       select new ClientAffecteViewModel
                                       {

                                           Formulaire = f,
                                           Affectation = a,
                                           Lot = l,

                                       }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == Note.REFUS_PAIEMENT || f.Formulaire.EtatClient == Note.RAPPEL || f.Formulaire.EtatClient == Note.RACCROCHE || f.Formulaire.EtatClient == Note.NRP || f.Formulaire.EtatClient == Note.INJOIGNABLE || f.Formulaire.EtatClient == Note.AUTRE || f.Formulaire.EtatClient == Note.A_VERIFIE).ToList();

                    }

                    if (typeStat == "1")
                    {
                        nb = tot.Count();
                        rdv = rdvCAVm.Count();
                        fn = fnCAVm.Count();
                        versement = versCAVm.Count();
                        encours = encoursCAVm.Count();

                        avgRdv = String.Format("{0:0.00}", ((float)rdv / (float)nb) * 100);
                        avgFn = String.Format("{0:0.00}", ((float)fn / (float)nb) * 100);
                        avgVers = String.Format("{0:0.00}", ((float)versement / (float)nb) * 100);
                        avgencours = String.Format("{0:0.00}", ((float)encours / (float)nb) * 100);

                    }
                    else if (typeStat == "2")
                    {

                        rdv = rdvCAVm.Where(f => f.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                        fn = fnCAVm.Where(f => f.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                        versement = versCAVm.Where(f => f.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                        encours = encoursCAVm.Where(f => f.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();

                        nb = rdv + fn + versement + encours;

                        avgRdv = String.Format("{0:0.00}", ((float)rdv / (float)nb) * 100);
                        avgFn = String.Format("{0:0.00}", ((float)fn / (float)nb) * 100);
                        avgVers = String.Format("{0:0.00}", ((float)versement / (float)nb) * 100);
                        avgencours = String.Format("{0:0.00}", ((float)encours / (float)nb) * 100);
                    }
                    else if (typeStat == "3")
                    {

                        rdv = rdvCAVm.Where(a => a.Affectation.EmployeId + "" == agent).Count();
                        fn = fnCAVm.Where(a => a.Affectation.EmployeId + "" == agent).Count();
                        versement = versCAVm.Where(a => a.Affectation.EmployeId + "" == agent).Count();
                        encours = encoursCAVm.Where(a => a.Affectation.EmployeId + "" == agent).Count();

                        nb = rdv + fn + versement + encours;

                        avgRdv = String.Format("{0:0.00}", ((float)rdv / (float)nb) * 100);
                        avgFn = String.Format("{0:0.00}", ((float)fn / (float)nb) * 100);
                        avgVers = String.Format("{0:0.00}", ((float)versement / (float)nb) * 100);
                        avgencours = String.Format("{0:0.00}", ((float)encours / (float)nb) * 100);
                    }
                    else if (typeStat == "4")
                    {

                        rdv = rdvCAVm.Where(a => a.Affectation.EmployeId + "" == agent && a.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                        fn = fnCAVm.Where(a => a.Affectation.EmployeId + "" == agent && a.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                        versement = versCAVm.Where(a => a.Affectation.EmployeId + "" == agent && a.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                        encours = encoursCAVm.Where(a => a.Affectation.EmployeId + "" == agent && a.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();

                        nb = rdv + fn + versement + encours;

                        avgRdv = String.Format("{0:0.00}", ((float)rdv / (float)nb) * 100);
                        avgFn = String.Format("{0:0.00}", ((float)fn / (float)nb) * 100);
                        avgVers = String.Format("{0:0.00}", ((float)versement / (float)nb) * 100);
                        avgencours = String.Format("{0:0.00}", ((float)encours / (float)nb) * 100);
                    }

                    StatLot statLot = new StatLot()
                    {
                        nb = nb,
                        rdv = rdv,
                        fn = fn,
                        versement = versement,
                        encours = encours,
                        avgRdv = avgRdv.Replace(",", "."),
                        avgFn = avgFn.Replace(",", "."),
                        avgVers = avgVers.Replace(",", "."),
                        avgencours = avgencours.Replace(",", ".")
                    };

                    return View("Index", statLot);
                }
            }
        }


        public IEnumerable<SelectListItem> AgentListForDropDown()
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                      EmployeService EmpService = new EmployeService(UOW);

                    List<Employe> agents = EmpService.GetMany(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true).ToList();
                    List<SelectListItem> listItems = new List<SelectListItem>();


                    agents.ForEach(l =>
                    {
                        listItems.Add(new SelectListItem { Text = l.Username, Value = l.EmployeId + "" });
                    });

                    return listItems;
                }
            }
        }

        [HttpGet]
        public ActionResult annuelStatTraite(string year)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    int annuelPoste1Tot = 0;
                    int annuelPoste2Tot = 0;
                    int annuelPoste3Tot = 0;
                    int annuelPoste4Tot = 0;

                    double annuelRentaPoste1Tot = 0;
                    double annuelRentaPoste2Tot = 0;
                    double annuelRentaPoste3Tot = 0;
                    double annuelRentaPoste4Tot = 0;

                    int tot = 0;
                    double totRenta = 0;

                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    JoinedList = (from f in FormulaireService.GetAll()
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId

                                  select new ClientAffecteViewModel
                                  {

                                      Formulaire = f,
                                      Affectation = a,
                                      Lot = l,

                                  }).ToList();

                    if(year != null && year!="")
                    {
                        annuelPoste1Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE1" && j.Formulaire.TraiteLe.Date.Year + "" == year).Count();
                        annuelPoste2Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE2" && j.Formulaire.TraiteLe.Date.Year + "" == year).Count();
                        annuelPoste3Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE3" && j.Formulaire.TraiteLe.Date.Year + "" == year).Count();
                        annuelPoste4Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE4" && j.Formulaire.TraiteLe.Date.Year + "" == year).Count();

                        annuelRentaPoste1Tot = rentabiliteAgents(year, "POSTE1", LotService, FormulaireService, AffectationService);
                        annuelRentaPoste2Tot = rentabiliteAgents(year, "POSTE2", LotService, FormulaireService, AffectationService);
                        annuelRentaPoste3Tot = rentabiliteAgents(year, "POSTE3", LotService, FormulaireService, AffectationService);
                        annuelRentaPoste4Tot = rentabiliteAgents(year, "POSTE4", LotService, FormulaireService, AffectationService);

                    }

                    tot = annuelPoste1Tot + annuelPoste2Tot + annuelPoste3Tot + annuelPoste4Tot;
                    totRenta = annuelRentaPoste1Tot + annuelRentaPoste2Tot + annuelRentaPoste3Tot + annuelRentaPoste4Tot;

                    return Json(new { tot = tot, annuelPoste1Tot = annuelPoste1Tot, annuelPoste2Tot = annuelPoste2Tot, annuelPoste3Tot = annuelPoste3Tot, annuelPoste4Tot = annuelPoste4Tot, totRenta = String.Format("{0:0.000}", totRenta), annuelRentaPoste1Tot = annuelRentaPoste1Tot, annuelRentaPoste2Tot = annuelRentaPoste2Tot, annuelRentaPoste3Tot = annuelRentaPoste3Tot, annuelRentaPoste4Tot = annuelRentaPoste4Tot }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public List<ClientAffecteViewModel> getTraitHist(int idAff, FormulaireService FormulaireService, AffectationService AffectationService, LotService LotService)
        {
            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            JoinedList = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where f.Status == Domain.Entities.Status.VERIFIE && f.EtatClient == Domain.Entities.Note.SOLDE_TRANCHE && f.AffectationId == idAff

                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).OrderByDescending(f => f.Formulaire.TraiteLe).ToList();

            return JoinedList;


        }

        public double rentabiliteAgents(string year,string agent, LotService LotService, FormulaireService FormulaireService, AffectationService AffectationService)
        {

            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> TempJoinedList = new List<ClientAffecteViewModel>();

            double trancheTot = 0;
            double soldeTot = 0;
            double res=0; 
            JoinedList = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where f.Status == Domain.Entities.Status.VERIFIE
                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).Where(j => j.Affectation.Employe.Username == agent && j.Formulaire.TraiteLe.Date.Year + "" == year).ToList();



            foreach (ClientAffecteViewModel cvm in JoinedList)
            {

                if (cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE_TRANCHE)
                {

                    trancheTot += (cvm.Formulaire.MontantVerseDeclare);

                }

                if (cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE)
                {

                    TempJoinedList = getTraitHist(cvm.Affectation.AffectationId, FormulaireService, AffectationService, LotService);
                    if (TempJoinedList.Count() == 0)
                    {

                        soldeTot += (cvm.Formulaire.MontantDebInitial);

                    }
                    else
                    {

                        soldeTot += (TempJoinedList.FirstOrDefault().Formulaire.MontantDebMAJ);

                    }

                }


            }

            res = soldeTot + trancheTot;

            return res;

        }
        public double rentabiliteAgents(string year,string month, string agent, LotService LotService, FormulaireService FormulaireService, AffectationService AffectationService)
        {

            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> TempJoinedList = new List<ClientAffecteViewModel>();

            double trancheTot = 0;
            double soldeTot = 0;
            double res = 0;
            JoinedList = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where f.Status == Domain.Entities.Status.VERIFIE
                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).Where(j => j.Affectation.Employe.Username == agent && j.Formulaire.TraiteLe.Date.Year + "" == year && j.Formulaire.TraiteLe.Date.Month + "" == month).ToList();



            foreach (ClientAffecteViewModel cvm in JoinedList)
            {

                if (cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE_TRANCHE)
                {

                    trancheTot += (cvm.Formulaire.MontantVerseDeclare);

                }

                if (cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE)
                {

                    TempJoinedList = getTraitHist(cvm.Affectation.AffectationId, FormulaireService, AffectationService, LotService);
                    if (TempJoinedList.Count() == 0)
                    {

                        soldeTot += (cvm.Formulaire.MontantDebInitial);

                    }
                    else
                    {

                        soldeTot += (TempJoinedList.FirstOrDefault().Formulaire.MontantDebMAJ);

                    }

                }


            }

            res = soldeTot + trancheTot;

            return res;

        }


        public double rentabiliteAgents(DateTime date, string agent, LotService LotService, FormulaireService FormulaireService, AffectationService AffectationService)
        {

            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> TempJoinedList = new List<ClientAffecteViewModel>();

            double trancheTot = 0;
            double soldeTot = 0;
            double res = 0;
            JoinedList = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where f.Status == Domain.Entities.Status.VERIFIE
                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).Where(j => j.Affectation.Employe.Username == agent && j.Formulaire.TraiteLe.Date  == date ).ToList();



            foreach (ClientAffecteViewModel cvm in JoinedList)
            {

                if (cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE_TRANCHE)
                {

                    trancheTot += (cvm.Formulaire.MontantVerseDeclare);

                }

                if (cvm.Formulaire.EtatClient == Domain.Entities.Note.SOLDE)
                {

                    TempJoinedList = getTraitHist(cvm.Affectation.AffectationId, FormulaireService, AffectationService, LotService);
                    if (TempJoinedList.Count() == 0)
                    {

                        soldeTot += (cvm.Formulaire.MontantDebInitial);

                    }
                    else
                    {

                        soldeTot += (TempJoinedList.FirstOrDefault().Formulaire.MontantDebMAJ);

                    }

                }


            }

            res = soldeTot + trancheTot;

            return res;

        }

        [HttpPost]
        public ActionResult mensuelStatTraite(string year,string month)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    EmployeService EmpService = new EmployeService(UOW);

                    int mensuelPoste1Tot = 0;
                    int mensuelPoste2Tot = 0;
                    int mensuelPoste3Tot = 0;
                    int mensuelPoste4Tot = 0;
                    int tot = 0;
                    
                    double mensuelRentaPoste1Tot = 0;
                    double mensuelRentaPoste2Tot = 0;
                    double mensuelRentaPoste3Tot = 0;
                    double mensuelRentaPoste4Tot = 0;
                    double totRenta = 0;
                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    JoinedList = (from f in FormulaireService.GetAll()
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId

                                  select new ClientAffecteViewModel
                                  {

                                      Formulaire = f,
                                      Affectation = a,
                                      Lot = l,

                                  }).ToList();



                    if(month!="" && year!="" && month !=null && year != null)
                    {
                        mensuelPoste1Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE1" && j.Formulaire.TraiteLe.Date.Year + "" == year && j.Formulaire.TraiteLe.Date.Month + "" == month).Count();
                        mensuelPoste2Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE2" && j.Formulaire.TraiteLe.Date.Year + "" == year && j.Formulaire.TraiteLe.Date.Month + "" == month).Count();
                        mensuelPoste3Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE3" && j.Formulaire.TraiteLe.Date.Year + "" == year && j.Formulaire.TraiteLe.Date.Month + "" == month).Count();
                        mensuelPoste4Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE4" && j.Formulaire.TraiteLe.Date.Year + "" == year && j.Formulaire.TraiteLe.Date.Month + "" == month).Count();
                        tot = mensuelPoste1Tot + mensuelPoste2Tot + mensuelPoste3Tot + mensuelPoste4Tot;

                        mensuelRentaPoste1Tot = rentabiliteAgents(year, month, "POSTE1", LotService, FormulaireService, AffectationService);
                        mensuelRentaPoste2Tot = rentabiliteAgents(year, month, "POSTE2", LotService, FormulaireService, AffectationService);
                        mensuelRentaPoste3Tot = rentabiliteAgents(year, month, "POSTE3", LotService, FormulaireService, AffectationService);
                        mensuelRentaPoste4Tot = rentabiliteAgents(year, month, "POSTE4", LotService, FormulaireService, AffectationService);
                        totRenta = mensuelRentaPoste1Tot + mensuelRentaPoste2Tot + mensuelRentaPoste3Tot + mensuelRentaPoste4Tot;

                    }


                    return Json(new { tot = tot, mensuelPoste1Tot = mensuelPoste1Tot, mensuelPoste2Tot = mensuelPoste2Tot, mensuelPoste3Tot = mensuelPoste3Tot, mensuelPoste4Tot = mensuelPoste4Tot, totRenta = String.Format("{0:0.00}", totRenta) , mensuelRentaPoste1Tot = mensuelRentaPoste1Tot, mensuelRentaPoste2Tot = mensuelRentaPoste2Tot, mensuelRentaPoste3Tot = mensuelRentaPoste3Tot, mensuelRentaPoste4Tot = mensuelRentaPoste4Tot }) ;
                }
            }
        }

        [HttpPost]
        public ActionResult quotidienStatTraite(string date)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                   
                    DateTime d = new DateTime();

                    int quotidienPoste1Tot = 0;
                    int quotidienPoste2Tot = 0;
                    int quotidienPoste3Tot = 0;
                    int quotidienPoste4Tot = 0;
                    int tot = 0;

                    double quotidienRentaPoste1Tot = 0;
                    double quotidienRentaPoste2Tot = 0;
                    double quotidienRentaPoste3Tot = 0;
                    double quotidienRentaPoste4Tot = 0;
                    double totRenta = 0;
                    List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

                    JoinedList = (from f in FormulaireService.GetAll()
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId

                                  select new ClientAffecteViewModel
                                  {

                                      Formulaire = f,
                                      Affectation = a,
                                      Lot = l,

                                  }).ToList();

                    if(date != "" && date !=null)
                    {
                        if (DateTime.TryParse(date, out d))
                        {
                            quotidienPoste1Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE1" && j.Formulaire.TraiteLe.Date == d.Date).Count();
                            quotidienPoste2Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE2" && j.Formulaire.TraiteLe.Date == d.Date).Count();
                            quotidienPoste3Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE3" && j.Formulaire.TraiteLe.Date == d.Date).Count();
                            quotidienPoste4Tot = JoinedList.Where(j => j.Affectation.Employe.Username == "POSTE4" && j.Formulaire.TraiteLe.Date == d.Date).Count();
                            tot = quotidienPoste1Tot + quotidienPoste2Tot + quotidienPoste3Tot + quotidienPoste4Tot;


                            quotidienRentaPoste1Tot = rentabiliteAgents(d, "POSTE1", LotService, FormulaireService, AffectationService);
                            quotidienRentaPoste2Tot = rentabiliteAgents(d, "POSTE2", LotService, FormulaireService, AffectationService);
                            quotidienRentaPoste3Tot = rentabiliteAgents(d, "POSTE3", LotService, FormulaireService, AffectationService);
                            quotidienRentaPoste4Tot = rentabiliteAgents(d, "POSTE4", LotService, FormulaireService, AffectationService);
                            totRenta = quotidienRentaPoste1Tot + quotidienRentaPoste2Tot + quotidienRentaPoste3Tot + quotidienRentaPoste4Tot;
                        
                        }
                    }
                   


                    return Json(new { tot = tot, quotidienPoste1Tot = quotidienPoste1Tot, quotidienPoste2Tot = quotidienPoste2Tot, quotidienPoste3Tot = quotidienPoste3Tot, quotidienPoste4Tot = quotidienPoste4Tot,totRenta= String.Format("{0:0.00}", totRenta),quotidienRentaPoste1Tot=quotidienRentaPoste1Tot,quotidienRentaPoste2Tot=quotidienRentaPoste2Tot,quotidienRentaPoste3Tot=quotidienRentaPoste3Tot,quotidienRentaPoste4Tot=quotidienRentaPoste4Tot });
                }
            }
        }


    }
}
