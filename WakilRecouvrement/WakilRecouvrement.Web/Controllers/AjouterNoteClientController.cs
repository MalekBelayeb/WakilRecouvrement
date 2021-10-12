using Microsoft.Ajax.Utilities;
using MyFinance.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Data;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;

namespace WakilRecouvrement.Web.Controllers
{
    public class AjouterNoteClientController : Controller
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Logger");

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            log.Error(filterContext.Exception);
        }

        public ActionResult AjouterNote(string id, string msgError, string pageSave, string currentSort, string currentFilterNumLot, string currentFilterTraite)
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

                    ViewBag.page = pageSave;
                    ViewBag.CurrentSort = currentSort;
                    ViewBag.currentFilterNumLot = currentFilterNumLot;
                    ViewBag.currentFilterTraite = currentFilterTraite;

                    ViewBag.TraiteList = new SelectList(DropdownListController.TraiteListForDropDownForCreation(), "Value", "Text");
                    ViewBag.id = id;
                    ViewBag.affectation = AffectationService.GetById(long.Parse(id));
                    ViewBag.errormsg = msgError;

                    var client = (from a in AffectationService.GetAll()
                                  join l in LotService.GetAll() on a.LotId equals l.LotId
                                  where a.AffectationId == long.Parse(id)
                                  select new
                                  {
                                      SoldeDeb = l.SoldeDebiteur,
                                      TelPortableFN = l.TelPortableFN,
                                      TelFixeFN = l.TelFixeFN,
                                      TelPortable = l.TelPortable,
                                      TelFixe = l.TelFixe

                                  }).FirstOrDefault();

                    TelFN telfn = new TelFN { TelFixe = client.TelFixe, TelFixeFN = client.TelFixeFN, TelPortable = client.TelPortable, TelPortableFN = client.TelPortableFN };

                    ViewBag.TelFN = telfn;

                    string soldeDeb = client.SoldeDeb;

                    ViewBag.soldeDeb = soldeDeb.IfNullOrWhiteSpace("0").Replace(',', '.');
                    
                    return View(FormulaireService.GetAll().OrderBy(o => o.TraiteLe).ToList().Where(f => f.AffectationId == int.Parse(id)));
                }
            }

        }

        [HttpPost]
        public ActionResult CreerFormulaireNote(string id, string DescriptionAutre, string EtatClient, string RDVDateTime, string RappelDateTime, string soldetranche, HttpPostedFileBase[] PostedFile, string pageSave, string CurrentSortSave, string currentFilterNumLotSave, string currentFilterTraiteSave)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    if (Session["username"] == null || Session["username"].ToString().Length < 1)
                        return RedirectToAction("Login", "Authentification");

                    LotService LotService = new LotService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);
                    AffectationService AffectationService = new AffectationService(UOW);
                    RecuImageService RecuImageService = new RecuImageService(UOW);

                    Debug.WriteLine("PostedFile.Length "+PostedFile.Length);

                    ViewBag.TraiteList = new SelectList(DropdownListController.TraiteListForDropDownForCreation(), "Value", "Text");

                    Formulaire Formulaire = new Formulaire();
                    ViewBag.errormsg = "";

                    switch ((Note)Enum.Parse(typeof(Note), EtatClient))
                    {
                        case Note.INJOIGNABLE:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;

                            Formulaire.EtatClient = Note.INJOIGNABLE;

                            break;
                        case Note.NRP:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;

                            Formulaire.EtatClient = Note.NRP;

                            break;
                        case Note.RACCROCHE:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;

                            Formulaire.EtatClient = Note.RACCROCHE;

                            break;
                        case Note.RDV:

                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.EtatClient = Note.RDV;
                            Formulaire.DateRDV = DateTime.Parse(RDVDateTime);
                            Formulaire.Status = Status.VERIFIE;

                            break;

                        case Note.REFUS_PAIEMENT:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;

                            Formulaire.EtatClient = Note.REFUS_PAIEMENT;

                            break;
                        case Note.SOLDE:

                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.EtatClient = Note.SOLDE;
                            Formulaire.Status = Status.EN_COURS;

                            if (soldetranche.IndexOf('.') != -1)
                            {
                                soldetranche = soldetranche.Replace('.', ',');
                            }

                            Formulaire.MontantVerseDeclare = double.Parse(soldetranche.IfNullOrWhiteSpace(Formulaire.MontantDebInitial + ""));

                            break;
                        case Note.FAUX_NUM:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;

                            Formulaire.EtatClient = Note.FAUX_NUM;

                            break;
                        case Note.A_VERIFIE:

                            Formulaire.AffectationId = int.Parse(id);

                            Formulaire.TraiteLe = DateTime.Now;

                            Formulaire.EtatClient = Note.A_VERIFIE;

                            Formulaire.ContacteBanque = false;

                            Formulaire.Status = Status.EN_COURS;

                            break;
                        case Note.AUTRE:
                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.EtatClient = Note.AUTRE;
                            Formulaire.TraiteLe = DateTime.Now;
                            break;
                        case Note.RAPPEL:

                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.Status = Status.VERIFIE;
                            Formulaire.RappelLe = DateTime.Parse(RappelDateTime);
                            Formulaire.EtatClient = Note.RAPPEL;

                            break;
                        case Note.SOLDE_TRANCHE:

                            Formulaire.AffectationId = int.Parse(id);
                            Formulaire.TraiteLe = DateTime.Now;
                            Formulaire.EtatClient = Note.SOLDE_TRANCHE;
                            Formulaire.Status = Status.EN_COURS;

                            if (soldetranche.IndexOf('.') != -1)
                            {
                                soldetranche = soldetranche.Replace('.', ',');
                            }


                            Formulaire.MontantVerseDeclare = double.Parse(soldetranche.IfNullOrWhiteSpace("0"));

                            break;
                    }

                    Formulaire.DescriptionAutre = DescriptionAutre;
                    Formulaire.NotifieBanque = false;

                    List< ClientAffecteViewModel> formulaireAffectationJoinedList = (from f in FormulaireService.GetAll()
                                                           join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                                           select new ClientAffecteViewModel
                                                           {
                                                               Formulaire = f,
                                                               Affectation = a

                                                           }).ToList();
                  
                    var nbSolde = formulaireAffectationJoinedList.Where(j=> j.Affectation.AffectationId == int.Parse(id) && j.Formulaire.EtatClient == Note.SOLDE).Count();

                    if (nbSolde >= 1)
                    {
                        return RedirectToAction("AjouterNote", new { id = id, msgError = "Client est deja soldé !" });
                    }

                    if (Formulaire.EtatClient == Note.A_VERIFIE)
                    {

                        var nbVerfie = formulaireAffectationJoinedList.Where(j => j.Affectation.AffectationId == int.Parse(id) && j.Formulaire.Status == Status.EN_COURS && j.Formulaire.EtatClient == Note.A_VERIFIE).Count();
                        
                        if (nbVerfie >= 1)
                        {
                            return RedirectToAction("AjouterNote", new { id = id, msgError = "Une verification est deja en attente !" });
                        }
                    }

                    FormulaireService.Add(Formulaire);
                    FormulaireService.Commit();

                    List<ClientAffecteViewModel> updatedFormulaireAffectationJoinedList = (from f in FormulaireService.GetAll()
                                                                                    join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                                                                    select new ClientAffecteViewModel
                                                                                    {
                                                                                        Formulaire = f,
                                                                                        Affectation = a

                                                                                    }).ToList();


                    Lot Lot = (from updatedFAJList in updatedFormulaireAffectationJoinedList
                                     join l in LotService.GetAll() on updatedFAJList.Affectation.LotId equals l.LotId
                                     where updatedFAJList.Formulaire.FormulaireId == Formulaire.FormulaireId
                                     select new ClientAffecteViewModel
                                     {

                                         Lot = l

                                     }).Select(l=>l.Lot).FirstOrDefault();

                    
                    
                    Lot.FormulaireId = Formulaire.FormulaireId;
                    LotService.Update(Lot);

                    if (Lot.SoldeDebiteur == "" || Lot.SoldeDebiteur == null)
                    {
                        Lot.SoldeDebiteur = "0";
                    }

                    if (Lot.SoldeDebiteur.IndexOf('.') != -1)
                    {
                        Lot.SoldeDebiteur = Lot.SoldeDebiteur.Replace('.', ',');
                    }

                    Formulaire.MontantDebInitial = double.Parse(Lot.SoldeDebiteur);

                    if (updatedFormulaireAffectationJoinedList.Where(f => f.Formulaire.AffectationId == int.Parse(id)).Count() == 1)
                    {
                        Formulaire.MontantDebMAJ = double.Parse(Lot.SoldeDebiteur);
                    }
                    else
                    {
                         
                        ClientAffecteViewModel cavm = (from updatedFAJList in updatedFormulaireAffectationJoinedList
                                                       where updatedFAJList.Affectation.AffectationId == int.Parse(id) && updatedFAJList.Formulaire.MontantDebMAJ != 0
                                                       orderby updatedFAJList.Formulaire.MontantDebMAJ ascending
                                                       select new ClientAffecteViewModel
                                                       {
                                                           Formulaire = updatedFAJList.Formulaire,
                                                           Affectation = updatedFAJList.Affectation

                                                       }).FirstOrDefault();

                        if (cavm != null)
                        {
                            Formulaire.MontantDebMAJ = cavm.Formulaire.MontantDebMAJ;
                        }
                        else
                        {
                            Formulaire.MontantDebMAJ = 0;
                        }

                    }

                    Formulaire.AgentUsername = Session["username"] +"";

                    FormulaireService.Update(Formulaire);
                    FormulaireService.Commit();

                    if (PostedFile != null)
                    {
                        if (PostedFile.Length > 0)
                        {

                            foreach (HttpPostedFileBase postedFile in PostedFile)
                            {
                                if (postedFile == null)
                                    return RedirectToAction("MesAffectation", "MesAffectationList", new { traite = currentFilterTraiteSave, numLot = currentFilterNumLotSave, sortOrder = CurrentSortSave, page = pageSave });
                            }

                            string filePath = string.Empty;
                            string path = Server.MapPath("~/Uploads/Recu/");

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            string recuPath = path + Formulaire.FormulaireId;

                            if (!Directory.Exists(recuPath))
                            {
                                Directory.CreateDirectory(recuPath);
                            }

                            foreach (HttpPostedFileBase postedFile in PostedFile)
                            {
                                string filename = Directory.GetFiles(recuPath).Length + 1 + "_" + Lot.IDClient + "_" + Lot.Compte;
                                filePath = recuPath + "/" + filename + Path.GetExtension(postedFile.FileName);
                                postedFile.SaveAs(filePath);
                                RecuImage recuImage = new RecuImage
                                { 
                                    FormulaireId = Formulaire.FormulaireId,
                                    ImageName = filename + Path.GetExtension(postedFile.FileName)
                                };
                                RecuImageService.Add(recuImage);
                            }
                        
                            RecuImageService.Commit();
                            LotService.Commit();
                        
                        }
                    }
                    return RedirectToAction("MesAffectation", "MesAffectationList", new { traite = currentFilterTraiteSave, numLot = currentFilterNumLotSave, sortOrder = CurrentSortSave, page = pageSave });

                }
            }
        }


        [HttpPost]
        public ActionResult UpdateTelFN(int lotId, int affectationId, string portable, string fixe)
        {
            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);

                    bool TelFixe = false;
                    bool TelPortable = false;
                    Lot lot = LotService.GetById(lotId);


                    if (Request.Form["TelPortable"] == "on")
                    {
                        TelPortable = true;
                    }
                    
                    if (Request.Form["TelFixe"] == "on")
                    {
                        TelFixe = true;
                    }

                    lot.TelFixe = fixe;
                    lot.TelPortable = portable;

                    lot.TelFixeFN = TelFixe;
                    lot.TelPortableFN = TelPortable;


                    LotService.Update(lot);
                    LotService.Commit();


                    return RedirectToAction("AjouterNote", "AjouterNoteClient", new { id = affectationId, msgError = "" });

                }
            }
        }

        public ActionResult deleteHist(int idHist, int idAff, string msgError)
        {

            using (WakilRecouvContext WakilContext = new WakilRecouvContext())
            {
                using (UnitOfWork UOW = new UnitOfWork(WakilContext))
                {

                    LotService LotService = new LotService(UOW);
                    AffectationService AffectationService =new AffectationService(UOW);
                    FormulaireService FormulaireService = new FormulaireService(UOW);

                    List<ClientAffecteViewModel> JoinedList = (from f in FormulaireService.GetAll()
                                                               join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                                               join l in LotService.GetAll() on a.LotId equals l.LotId
                                                               where f.AffectationId == idAff
                                                               select new ClientAffecteViewModel
                                                               {

                                                                   Formulaire = f,
                                                                   Affectation = a,
                                                                   Lot = l,
                                                                   AffectationId = a.AffectationId,

                                                               }).ToList();

                    Formulaire Formulaire =JoinedList.Select(j=>j.Formulaire).Where(f=>f.FormulaireId == idHist).FirstOrDefault();

                    int idaffectation = Formulaire.AffectationId;

                    Lot Lot = JoinedList.Select(j => j.Lot).FirstOrDefault();
                    if(Lot!=null)
                    {
                        Lot.FormulaireId = null;

                        LotService.Update(Lot);
                        LotService.Commit();

                        FormulaireService.Delete(Formulaire);
                        FormulaireService.Commit();
                        //ekher formulaire par client
                        List<Formulaire> lastNewFormulaireList = JoinedList.Select(j=>j.Formulaire).Where(f=>f.FormulaireId != Formulaire.FormulaireId).OrderByDescending(f => f.TraiteLe).ToList();
                        
                        Lot updatedLot = Lot;

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
                    }
                   
                    return RedirectToAction("AjouterNote", "AjouterNoteClient", new { id = idAff, msgError = msgError });

                }
            }
        }




    }
}