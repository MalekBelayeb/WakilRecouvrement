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
using Microsoft.Ajax.Utilities;

namespace WakilRecouvrement.Web.Controllers
{
    public class HistoriqueClientController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");
        public ActionResult Historique(int id)
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
                        ViewBag.affectationId = clientAffecteViewModels.Select(c => c.Affectation.AffectationId).FirstOrDefault();


                        LotService.Dispose();
                        FormulaireService.Dispose();
                        AffectationService.Dispose();
                        EmpService.Dispose();


                        return View(clientAffecteViewModels);

                    }
                    catch(Exception e)
                    {
                        
                        log.Error(e);
                        return View("~/Views/Shared/Error.cshtml", null);

                    }

                }
            }

        }
        
        public ActionResult ActualiserClient(string id)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    try
                    {
                        int affectationId = int.Parse(id);

                        LotService LotService = new LotService(UOW);
                        FormulaireService FormulaireService = new FormulaireService(UOW);
                        AffectationService AffectationService = new AffectationService(UOW);

                        var JoinedLot = from f in FormulaireService.GetAll()
                                        join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                        where a.AffectationId == affectationId
                                        select new ClientAffecteViewModel { Formulaire = f };

                        List<Formulaire> formulaireList = new List<Formulaire>();
                        JoinedLot.ForEach(j =>
                        {
                            if(j.Formulaire.Status == Status.VERIFIE)
                            {
                                Formulaire formulaireTemp = j.Formulaire;

                                if (formulaireList.Count == 0)
                                {

                                    Decimal newMontantDebMaj = Convert.ToDecimal(formulaireTemp.MontantDebInitial) - Convert.ToDecimal(formulaireTemp.MontantVerseDeclare);

                                    if ((formulaireTemp.EtatClient == Note.SOLDE || formulaireTemp.EtatClient == Note.SOLDE_TRANCHE))
                                    {

                                        if (newMontantDebMaj > 0)
                                        {
                                            formulaireTemp.EtatClient = Note.SOLDE_TRANCHE;
                                            formulaireTemp.MontantDebMAJ = Math.Abs((double)newMontantDebMaj);

                                        }
                                        else
                                        {
                                            formulaireTemp.EtatClient = Note.SOLDE;
                                            formulaireTemp.MontantDebMAJ = 0;

                                        }
                                    }

                                }
                                else
                                {
                                    double minSoldeDebMAj = formulaireTemp.MontantDebInitial;

                                    try
                                    {

                                        minSoldeDebMAj = formulaireList.Where(minJ => minJ.Status == Status.VERIFIE).Min(minJ => minJ.MontantDebMAJ);
                                        formulaireTemp.MontantDebMAJ = minSoldeDebMAj;

                                    }
                                    catch (Exception e)
                                    {
                                        minSoldeDebMAj = formulaireTemp.MontantDebInitial;
                                    }

                                    Decimal newMontantDebMaj = Convert.ToDecimal(minSoldeDebMAj) - Convert.ToDecimal(formulaireTemp.MontantVerseDeclare);

                                    if ((formulaireTemp.EtatClient == Note.SOLDE || formulaireTemp.EtatClient == Note.SOLDE_TRANCHE))
                                    {

                                        if (newMontantDebMaj > 0)
                                        {
                                            formulaireTemp.EtatClient = Note.SOLDE_TRANCHE;
                                            formulaireTemp.MontantDebMAJ = Math.Abs((double)newMontantDebMaj);

                                        }
                                        else
                                        {
                                            formulaireTemp.EtatClient = Note.SOLDE;
                                            formulaireTemp.MontantDebMAJ = 0;
                                        }
                                    }


                                }

                                FormulaireService.Update(formulaireTemp);

                                formulaireList.Add(j.Formulaire);
                            }
                           
                        });
                        FormulaireService.Commit();

                    }
                    catch (Exception e)
                    {

                    }
            
                }
            }
            
            return RedirectToAction("Historique",new {id = id});
        }


    }
}