using MyFinance.Data.Infrastructure;
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
    public class HistoriqueClientController : Controller
    {
        public ActionResult Historique(int id)
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
                                                                            join l in LotService.GetAll() on a.LotId equals l.LotId

                                                                            where a.AffectationId == id
                                                                            select new ClientAffecteViewModel
                                                                            {
                                                                                Formulaire = f,
                                                                                Affectation = a,
                                                                                Lot = l,
                                                                                Agent = f.AgentUsername

                                                                            }).ToList();

                    ViewBag.username = clientAffecteViewModels.Select(c => c.Agent).FirstOrDefault();
                    ViewBag.lot = clientAffecteViewModels.Select(c => c.Lot).FirstOrDefault();
                    ViewBag.id = id + "";

                    return View(clientAffecteViewModels);

                }
            }

        }
    }
}