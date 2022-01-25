using MyFinance.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models.ViewModel;
using System.Linq;
using WakilRecouvrement.Domain.Entities;
using System.Diagnostics;
using Newtonsoft.Json;
using WakilRecouvrement.Web.Models;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Ajax.Utilities;
using System.IO;

namespace WakilRecouvrement.Web.Controllers
{

    public class ValiderTraitementDetailController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");


        public string GetEtat(Formulaire formulaire)
        {

            if (formulaire == null)
            {
                return "";
            }
            else
            {
                return formulaire.IfNotNull(i => i.EtatClient).ToString();
            }

        }

        public ActionResult TraiterDetail(int idaffectation,int idformulaire)
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
                        RecuImageService RecuImageService = new RecuImageService(UOW);

                        ValiderTraitementViewModel validerTraitementViewModel = new ValiderTraitementViewModel();
                        List<string> recuImages = new List<string>();


                        List<TraiterDetailViewModel> traiterDetailViewModelList = (from f in FormulaireService.GetAll()
                                                                                   join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                                                                   join l in LotService.GetAll() on a.LotId equals l.LotId

                                                                                   where a.AffectationId == idaffectation
                                                                                   select new TraiterDetailViewModel
                                                                                   {
                                                                                       Formulaire = f,
                                                                                       Affectation = a,
                                                                                       Lot = l
                                                                                   }).ToList();
                       


                        ValiderTraitementViewModel ValiderTraitementViewModel = (from t in traiterDetailViewModelList
                                                                                 where t.Formulaire.FormulaireId == idformulaire
                                                                                 select new ValiderTraitementViewModel
                                                                                 {
                                                                                     Lot = t.Lot,
                                                                                     Username = t.Formulaire.AgentUsername,
                                                                                     VerifieLe = t.Formulaire.VerifieLe.ToString(),
                                                                                     DateAff = "",
                                                                                     TraiteLe = t.Formulaire.TraiteLe.ToString("dd/MM/yyyy HH:mm:ss"),
                                                                                     Etat = GetEtat(t.Formulaire).ToString(),
                                                                                     FormulaireId = t.Formulaire.FormulaireId,
                                                                                     ContactBanque = t.Formulaire.ContacteBanque,
                                                                                     MontantVerseDeclare = t.Formulaire.MontantVerseDeclare,
                                                                                     descAutre = t.Formulaire.DescriptionAutre,
                                                                                     AffectationId = t.Formulaire.AffectationId
                                                                                 }).FirstOrDefault();


                        ViewBag.ValiderTraitementViewModel = ValiderTraitementViewModel;

                        recuImages = getRecuImagesPath(idformulaire, RecuImageService);

                        int nbRecuImage = recuImages.Count();

                        if (nbRecuImage > 0)
                        {
                            try
                            {
                                ViewBag.recuImages = JsonConvert.SerializeObject(recuImages);
                            }
                            catch (Exception e)
                            {
                                ViewBag.recuImages = null;
                            }
                        }
                        else
                        {
                            ViewBag.recuImages = null;
                        }
                        ViewBag.nbRecuImage = nbRecuImage;



                        LotService.Dispose();
                        FormulaireService.Dispose();
                        AffectationService.Dispose();
                        EmpService.Dispose();
                        RecuImageService.Dispose();

                        return View(traiterDetailViewModelList);


                    }
                    catch(Exception e)
                    {
                        log.Error(e);
                        return View("~/Views/Shared/Error.cshtml", null);
                    }

               

                }
            }
        }

        public List<string> getRecuImagesPath(int FormulaireId, RecuImageService RecuImageService)
        {
            List<string> imageNames = RecuImageService.GetMany(r => r.FormulaireId == FormulaireId).Select(r => r.ImageName).ToList();
            Debug.WriteLine("----"+ FormulaireId);
            string path = Server.MapPath("~/Uploads/Recu/") + FormulaireId;
            
            string uri = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Host + ":" + HttpContext.Request.Url.Port + "/WakilRecouvrement/" + "/Uploads/Recu/" + FormulaireId + "/";

            List<string> urlImages = new List<string>();
            if (Directory.Exists(path))
            {
                
                foreach(string imgName in imageNames)
                {
                    Debug.WriteLine(imgName);
                    urlImages.Add(uri + imgName);

                }

            }
            Debug.WriteLine(urlImages.Count());

            return urlImages;
        }

        public ActionResult ValiderEtat(string id, string valid, string montantInput)
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

                        var JoinedLot = from f in FormulaireService.GetAll()
                                        join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                        join l in LotService.GetAll() on a.LotId equals l.LotId
                                        where f.FormulaireId == int.Parse(id)
                                        select new ClientAffecteViewModel { Lot = l, Formulaire = f };

                        Lot Lot = JoinedLot.Select(j => j.Lot).FirstOrDefault();

                        Formulaire Formulaire = JoinedLot.Select(j => j.Formulaire).FirstOrDefault();

                        if (Formulaire != null)
                        {

                            double DebMaJ = (from f in FormulaireService.GetAll()
                                             join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                             where a.AffectationId == Formulaire.AffectationId
                                             orderby f.MontantDebMAJ ascending
                                             select f.MontantDebMAJ
                                     ).FirstOrDefault();


                            if (int.Parse(valid) == 0)
                            {


                                if (Formulaire.Status == Status.EN_COURS)
                                {

                                    Formulaire.Status = Status.NON_VERIFIE;
                                    FormulaireService.Update(Formulaire);
                                    //FormulaireService.Delete(Formulaire);
                                    FormulaireService.Commit();

                                }

                                return RedirectToAction("TraiterDetail", new { idaffectation = JoinedLot.Select(j => j.Formulaire.AffectationId).FirstOrDefault(), idformulaire = JoinedLot.Select(j => j.Formulaire.FormulaireId).FirstOrDefault() });
                            }

                            double.TryParse(Lot.SoldeDebiteur.Replace('.', ','), out double SoldeDebiteur);
                            Debug.WriteLine(SoldeDebiteur);
                            Decimal NewSolde = 0;

                            switch (Formulaire.EtatClient)
                            {
                                case Note.SOLDE:

                                    NewSolde = Decimal.Subtract(decimal.Parse(DebMaJ.ToString()), decimal.Parse(Formulaire.MontantVerseDeclare.ToString()));

                                    if (NewSolde <= 0)
                                    {

                                        Formulaire.MontantDebMAJ = 0;
                                        Formulaire.Status = Status.VERIFIE;
                                        Formulaire.VerifieLe = DateTime.Now;

                                    }
                                    break;

                                case Note.SOLDE_TRANCHE:

                                    NewSolde = Decimal.Subtract(decimal.Parse(DebMaJ.ToString()), decimal.Parse(Formulaire.MontantVerseDeclare.ToString()));

                                    if (NewSolde > 0)
                                    {

                                        Formulaire.MontantDebMAJ = double.Parse(NewSolde.ToString());

                                        Formulaire.Status = Status.VERIFIE;

                                        Formulaire.VerifieLe = DateTime.Now;

                                    }
                                    else if (NewSolde <= 0)
                                    {

                                        Formulaire.MontantDebMAJ = 0;

                                        Formulaire.Status = Status.VERIFIE;
                                        Formulaire.VerifieLe = DateTime.Now;
                                        Formulaire.EtatClient = Note.SOLDE;

                                    }

                                    break;

                                case Note.A_VERIFIE:

                                    if (montantInput.IsNullOrWhiteSpace() == false)
                                    {
                                        double.TryParse(montantInput.Replace('.', ','), out double montant);
                                        Formulaire.MontantVerseDeclare = montant;
                                    }
                                    else
                                    {
                                        Formulaire.MontantVerseDeclare = 0;
                                    }

                                    NewSolde = Decimal.Subtract(decimal.Parse(DebMaJ.ToString()), decimal.Parse(Formulaire.MontantVerseDeclare.ToString()));

                                    if (NewSolde <= 0)
                                    {

                                        Formulaire.MontantDebMAJ = 0;

                                        Formulaire.Status = Status.VERIFIE;
                                        Formulaire.VerifieLe = DateTime.Now;
                                        Formulaire.EtatClient = Note.SOLDE;

                                    }
                                    else if (NewSolde > 0)
                                    {
                                        Formulaire.MontantDebMAJ = double.Parse(NewSolde.ToString());

                                        Formulaire.Status = Status.VERIFIE;
                                        Formulaire.VerifieLe = DateTime.Now;
                                        Formulaire.EtatClient = Note.SOLDE_TRANCHE;

                                    }


                                    break;
                            }


                            FormulaireService.Update(Formulaire);
                            FormulaireService.Commit();

                        }



                        LotService.Dispose();
                        FormulaireService.Dispose();
                        AffectationService.Dispose();

                        return RedirectToAction("TraiterDetail", new { idaffectation = JoinedLot.Select(j => j.Formulaire.AffectationId).FirstOrDefault(), idformulaire = JoinedLot.Select(j => j.Formulaire.FormulaireId).FirstOrDefault() });



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