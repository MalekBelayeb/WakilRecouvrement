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
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Web.Controllers
{
    public class MesRDVController : Controller
    {
        public ActionResult SuiviRDV(string numLot, string RDVType, string RdvDate, string sortOrder, string currentFilterNumLot, string currentFilterRDVType, string CurrentSort, int? page)
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

                    ViewData["list"] = new SelectList(DropdownListController.NumLotListForDropDown(LotService), "Value", "Text");
                    ViewBag.RDVList = new SelectList(DropdownListController. RDVForDropDown(), "Value", "Text");
                    ViewData["sortOrder"] = new SelectList(DropdownListController.SortOrderSuiviRDVForDropDown(), "Value", "Text");


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


        public bool verifMesRDV(int affId, DateTime formTraiteLe, FormulaireService FormulaireService)
        {

            int res = FormulaireService.GetMany(f => f.AffectationId == affId && f.TraiteLe > formTraiteLe && (f.EtatClient == Note.A_VERIFIE || f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE || f.EtatClient == Note.RAPPEL)).Count();

            if (res == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}