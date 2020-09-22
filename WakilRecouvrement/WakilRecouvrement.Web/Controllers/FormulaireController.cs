using ClosedXML.Excel;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.EnterpriseServices.Internal;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;
using Excel = Microsoft.Office.Interop.Excel;

namespace WakilRecouvrement.Web.Controllers
{
    public class FormulaireController : Controller
    {

        AffectationService AffectationService;
        LotService LotService;
        EmployeService EmpService;
        FormulaireService FormulaireService;
        NotificationService NotificationService;
   
        public int id = 0;

        public FormulaireController()
        {
            AffectationService = new AffectationService();
            LotService = new LotService();
            EmpService = new EmployeService();
            FormulaireService = new FormulaireService();
            NotificationService = new NotificationService();

        }

        public ActionResult CreerFormulaire(string id)
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            ViewBag.TraiteList = new SelectList(TraiteListForDropDownForCreation(), "Value", "Text");
            ViewBag.id = id;
            ViewBag.affectation = AffectationService.GetById(long.Parse(id));
            string soldeDeb = AffectationService.GetById(long.Parse(id)).Lot.SoldeDebiteur;
            ViewBag.soldeDeb = soldeDeb.Replace(',', '.');
            return View(FormulaireService.GetAll().OrderByDescending(o=>o.TraiteLe).ToList().Where(f=>f.AffectationId == int.Parse(id)));
        }

        [HttpPost]
        public ActionResult CreerFormulaireIntermediate(string id)
        {
            return Json(new { redirectUrl = Url.Action("CreerFormulaire", "Formulaire", new { id = id }) });

        }


        public ActionResult SuiviClient()
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteListForDropDown(), "Value", "Text");

            return View();
        }

        public IEnumerable<SelectListItem> NumLotListForDropDown()
        {


            List<Lot> Lots = LotService.GetAll().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Touts les lots", Value = "0" });

            Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
            });

            return listItems;
        }

        public IEnumerable<SelectListItem> AgentListForDropDown()
        {

            List<Employe> agents = EmpService.GetAll().Where(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true).ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Touts les agents", Value = "0" });

            agents.ForEach(l => {
                listItems.Add(new SelectListItem { Text = l.Username, Value = l.EmployeId+"" });
            });

            return listItems;
        }
        public IEnumerable<SelectListItem> TraiteListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Touts les clients affectés", Value = "ALL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Touts clients traités sauf SOLDE/FAUX_NUM", Value = "SAUF" });

            foreach (var n in Enum.GetValues(typeof(Note)))
            {
               
                    listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString()  });
              
            }
  

            return listItems;
        }

        public IEnumerable<SelectListItem> TraiteListForDropDownForCreation()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();

            foreach (var n in Enum.GetValues(typeof(Note)))
            {

                listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString() });

            }


            return listItems;
        }


        [HttpPost]
        public ActionResult LoadData(string numLot,string agent, string traite )
        {
            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> JoinedList2 = new List<ClientAffecteViewModel>();

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteListForDropDown(), "Value", "Text");



            JoinedList2 = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId

                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).ToList().OrderByDescending(o => o.Formulaire.TraiteLe).DistinctBy(d => d.Formulaire.AffectationId).ToList();


           // GenerateExcel(GenerateDatatableFromJoinedList(JoinedList2), @"C:\Users\Admin\Downloads\test.xlsx");

            if (traite == "ALL")
            {
                JoinedList = (from a in AffectationService.GetAll()
                              join l in LotService.GetAll() on a.LotId equals l.LotId

                              select new ClientAffecteViewModel
                              {
                                  Formulaire = FormulaireService.GetOrderedFormulaireByAffectation(a.AffectationId),
                                  Affectation = a,
                                  Lot = l,

                              }).ToList().DistinctBy(a => a.Affectation.AffectationId).ToList();



            
            }

            else if (traite == "SAUF")
            {

                JoinedList = (from f in FormulaireService.GetAll()
                              join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                              join l in LotService.GetAll() on a.LotId equals l.LotId
                              where f.EtatClient != (Note)Enum.Parse(typeof(Note), "SOLDE") && f.EtatClient != (Note)Enum.Parse(typeof(Note), "FAUX_NUM")

                              select new ClientAffecteViewModel
                              {

                                  Formulaire = f,
                                  Affectation = a,
                                  Lot = l,

                              }).ToList().OrderByDescending(o => o.Formulaire.TraiteLe).DistinctBy(d => d.Formulaire.AffectationId).ToList();






            }
            else
            {

                JoinedList = (from f in FormulaireService.GetAll()
                              join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                              join l in LotService.GetAll() on a.LotId equals l.LotId
                              orderby f.TraiteLe descending
                              select new ClientAffecteViewModel
                              {

                                  Formulaire = f,
                                  Affectation = a,
                                  Lot = l,

                              }).ToList().DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), traite)).ToList();

            }

            if (numLot != "0")
            {
                JoinedList = JoinedList.ToList().Where(j => j.Lot.NumLot.Equals(numLot)).ToList();
            }



            if(int.Parse(agent)!=0)
            {
                JoinedList = JoinedList.ToList().Where(j => j.Affectation.EmployeId == int.Parse(agent)).ToList();
            }

            int nbTotal = JoinedList.Count();

            int nbSoldeTotal = 0;
            int nbTrancheSoldeTotal = 0;
            int nbFNTotal = 0;

          /*  if (listAffectation.Select(a=>a.Formulaires).Count()>0)
            {
                 nbSoldeTotal = listAffectation.Select(a => a.Formulaires).Where(a => a.LastOrDefault().EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE")).ToList().Count();
                 nbTrancheSoldeTotal = listAffectation.Where(a => a.Formulaires.Count() > 0).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE_TRANCHE")).ToList().Count();
                 nbFNTotal = listAffectation.Where(a => a.Formulaires.Count() > 0).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "FAUX_NUM")).ToList().Count();
            }
            */
            
            JsonResult result = new JsonResult();

            try
            {

                string search = Request.Form.GetValues("search[value]")[0];
                string draw = Request.Form.GetValues("draw")[0];
                string order = Request.Form.GetValues("order[0][column]")[0];
                string orderDir = Request.Form.GetValues("order[0][dir]")[0];
                int startRec = Convert.ToInt32(Request.Form.GetValues("start")[0]);
                int pageSize = Convert.ToInt32(Request.Form.GetValues("length")[0]);


                int totalRecords = JoinedList.Count();

                if (!string.IsNullOrEmpty(search) &&
                    !string.IsNullOrWhiteSpace(search))
                {
                    JoinedList = JoinedList.Where(j =>

                        j.Lot.Numero.ToString().Contains(search)
                    || j.Lot.Adresse.ToString().ToLower().Contains(search.ToLower())
                    || j.Lot.TelFixe.ToString().Contains(search)
                    || j.Lot.TelPortable.ToString().Contains(search)
                    || j.Lot.IDClient.ToString().Contains(search)
                    || j.Lot.Compte.ToString().Contains(search)
                    || j.Lot.LotId.ToString().Contains(search)
                    || j.Lot.NomClient.ToString().ToLower().Contains(search.ToLower())
                    || j.Lot.DescIndustry.ToString().ToLower().Contains(search.ToLower())

                        ).ToList();
                }


                JoinedList = SortTableData(order, orderDir, JoinedList);

                int recFilter = JoinedList.Count();

                JoinedList = JoinedList.Skip(startRec).Take(pageSize).ToList();
              
                
                var modifiedData = JoinedList.Select(j =>
                   new
                   {
                       j.Lot.LotId,
                       j.Lot.NumLot,
                       j.Lot.Compte,
                       j.Lot.IDClient,
                       j.Lot.NomClient,
                       j.Lot.TelPortable,
                       j.Lot.TelFixe,
                       j.Lot.SoldeDebiteur,
                       j.Lot.DescIndustry,
                       j.Lot.Adresse,
                       j.Lot.Type,
                       j.Lot.Numero,
                       j.Affectation.Employe.Username,
                       j.Affectation.AffectationId,
                       DateAff = j.Affectation.DateAffectation.ToString(),
                        
                       Etat = GetEtat(j.Formulaire).ToString()
                   
                   }
                   );

                var info = new { nbTotal = nbTotal, nbSoldeTotal = nbSoldeTotal, nbTrancheSoldeTotal = nbTrancheSoldeTotal, nbFNTotal = nbFNTotal };

                result = this.Json(new
                {

                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecords,
                    recordsFiltered = recFilter,
                    data = modifiedData,
                    info = info
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            return result;

        }


        private List<ClientAffecteViewModel> SortTableData(string order, string orderDir, List<ClientAffecteViewModel> data)
        {
            List<ClientAffecteViewModel> lst = new List<ClientAffecteViewModel>();
            try
            {
                switch (order)
                {
                    case "0":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.LotId).ToList()
                                                                                                 : data.OrderBy(l => l.Lot.LotId).ToList();
                        break;

                    case "1":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => long.Parse(j.Lot.NumLot)).ToList()
                                                                                                 : data.OrderBy(j => long.Parse(j.Lot.NumLot)).ToList();
                        break;
                    case "2":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j=> j.Lot.Compte).ToList()
                                                                                                 : data.OrderBy(j => j.Lot.Compte).ToList();
                        break;
                    case "3":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => long.Parse(j.Lot.IDClient)).ToList()
                                                                                                 : data.OrderBy(j => long.Parse(j.Lot.IDClient)).ToList();
                        break;
                    case "4":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.NomClient).ToList()
                                                                                                 : data.OrderBy(j => j.Lot.NomClient).ToList();
                        break;
                    case "5":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.TelPortable).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.TelPortable).ToList();
                        break;
                    case "6":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.TelFixe).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.TelFixe).ToList();
                        break;
                    case "7":

                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList()
                                                                                              : data.OrderBy(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList();

                        break;
                    case "8":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.DescIndustry).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.DescIndustry).ToList();
                        break;
                    case "9":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.Adresse).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.Adresse).ToList();
                        break;
                    case "10":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.Type).ToList()
                                                                                                   : data.OrderBy(j=> j.Lot.Type).ToList();
                        break;

                    case "11":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.Numero).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.Numero).ToList();
                        break;

                    default:
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.LotId).ToList()
                                                                                                 : data.OrderBy(j => j.Lot.LotId).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return lst;
        }

        [HttpPost]
        public ActionResult CreerFormulaireNote(string id,string DescriptionAutre,string EtatClient,string RDVDateTime, string RDVReporteDateTime,string soldetranche)
        {
            ViewBag.TraiteList = new SelectList(TraiteListForDropDownForCreation(), "Value", "Text");

            Formulaire Formulaire = new Formulaire();

            switch ((Note)Enum.Parse(typeof(Note), EtatClient))
            {
                case Note.INJOIGNABLE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.TraiteLe = DateTime.Now;
                    Formulaire.IsVerified = true;

                    Formulaire.EtatClient = Note.INJOIGNABLE;
                    
                    break;
                case Note.NRP:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.TraiteLe = DateTime.Now;
                    Formulaire.IsVerified = true;

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
                    Formulaire.IsVerified = true;

                    break;
                case Note.RDV_REPORTE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.RDV_REPORTE;
                    Formulaire.TraiteLe = DateTime.Now;
                    Formulaire.DateRDVReporte = DateTime.Parse(RDVReporteDateTime);
                    Formulaire.IsVerified = true;


                    break; 
                case Note.REFUS_PAIEMENT:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.TraiteLe = DateTime.Now;
                    Formulaire.IsVerified = true;

                    Formulaire.EtatClient = Note.REFUS_PAIEMENT;

                    break;
                case Note.SOLDE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.TraiteLe = DateTime.Now;
                    Formulaire.EtatClient = Note.SOLDE;

                    if (soldetranche.IndexOf('.') != -1)
                    {
                        soldetranche = soldetranche.Replace('.', ',');
                    }


                    Formulaire.MontantVerseDeclare = double.Parse(soldetranche);

                
                    break;
                case Note.FAUX_NUM:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.TraiteLe = DateTime.Now;
                    Formulaire.IsVerified = true;

                    Formulaire.EtatClient = Note.FAUX_NUM;

                    break;
                case Note.A_VERIFIE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.TraiteLe = DateTime.Now;

                    Formulaire.EtatClient = Note.A_VERIFIE;


                    if (soldetranche.IndexOf('.') != -1)
                    {
                        soldetranche = soldetranche.Replace('.', ',');
                    }

                    Formulaire.ContacteBanque = false;

                    Formulaire.MontantVerseDeclare = double.Parse(soldetranche);

                
                    break;
                case Note.AUTRE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.AUTRE;
                    Formulaire.TraiteLe = DateTime.Now;

                    Formulaire.DescriptionAutre = DescriptionAutre;

                    break;                
                case Note.RAPPEL:

                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.TraiteLe = DateTime.Now;
                    Formulaire.IsVerified = true;

                    Formulaire.EtatClient = Note.RAPPEL;
                 
                    /*EditAppSettings("ExecuteTaskServiceCallSchedulingStatus", "ON");
                    EditAppSettings("ExecuteTaskScheduleCronExpression", "0 0/1 * 1/1 * ? *");
                   */
                    break;
                case Note.SOLDE_TRANCHE:

                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.TraiteLe = DateTime.Now;
                    Formulaire.EtatClient = Note.SOLDE_TRANCHE;
                   
                    if (soldetranche.IndexOf('.') != -1)
                    {
                        soldetranche = soldetranche.Replace('.', ',');
                    }

                    Formulaire.MontantVerseDeclare = double.Parse(soldetranche);

                
                    break;
            }
            Formulaire.NotifieBanque = false;
            FormulaireService.Add(Formulaire);
            FormulaireService.Commit();



            switch(Formulaire.EtatClient)
            {
                case Note.SOLDE:
                    NotificationService.Add(new Notification { Message = "Nouveau client soldé avec un versement de " + Formulaire.MontantVerseDeclare + " TND", FormulaireId = Formulaire.FormulaireId, ToSingle = "", ToRole = "admin", Type = "SOLDE", From = Session["username"] + "", Status = "UNSEEN", AddedIn = DateTime.Now });

                    NotificationService.Commit();

                    break;

                case Note.SOLDE_TRANCHE:
                    NotificationService.Add(new Notification { Message = "Nouvelle tranche de somme " + Formulaire.MontantVerseDeclare + " TND a été versé ", FormulaireId = Formulaire.FormulaireId, ToSingle = "", ToRole = "admin", Type = "SOLDE_TRANCHE", From = Session["username"] + "", Status = "UNSEEN", AddedIn = DateTime.Now });

                    NotificationService.Commit();

                    break;

                case Note.A_VERIFIE:

                    NotificationService.Add(new Notification { Message = "Un client a declaré avoir versé une somme de " + Formulaire.MontantVerseDeclare + " TND", FormulaireId = Formulaire.FormulaireId, ToSingle = "", ToRole = "admin", Type = "A_VERIFIE", From = Session["username"] + "", Status = "UNSEEN", AddedIn = DateTime.Now });

                    NotificationService.Commit();

                    break;


            }

            return RedirectToAction("AffectationList", "Affectation");
        }

        [HttpPost]
        public ActionResult GetFormulaires(int id)
        {

            var joinedAffectation = FormulaireService.GetOrderedFormulaireByAffectationList(id).Select(j => new ClientAffecteViewModel { Formulaire = j });

            var list = joinedAffectation.Select(f => new
            {
                etat = f.Formulaire.EtatClient.ToString(),
                d1 = f.Formulaire.DateRDV.ToString(),
                d2 = f.Formulaire.DateRDVReporte.ToString(),
                tranche = f.Formulaire.MontantVerseDeclare.ToString(),
                desc = f.Formulaire.DescriptionAutre,
                verif = f.Formulaire.IsVerified,
                traitele = f.Formulaire.TraiteLe.ToString()
            }); 

            return Json(new { list=list });
        }

        public string GetEtat(Formulaire formulaire)
        {


            if(formulaire==null)
            {
                return "";
            }
            else
            {
                    
                return formulaire.IfNotNull(i => i.EtatClient).ToString();
            
            }

        }
        public string GetTraiteLe(Affectation affectation)
        {
            if (affectation.Formulaires.Count() == 0)
            {
                return "";
            }
            else
            {

                return affectation.Formulaires.LastOrDefault().IfNotNull(i => i.TraiteLe).ToString();

            }

        }
    
        public ActionResult ValiderTraitement()
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");
            
            if(TempData["IDClient"]==null)
            {
                ViewBag.IDClient = "0";
            }
            else
            {
                ViewBag.IDClient = TempData["IDClient"];
            }

            return View();
        }

        [HttpPost]
        public ActionResult ValiderTraitement(bool IsValid,string numLot,string traite,string agent)
        {
            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");

            if (IsValid ==false )
                ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
            if (IsValid == true)
                ViewBag.TraiteList = new SelectList(TraiteValidationValideListForDropDown(), "Value", "Text");

            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

            if (traite.Equals("ALL"))
            {
                JoinedList = (from f in FormulaireService.GetAll()
                              join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                              join l in LotService.GetAll() on a.LotId equals l.LotId

                              select new ClientAffecteViewModel
                              {
                                  Formulaire = f,
                                  Lot = l,
                                  Affectation = a

                              }).ToList().Where(j => j.Formulaire.IsVerified == IsValid).Where(j => j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE_TRANCHE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "A_VERIFIE")).ToList();

            }
            else
            {
                JoinedList = (from f in FormulaireService.GetAll()
                              join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                              join l in LotService.GetAll() on a.LotId equals l.LotId

                              select new ClientAffecteViewModel
                              {
                                  Formulaire = f,
                                  Lot = l,
                                  Affectation = a

                              }).ToList().Where(j => j.Formulaire.IsVerified == IsValid).Where(j => j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), traite)).ToList();
            }

            if (numLot != "0")
            {
                JoinedList = JoinedList.Where(j => j.Lot.NumLot.Equals(numLot)).ToList();

            }

            if (int.Parse(agent) != 0)
            {

                JoinedList = JoinedList.Where(j => j.Affectation.EmployeId == int.Parse(agent)).ToList();

            }


         

            JsonResult result = new JsonResult();

            try
            {
            
                string search = Request.Form.GetValues("search[value]")[0];
                string draw = Request.Form.GetValues("draw")[0];
                string order = Request.Form.GetValues("order[0][column]")[0];
                string orderDir = Request.Form.GetValues("order[0][dir]")[0];
                int startRec = Convert.ToInt32(Request.Form.GetValues("start")[0]);
                int pageSize = Convert.ToInt32(Request.Form.GetValues("length")[0]);

                int totalRecords = JoinedList.Count();

                if (!string.IsNullOrEmpty(search) &&
                    !string.IsNullOrWhiteSpace(search))
                {
                    JoinedList = JoinedList.Where(j =>

                        j.Lot.Numero.ToString().Contains(search)
                    || j.Lot.Adresse.ToString().ToLower().Contains(search.ToLower())
                    || j.Lot.IDClient.ToString().Contains(search)
                    || j.Lot.Compte.ToString().Contains(search)
                    || j.Lot.NomClient.ToString().ToLower().Contains(search.ToLower())
                    || j.Lot.DescIndustry.ToString().ToLower().Contains(search.ToLower())
                    || j.Formulaire.MontantVerseDeclare.ToString().ToLower().Contains(search.ToLower())

                        ).ToList();
                }

                if (IsValid == false)
                    JoinedList = SortTableDataForValidate(order, orderDir, JoinedList);
                if (IsValid == true)
                    JoinedList = SortTableDataForHistory(order, orderDir, JoinedList);

                int recFilter = JoinedList.Count();

                JoinedList = JoinedList.Skip(startRec).Take(pageSize).ToList();

                var modifiedData = JoinedList.Select(j =>
                   new
                   {

                       j.Lot.NumLot,
                       j.Formulaire.MontantVerseDeclare,
                       j.Lot.Compte,
                       j.Lot.IDClient,
                       j.Lot.NomClient,
                       j.Lot.SoldeDebiteur,
                       j.Lot.DescIndustry,
                       j.Affectation.Employe.Username,
                       j.Affectation.AffectationId,
                       VerifieLe = j.Formulaire.VerifieLe.ToString(),
                       DateAff = j.Affectation.DateAffectation.ToString(),
                       TraiteLe = GetTraiteLe(j.Affectation).ToString(),
                       Etat = GetEtat(j.Formulaire).ToString(),
                       FormulaireId = j.Formulaire.FormulaireId,
                       ContactBanque = j.Formulaire.ContacteBanque,

                   }
                   );
                int x = JoinedList.Count();

                var info = new { nbTotal =  x};

                result = this.Json(new
                {

                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecords,
                    recordsFiltered = recFilter,
                    data = modifiedData,
                    info = info

                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            return result;

        }

        public IEnumerable<SelectListItem> TraiteValidationListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Touts les traitements non validés", Value = "ALL" });
            listItems.Add(new SelectListItem {  Text = "Soldé", Value = "SOLDE" });
            listItems.Add(new SelectListItem {  Text = "Tranche", Value = "SOLDE_TRANCHE" });
            listItems.Add(new SelectListItem {  Text = "A verifié", Value = "A_VERIFIE" });

            return listItems;
        }


        public IEnumerable<SelectListItem> EnvoyerTraiteListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Text = "RDV", Value = "RDV" });
            listItems.Add(new SelectListItem { Text = "Versement", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "Verification", Value = "A_VERIFIE" });
            listItems.Add(new SelectListItem { Text = "En cours de traitement", Value = "Autre" });

            return listItems;
        }

        public IEnumerable<SelectListItem> TraiteValidationValideListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Touts les traitements validés", Value = "ALL" });
            listItems.Add(new SelectListItem { Text = "Soldé", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "Tranche", Value = "SOLDE_TRANCHE" });
            listItems.Add(new SelectListItem { Text = "A verifié", Value = "A_VERIFIE" });

            return listItems;
        }

        private List<ClientAffecteViewModel> SortTableDataForValidate(string order, string orderDir, List<ClientAffecteViewModel> data)
        {
            List<ClientAffecteViewModel> lst = new List<ClientAffecteViewModel>();
            try
            {
                switch (order)
                {
                  
                    case "0":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => long.Parse(j.Lot.NumLot)).ToList()
                                                                                                 : data.OrderBy(j => long.Parse(j.Lot.NumLot)).ToList();
                        break;
                    case "3":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Formulaire.MontantVerseDeclare).ToList()
                                                                                                 : data.OrderBy(j => j.Formulaire.MontantVerseDeclare).ToList();
                        break;

                    case "4":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => GetTraiteLe(j.Affectation)).ToList()
                                                                                                 : data.OrderBy(j => GetTraiteLe(j.Affectation)).ToList();
                        break;
                    case "5":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Affectation.DateAffectation).ToList()
                                                                                                 : data.OrderBy(j => j.Affectation.DateAffectation).ToList();
                        break;
                    case "6":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Affectation.EmployeId).ToList()
                                                                                                 : data.OrderBy(j => j.Affectation.EmployeId).ToList();
                        break;
                   
                    case "7":

                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList()
                                                                                              : data.OrderBy(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList();

                        break;
                    case "10":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.NomClient).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.NomClient).ToList();
                        break;
                                   
                    default:
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => GetTraiteLe(j.Affectation)).ToList()
                                                                                                   : data.OrderBy(j => GetTraiteLe(j.Affectation)).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return lst;
        }


        private List<ClientAffecteViewModel> SortTableDataForHistory(string order, string orderDir, List<ClientAffecteViewModel> data)
        {
            List<ClientAffecteViewModel> lst = new List<ClientAffecteViewModel>();
            try
            {
                switch (order)
                {

                    case "0":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => long.Parse(j.Lot.NumLot)).ToList()
                                                                                                 : data.OrderBy(j => long.Parse(j.Lot.NumLot)).ToList();
                        break;
                    case "2":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Formulaire.MontantVerseDeclare).ToList()
                                                                                                 : data.OrderBy(j => j.Formulaire.MontantVerseDeclare).ToList();
                        break;
                    case "3":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j =>j.Formulaire.VerifieLe).ToList()
                                                                                                 : data.OrderBy(j => j.Formulaire.VerifieLe).ToList();
                        break;
                    case "4":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => GetTraiteLe(j.Affectation)).ToList()
                                                                                                 : data.OrderBy(j => GetTraiteLe(j.Affectation)).ToList();
                        break;
                    case "5":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Affectation.DateAffectation).ToList()
                                                                                                 : data.OrderBy(j => j.Affectation.DateAffectation).ToList();
                        break;
                    case "6":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Affectation.EmployeId).ToList()
                                                                                                 : data.OrderBy(j => j.Affectation.EmployeId).ToList();
                        break;

                    case "7":

                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList()
                                                                                              : data.OrderBy(j => Double.Parse(j.Lot.SoldeDebiteur)).ToList();

                        break;
                    case "10":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.NomClient).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.NomClient).ToList();
                        break;        
                    case "11":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Lot.DescIndustry).ToList()
                                                                                                   : data.OrderBy(j => j.Lot.DescIndustry).ToList();
                        break;

                    default:
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j =>j.Formulaire.VerifieLe).ToList()
                                                                                                   : data.OrderBy(j => j.Formulaire.VerifieLe).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return lst;
        }

        public ActionResult HistoriqueTraitements()
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteValidationValideListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

            return View();
        }


        [HttpPost]
        public ActionResult VerifierEtat(int id,bool valid)
        {

                var JoinedLot = from f in FormulaireService.GetAll()
                join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                join l in LotService.GetAll() on a.LotId equals l.LotId
                where f.FormulaireId == id
                    select new ClientAffecteViewModel {Lot = l,Formulaire = f };
            
         
            Lot Lot = JoinedLot.ToList().FirstOrDefault().Lot;
            Formulaire Formulaire = JoinedLot.ToList().FirstOrDefault().Formulaire;
            
            double SoldeDebiteur = double.Parse(Lot.SoldeDebiteur);
            Decimal NewSolde = 0;
           
            NewSolde = Decimal.Subtract(decimal.Parse(SoldeDebiteur.ToString()), decimal.Parse(Formulaire.MontantVerseDeclare.ToString()));


            switch (Formulaire.EtatClient)
            {
                case Note.SOLDE:

                    if( NewSolde <=0 )
                    {

                        Lot.SoldeDebiteur = 0 + "";

                        Formulaire.IsVerified = valid;
                        Formulaire.VerifieLe = DateTime.Now;

                        LotService.Update(Lot);
                        LotService.Commit();

                        NotificationService.Delete(NotificationService.GetAll().Where(e => e.FormulaireId == Formulaire.FormulaireId).FirstOrDefault());
                        NotificationService.Commit();
                    }
                    break;

                case Note.SOLDE_TRANCHE:

                    if (NewSolde > 0 )
                    {
                    
                        Lot.SoldeDebiteur = NewSolde+"";
                        Formulaire.IsVerified = valid;
                        Formulaire.VerifieLe = DateTime.Now;

                        LotService.Update(Lot);
                        LotService.Commit();

                        NotificationService.Delete(NotificationService.GetAll().Where(e => e.FormulaireId == Formulaire.FormulaireId).FirstOrDefault());
                        NotificationService.Commit();

                    }

                    break;

                case Note.A_VERIFIE:
                   
                    if(NewSolde<=0)
                    {
                        Lot.SoldeDebiteur = 0 + "";

                        Formulaire.IsVerified = valid;
                        Formulaire.VerifieLe = DateTime.Now;
                        Formulaire.EtatClient = Note.SOLDE;

                        LotService.Update(Lot);
                        LotService.Commit();

                       
                    }
                    else if(NewSolde>0)
                    {
                        Lot.SoldeDebiteur = NewSolde + "";
                        Formulaire.IsVerified = valid;
                        Formulaire.VerifieLe = DateTime.Now;
                        Formulaire.EtatClient = Note.SOLDE_TRANCHE;

                        LotService.Update(Lot);
                        LotService.Commit();

                    }

                    NotificationService.Delete(NotificationService.GetAll().Where(e=>e.FormulaireId == Formulaire.FormulaireId).FirstOrDefault());
                    NotificationService.Commit();

                    break;
            }

            FormulaireService.Update(Formulaire);
            FormulaireService.Commit();
          
            return Json(new { });
        }


        [HttpPost]
        public ActionResult OnNotificationClickIntermediate()
        {

            Formulaire formulaire = FormulaireService.GetById(long.Parse(HttpContext.Request.Form["id"].ToString()));
            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");
            ViewBag.searching = "1";

            Affectation affectation = AffectationService.GetById(formulaire.AffectationId);

            //ViewBag.searching = "1";
            TempData["IDClient"] = affectation.Lot.IDClient;
            
            return Json(new { redirectUrl = Url.Action("ValiderTraitement", "Formulaire") });

        } 
        
        [HttpPost]
        public ActionResult UpdateContactBanque(int id)
        {
            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");


            Formulaire formulaire = FormulaireService.GetById(id);
            Debug.WriteLine(id);    
            formulaire.ContacteBanque = true;

            FormulaireService.Update(formulaire);
            FormulaireService.Commit();

            return Json(new { });

        }

        [HttpPost]
        public ActionResult SendGmail(int id)
        {
            Formulaire formulaire = FormulaireService.GetById(id);
            Affectation affectation = AffectationService.GetById(formulaire.AffectationId);
            string to = "zaitounabank@gmail.com";
            string subject = "Demande de verification";
            string body = "Un client portant l'id "+affectation.Lot.IDClient+" declare avoir versé une somme de "+ formulaire.MontantVerseDeclare + " TND sans justifié avec un recu, priere de verifier";

            MailMessage mm = new MailMessage();
            mm.From = new MailAddress("alwakilrecouvrementmailtest@gmail.com");
            mm.To.Add(to);
            mm.Subject = subject;
            mm.Body = body;
            mm.IsBodyHtml = false;


            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.UseDefaultCredentials = false;
            smtp.Port = 587;
            smtp.EnableSsl = true;
           
            smtp.Credentials = new NetworkCredential("alwakilrecouvrementmailtest@gmail.com", "wakil123456");
            smtp.Send(mm);
           

            return Json(new { });
        }



        public DataTable GenerateDatatableFromJoinedList(List<ClientAffecteViewModel> list,string traite)
        {
            List<FormulaireExportable> newList = new List<FormulaireExportable>();
            DataTable dataTable = new DataTable();

            if (traite.Equals("RDV"))
            {
               newList = list.Select(j =>
                new FormulaireExportable
                {
                NomClient = j.Lot.NomClient,
                NumLot = j.Lot.NumLot,
                Compte = j.Lot.Compte,
                IDClient = j.Lot.IDClient,
                Etat = j.Formulaire.EtatClient.ToString(),
                RDV = j.Formulaire.DateRDV.ToString()
                }

                ).ToList();

                dataTable.Columns.Add("NumLot", typeof(string));
                dataTable.Columns.Add("IDClient", typeof(string));
                dataTable.Columns.Add("Compte", typeof(string));
                dataTable.Columns.Add("NomClient", typeof(string));
                dataTable.Columns.Add("Etat", typeof(string));
                dataTable.Columns.Add("RDV", typeof(string));
                
                foreach (FormulaireExportable c in newList)
                {

                    string etat = "";

                    if (c.Etat == "SOLDE")
                    {
                        etat = "Client soldé";

                    }
                    else if (c.Etat == "SOLDE_TRANCHE")
                    {

                        etat = "tranche versé";

                    }
                    else if (c.Etat == "RDV")
                    {
                        etat = "RDV";
                    }
                    else if (c.Etat == "A_VERIFIE")
                    {
                        etat = "A_VERIFIE";
                    }
                    else
                    {
                        etat = "En cours de traitement...";
                    }

                    DataRow row = dataTable.NewRow();
                    row["NumLot"] = c.NumLot;
                    row["IDClient"] = c.IDClient;
                    row["Compte"] = c.Compte;
                    row["NomClient"] = c.NomClient;
                    row["Etat"] = etat;
                    row["RDV"] = c.RDV;
                    dataTable.Rows.Add(row);

                }
            }
            else if(traite.Equals("SOLDE"))
            {
                newList = list.Select(j =>
                new FormulaireExportable
                {
                    NomClient = j.Lot.NomClient,
                    NumLot = j.Lot.NumLot,
                    Compte = j.Lot.Compte,
                    IDClient = j.Lot.IDClient,
                    Etat = j.Formulaire.EtatClient.ToString(),
                    Versement = j.Formulaire.MontantVerseDeclare.ToString()
                }

                ).ToList();

                dataTable.Columns.Add("NumLot", typeof(string));
                dataTable.Columns.Add("IDClient", typeof(string));
                dataTable.Columns.Add("Compte", typeof(string));
                dataTable.Columns.Add("NomClient", typeof(string));
                dataTable.Columns.Add("Etat", typeof(string));
                dataTable.Columns.Add("Versement", typeof(string));

                foreach (FormulaireExportable c in newList)
                {

                    string etat = "";

                    if (c.Etat == "SOLDE")
                    {
                        etat = "Client soldé";

                    }
                    else if (c.Etat == "SOLDE_TRANCHE")
                    {

                        etat = "Tranche versé";

                    } else if(c.Etat =="A_VERIFIE")
                    {

                        etat = "A verifié";

                    }
                    else
                    {
                        etat = "En cours de traitement...";
                    }

                    DataRow row = dataTable.NewRow();
                    row["NumLot"] = c.NumLot;
                    row["IDClient"] = c.IDClient;
                    row["Compte"] = c.Compte;
                    row["NomClient"] = c.NomClient;
                    row["Etat"] = etat;
                    row["Versement"] = c.Versement;
                    dataTable.Rows.Add(row);

                }

            }
            else if (traite.Equals("A_VERIFIE"))
            {
                newList = list.Select(j =>
                new FormulaireExportable
                {
                    NomClient = j.Lot.NomClient,
                    NumLot = j.Lot.NumLot,
                    Compte = j.Lot.Compte,
                    IDClient = j.Lot.IDClient,
                    Etat = j.Formulaire.EtatClient.ToString(),
                }

                ).ToList();

                dataTable.Columns.Add("NumLot", typeof(string));
                dataTable.Columns.Add("IDClient", typeof(string));
                dataTable.Columns.Add("Compte", typeof(string));
                dataTable.Columns.Add("NomClient", typeof(string));
                dataTable.Columns.Add("Etat", typeof(string));

                foreach (FormulaireExportable c in newList)
                {

                    string etat = "";

                    if (c.Etat == "SOLDE")
                    {
                        etat = "Client soldé";

                    }
                    else if (c.Etat == "SOLDE_TRANCHE")
                    {

                        etat = "Tranche versé";

                    }
                    else if (c.Etat == "A_VERIFIE")
                    {

                        etat = "A verifié";

                    }
                    else
                    {
                        etat = "En cours de traitement...";
                    }

                    DataRow row = dataTable.NewRow();
                    row["NumLot"] = c.NumLot;
                    row["IDClient"] = c.IDClient;
                    row["Compte"] = c.Compte;
                    row["NomClient"] = c.NomClient;
                    row["Etat"] = etat;
                    dataTable.Rows.Add(row);

                }

            }
            else
            {
                newList = list.Select(j =>
                new FormulaireExportable
                {
                NomClient = j.Lot.NomClient,
                NumLot = j.Lot.NumLot,
                Compte = j.Lot.Compte,
                IDClient = j.Lot.IDClient,
                Etat = j.Formulaire.EtatClient.ToString()
                }

                ).ToList();

                dataTable.Columns.Add("NumLot", typeof(string));
                dataTable.Columns.Add("IDClient", typeof(string));
                dataTable.Columns.Add("Compte", typeof(string));
                dataTable.Columns.Add("NomClient", typeof(string));
                dataTable.Columns.Add("Etat", typeof(string));
                
                foreach (FormulaireExportable c in newList)
                {

                    string etat = "";

                    if (c.Etat == "SOLDE")
                    {
                        etat = "Client soldé";

                    }
                    else if (c.Etat == "SOLDE_TRANCHE")
                    {

                        etat = "tranche versé";

                    }
                    else
                    {
                        etat = "En cours de traitement...";
                    }

                    DataRow row = dataTable.NewRow();
                    row["NumLot"] = c.NumLot;
                    row["IDClient"] = c.IDClient;
                    row["Compte"] = c.Compte;
                    row["NomClient"] = c.NomClient;
                    row["Etat"] = etat;
                    dataTable.Rows.Add(row);

                }

            }

            return dataTable;
        }

   


        public ActionResult EnvoyerBanque()
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(EnvoyerTraiteListForDropDown(), "Value", "Text");

            return View();
        }



        [HttpPost]
        public ActionResult EnvoyerBanqueLoadData(string traite,string numLot,string objet,string email, bool send)
        {

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(EnvoyerTraiteListForDropDown(), "Value", "Text");
            
            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            JoinedList = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId

                          select new ClientAffecteViewModel
                          {
                              
                              Formulaire = f,
                              Lot = l,
                              Affectation = a

                          }).ToList().Where(  j => ((j.Formulaire.IsVerified == true || j.Formulaire.EtatClient == Note.A_VERIFIE)  && j.Formulaire.NotifieBanque == false) ).ToList();

            string subject = "";
            string body = "";
            string name = "";


            if (traite == "RDV")
            {

                
                JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.RDV || j.Formulaire.EtatClient == Note.RDV_REPORTE).ToList();
                subject = EmailConstants.RDV_SUBJECT;
                body = EmailConstants.RDV_BODY;
                name = "RDV";

            }
            else if(traite == "SOLDE")
            {
                
                JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.SOLDE || j.Formulaire.EtatClient == Note.SOLDE_TRANCHE).ToList();
                subject = EmailConstants.VERSEMENT_SUBJECT;
                body = EmailConstants.VERSEMENT_BODY;
                name = "SOLDE";

            }
            else if(traite == "A_VERIFIE")
            {
                
                JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.A_VERIFIE ).ToList();
                subject = EmailConstants.AVERIFIE_SUBJECT;
                body = EmailConstants.AVERIFIE_BODY;
                name = "A_VERIFIE";

            }
            else if(traite == "Autre")
            {
              
                JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.FAUX_NUM || j.Formulaire.EtatClient== Note.NRP || j.Formulaire.EtatClient == Note.RACCROCHE || j.Formulaire.EtatClient == Note.INJOIGNABLE || j.Formulaire.EtatClient == Note.RAPPEL || j.Formulaire.EtatClient == Note.REFUS_PAIEMENT).ToList();
                subject = EmailConstants.ENCOURS_SUBJECT;
                body = EmailConstants.ENCOURS_BODY;
                name = "EN_COURS";

            }


            if (numLot != "0")
            {
                JoinedList = JoinedList.ToList().Where(j => j.Lot.NumLot.Equals(numLot)).ToList();
            }

            if(send == true)
            {

                //string fileName = Path.GetFileName(fileUploader.FileName);
                //mail.Attachments.Add(new Attachment(fileUploader.InputStream, fileName));
                
                string path = GetFolderName()+ "/" + name+"_MAJ_"+DateTime.Now.ToString("dd.MM.yyyy")+".xlsx";
                GenerateExcel(GenerateDatatableFromJoinedList(JoinedList, traite), path);

                SendMail(EmailConstants.TO, objet, email, path);

                foreach(var j in JoinedList)
                {
                    
                    j.Formulaire.NotifieBanque = true;
                    FormulaireService.Update(j.Formulaire);

                }
                FormulaireService.Commit();
             
                // return RedirectToAction("EnvoyerBanque");
            }


            JsonResult result = new JsonResult();

            try
            {
                string search = Request.Form.GetValues("search[value]")[0];
                string draw = Request.Form.GetValues("draw")[0];
                string order = Request.Form.GetValues("order[0][column]")[0];
                string orderDir = Request.Form.GetValues("order[0][dir]")[0];
                int startRec = Convert.ToInt32(Request.Form.GetValues("start")[0]);
                int pageSize = Convert.ToInt32(Request.Form.GetValues("length")[0]);

                int totalRecords = JoinedList.Count();

                if (!string.IsNullOrEmpty(search) &&
                    !string.IsNullOrWhiteSpace(search))
                {
                    JoinedList = JoinedList.Where(j =>

                        j.Lot.Numero.ToString().Contains(search)
                    || j.Lot.Adresse.ToString().ToLower().Contains(search.ToLower())
                    || j.Lot.IDClient.ToString().Contains(search)
                    || j.Lot.Compte.ToString().Contains(search)
                    || j.Lot.NomClient.ToString().ToLower().Contains(search.ToLower())
                    || j.Lot.DescIndustry.ToString().ToLower().Contains(search.ToLower())
                    || j.Formulaire.MontantVerseDeclare.ToString().ToLower().Contains(search.ToLower())

                        ).ToList();
                }

                
                JoinedList = SortTableDataForValidate(order, orderDir, JoinedList);

                int recFilter = JoinedList.Count();

                JoinedList = JoinedList.Skip(startRec).Take(pageSize).ToList();

                var modifiedData = JoinedList.Select(j =>
                   new
                   {

                       j.Lot.NumLot,
                       j.Lot.Compte,
                       j.Lot.IDClient,
                       j.Lot.NomClient,
                       Etat = j.Formulaire.EtatClient.ToString(),
                      
                   }
                   );
              
                int x = JoinedList.Count();
                ViewBag.x = x;
                var info = new { nbTotal = x, subject = subject, body = body,to=EmailConstants.TO };

                result = this.Json(new
                {

                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecords,
                    recordsFiltered = recFilter,
                    data = modifiedData,
                    info = info

                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            return result;
        }


    public static void GenerateExcel(DataTable dataTable, string path)
    {

        DataSet dataSet = new DataSet();
        dataSet.Tables.Add(dataTable);
        // create a excel app along side with workbook and worksheet and give a name to it
        Excel.Application excelApp = new Excel.Application();
        Excel.Workbook excelWorkBook = excelApp.Workbooks.Add();
        Excel._Worksheet xlWorksheet = excelWorkBook.Sheets[1];
        Excel.Range xlRange = xlWorksheet.UsedRange;
        foreach (DataTable table in dataSet.Tables)
        {
            //Add a new worksheet to workbook with the Datatable name
            Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
            excelWorkSheet.Name = table.TableName;
            // add all the columns
            for (int i = 1; i < table.Columns.Count + 1; i++)
            {
                excelWorkSheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
            }
            // add all the rows
            for (int j = 0; j < table.Rows.Count; j++)
            {
                for (int k = 0; k < table.Columns.Count; k++)
                {
                    excelWorkSheet.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();
                }
            }
        }
            // excelWorkBook.Save(); -> this will save to its default location
            
            excelWorkBook.SaveAs(path); // -> this will do the custom
            excelWorkBook.Close();
            excelApp.Quit();
        }



        public void SendMail(string to,string subject,string body,string path)
        {

            MailMessage mm = new MailMessage();
            mm.From = new MailAddress("alwakilrecouvrementmailtest@gmail.com");
            mm.To.Add(to);
            mm.Subject = subject;
            mm.Body = body;
            mm.IsBodyHtml = false;

            mm.Attachments.Add(new Attachment(path));
            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.UseDefaultCredentials = false;
            smtp.Port = 587;
            smtp.EnableSsl = true;

            smtp.Credentials = new NetworkCredential("alwakilrecouvrementmailtest@gmail.com", "wakil123456");
            smtp.Send(mm);
        }

        public string GetFolderName()
        {
            string folderName = Server.MapPath("~/Uploads/Updates");
            if(!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
                
            }

            return folderName;
        }
      




}  


}
