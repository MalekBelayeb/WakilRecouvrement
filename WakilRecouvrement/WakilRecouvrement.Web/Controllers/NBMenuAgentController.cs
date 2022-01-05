using MyFinance.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Service;

namespace WakilRecouvrement.Web.Controllers
{
    public class NBMenuAgentController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");

        [HttpPost]
        public ActionResult SuiviRDVNB()
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

                        int nb = 0;
                        int rappelNB = 0;
                        int rejetes = 0;

                        List<Formulaire> formulaires = FormulaireService.GetAll().ToList();

                        if (formulaires == null)
                        {
                            nb = 0;
                            rappelNB = 0;
                            rejetes = 0;
                        }
                        else
                        {
                            nb = (from f in formulaires
                                  where f.AgentUsername == Session["username"] + "" && f.EtatClient == Note.RDV && f.DateRDV.Date == DateTime.Now.Date
                                  orderby f.TraiteLe descending
                                  select new ClientAffecteViewModel
                                  {
                                      Formulaire = f,
                                  }).Count();


                            rappelNB = (from f in formulaires
                                        where f.AgentUsername == Session["username"] + "" && f.EtatClient == Note.RAPPEL && f.RappelLe.Date == DateTime.Now.Date
                                        select new ClientAffecteViewModel
                                        {
                                            Formulaire = f

                                        }).Count();


                            rejetes = (from f in formulaires
                                       where f.AgentUsername == Session["username"] + "" && f.Status == Status.NON_VERIFIE
                                       select new ClientAffecteViewModel
                                       {
                                           Formulaire = f

                                       }).Count();



                            LotService.Dispose();
                            FormulaireService.Dispose();
                            AffectationService.Dispose();

                        }


                        return Json(new { nb = nb, rappelNB = rappelNB, rejetes = rejetes });



                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        return Json(new { nb = 0, rappelNB = 0, rejetes = 0 });

                    }

                }
            }
        }



    }
}