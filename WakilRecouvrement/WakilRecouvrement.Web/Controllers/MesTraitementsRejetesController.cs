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
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Web.Controllers
{
    public class MesTraitementsRejetesController : Controller
    {


        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");


        public ActionResult TraitementRejetesList(string SearchString, string currentFilter, string sortOrder, int? page)
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
                                      where a.Employe.Username.Equals(Session["username"]) && f.Status == Status.NON_VERIFIE orderby f.TraiteLe descending

                                      select new ClientAffecteViewModel
                                      {
                                          Lot = l,
                                          Affectation = a,
                                          Formulaire = f

                                      }).ToList();

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