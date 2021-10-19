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

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            log.Error(filterContext.Exception);
        }


        [HttpPost]
        public ActionResult SuiviRDVNB()
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);

                    int nb = 0;
                    int rappelNB = 0;
                    int rejetes = 0;
                    List<Formulaire> formulaires = FormulaireService.GetAll().ToList();

                    nb = (from f in formulaires
                          where f.AgentUsername.Equals(Session["username"]) && f.EtatClient == Note.RDV && f.DateRDV.Date == DateTime.Now.Date
                          orderby f.TraiteLe descending
                          select new ClientAffecteViewModel
                          {
                              Formulaire = f,
                          }).Where(j => verifMesRDV(j.Formulaire.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).Count();



                    rappelNB = (from f in formulaires
                                where f.AgentUsername.Equals(Session["username"]) && f.EtatClient == Note.RAPPEL && f.RappelLe.Date == DateTime.Now.Date
                                select new ClientAffecteViewModel
                                {
                                    Formulaire = f

                                }).Where(j => verifMesRappels(j.Formulaire.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).Count();


                    rejetes = (from f in formulaires
                               where f.AgentUsername.Equals(Session["username"]) && f.Status == Status.NON_VERIFIE
                               select new ClientAffecteViewModel
                               {
                                   Formulaire = f

                               }).Where(j => verifMesRappels(j.Formulaire.AffectationId, j.Formulaire.TraiteLe, FormulaireService)).Count();


                    return Json(new { nb = nb, rappelNB = rappelNB, rejetes = rejetes });
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