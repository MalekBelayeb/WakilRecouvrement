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
    public class MesRappelsListController : Controller
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");



        // GET: MesRappelsList
        public ActionResult MesRappels(string numLot, string currentFilterNumLot, string CurrentSort, string RappelDate, string sortOrder, int? page)
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
                                              AffectationId = a.AffectationId,
                                              Formulaire = f

                                          }).OrderByDescending(o => o.Formulaire.TraiteLe).Where(j => verifMesRappels(j.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).ToList();

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
                                                  AffectationId = a.AffectationId,
                                                  Formulaire = f

                                              }).OrderByDescending(o => o.Formulaire.TraiteLe).Where(j => verifMesRappels(j.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).ToList();
                            }

                        }



                        ViewData["list"] = new SelectList(DropdownListController.NumLotListForDropDown(LotService), "Value", "Text");
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

        public bool verifMesRappels(int affId, DateTime formTraiteLe, FormulaireService FormulaireService)
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
    }
}