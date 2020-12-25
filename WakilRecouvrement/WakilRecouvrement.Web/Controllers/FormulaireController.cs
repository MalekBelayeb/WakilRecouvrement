using ClosedXML.Excel;
using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Web.Models.ViewModel;
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





        public ActionResult CreerFormulaire(string id, string msgError)
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            ViewBag.TraiteList = new SelectList(TraiteListForDropDownForCreation(), "Value", "Text");
            ViewBag.id = id;
            ViewBag.affectation = AffectationService.GetById(long.Parse(id));
            ViewBag.errormsg = msgError;

            ViewBag.TelFN = (from a in AffectationService.GetAll()
                                     join l in LotService.GetAll() on a.LotId equals l.LotId
                                     where a.AffectationId == long.Parse(id)
                                     select new TelFN
                                     {
                                         TelPortableFN = l.TelPortableFN,
                                         TelFixeFN = l.TelFixeFN
                                     
                                     }).FirstOrDefault();

            string soldeDeb = (from a in AffectationService.GetAll()
                              join l in LotService.GetAll() on a.LotId equals l.LotId
                              where a.AffectationId == long.Parse(id)
                              select new
                              {
                                  SoldeDeb = l.SoldeDebiteur

                              }).FirstOrDefault().SoldeDeb;


            ViewBag.soldeDeb = soldeDeb.Replace(',', '.');

            return View(FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe).ToList().Where(f => f.AffectationId == int.Parse(id)));
        }




        public IEnumerable<SelectListItem> SortOrderSuiviClientForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. decroissant)", Value = "3" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. croissant)", Value = "4" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. decroissant)", Value = "5" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. croissant)", Value = "6" });
         
            return listItems;
        }

        public IEnumerable<SelectListItem> SortOrderSuiviRDVForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. decroissant)", Value = "3" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. croissant)", Value = "4" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date RDV (o. decroissant)", Value = "5" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date RDV (o. croissant)", Value = "6" });

            return listItems;
        }
        public IEnumerable<SelectListItem> SortOrderRentabiliteForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. decroissant)", Value = "3" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date affectation (o. croissant)", Value = "4" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. decroissant)", Value = "5" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. croissant)", Value = "6" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Revenue (o. decroissant)", Value = "7" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Revenue (o. croissant)", Value = "8" });


            return listItems;
        }

        public IEnumerable<SelectListItem> SortOrderRentabiliteDateForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Annuel", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Mensuel", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Quotidienne", Value = "2" });



            return listItems;
        }


        public List<Formulaire> getFormulaires(int AffectationId)
        {
            List<Formulaire> formulaires = (from f in FormulaireService.GetAll()
                                            join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                            where a.AffectationId == AffectationId
                                            select f).ToList();

            return formulaires;
        }




        public ActionResult HistoriqueClient(int id)
        {

            Debug.WriteLine(id);
            List<ClientAffecteViewModel> clientAffecteViewModels = (from f in FormulaireService.GetAll()
                                                                    join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                                                    where a.AffectationId == id
                                                                    select new ClientAffecteViewModel
                                                                    {
                                                                        Formulaire = f,
                                                                        Affectation = a

                                                                    }).ToList();
            var obj = (from a in AffectationService.GetAll()
                       join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                       where a.AffectationId == id
                       select new { e.Username, a.AffectationId }).FirstOrDefault();
           
            
            ViewBag.username = obj.Username.ToString();
            ViewBag.id = obj.AffectationId.ToString();

            return View(clientAffecteViewModels);
        }

        public ActionResult SuiviClient(string numLot,string SearchString,string traite,string agent, string currentFilter, string sortOrder, int? page)
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");
           
            ViewBag.CurrentSort = sortOrder;

            List<ClientAffecteViewModel> JoinedList;

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteListForDropDown(), "Value", "Text");
            ViewData["sortOrder"] = new SelectList(SortOrderSuiviClientForDropDown(), "Value", "Text");

            if (SearchString != null)
            {
                page = 1;
            }
            else
            {
                SearchString = currentFilter;
            }

            ViewBag.CurrentFilter = SearchString;


            if (!String.IsNullOrEmpty(traite))
            {

                if (traite == "ALL")
                {
                  
                    JoinedList = (from a in AffectationService.GetAll()
                                  join l in LotService.GetAll() on a.LotId equals l.LotId

                                   select new ClientAffecteViewModel
                                  {

                                      Formulaire = FormulaireService.GetMany(f => f.AffectationId == a.AffectationId).OrderByDescending(f => f.TraiteLe).FirstOrDefault(),
                                     // Formulaire = a.Formulaires.OrderByDescending(o => o.TraiteLe).FirstOrDefault(),
                                      Affectation = a,
                                      Lot = l,

                                  }).DistinctBy(a => a.Affectation.AffectationId).ToList();

               
                }
                else if (traite == "SAUF")
                {

                    JoinedList = (from f in FormulaireService.GetMany(f=>f.EtatClient+"" != "SOLDE" && f.EtatClient+"" !=  "FAUX_NUM").OrderByDescending(o => o.TraiteLe)
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId
                                  select new ClientAffecteViewModel
                                  {

                                      Formulaire = f,
                                      Affectation = a,
                                      Lot = l,

                                  }).DistinctBy(d => d.Formulaire.AffectationId).ToList();
                }
                else
                {

                    JoinedList =(from f in FormulaireService.GetMany(f => f.EtatClient+"" == traite ).OrderByDescending(o => o.TraiteLe)
                                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                  join l in LotService.GetAll() on a.LotId equals l.LotId
                                  select new ClientAffecteViewModel
                                  {

                                      Formulaire = f,
                                      Affectation = a,
                                      Lot = l,

                                  }).DistinctBy(d => d.Formulaire.AffectationId).ToList();
                }

            }
            else
            {
                
                JoinedList = (from a in AffectationService.GetAll()
                              join l in LotService.GetAll() on a.LotId equals l.LotId

                              select new ClientAffecteViewModel
                              {
                                  Formulaire = FormulaireService.GetMany(f => f.AffectationId == a.AffectationId).OrderByDescending(f => f.TraiteLe).FirstOrDefault(),
                                  //Formulaire = a.Formulaires.OrderByDescending(o => o.TraiteLe).FirstOrDefault(),
                                  Affectation = a,
                                  Lot = l,

                              }).DistinctBy(a => a.Affectation.AffectationId).ToList();
            }

             
            if (!String.IsNullOrEmpty(agent))
            {
                if (int.Parse(agent) != 0)
                {
                    JoinedList = JoinedList.Where(j => j.Affectation.EmployeId == int.Parse(agent)).ToList();
                }

            }

        

            if (!String.IsNullOrEmpty(numLot))
            {
                if (numLot.Equals("0") == false)
                    JoinedList = JoinedList.Where(j => j.Lot.NumLot.Equals(numLot)).ToList();

            }

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
                    JoinedList = JoinedList.Where(s=>s.Formulaire!=null).OrderByDescending(s => s.Formulaire.TraiteLe).ToList();
                    break;                 
                case "6":
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

        public IEnumerable<SelectListItem> NumLotListForDropDown()
        {

            List<Lot> Lots = LotService.GetAll().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les lots", Value = "0" });

            Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
            });

            return listItems;
        }


        public IEnumerable<SelectListItem> RDVForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "RDV du jour", Value = "RDV_J" });
            listItems.Add(new SelectListItem { Text = "RDV pour demain", Value = "RDV_DEMAIN" });
            listItems.Add(new SelectListItem { Text = "RDV pour les prochains jours", Value = "RDV_JOURS_PROCHAINE" });
            listItems.Add(new SelectListItem { Text = "RDV pour la semaine prochaine", Value = "RDV_SEMAINE_PROCHAINE" });
            listItems.Add(new SelectListItem { Text = "Tous les RDV", Value = "ALL" });


            return listItems;
        }


        public IEnumerable<SelectListItem> AgentListForDropDown()
        {

            List<Employe> agents = EmpService.GetMany(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true).ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les agents", Value = "0" });

            agents.ForEach(l => {
                listItems.Add(new SelectListItem { Text = l.Username, Value = l.EmployeId + "" });
            });

            return listItems;
        }
        public IEnumerable<SelectListItem> TraiteListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les clients affectés", Value = "ALL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous clients traités sauf SOLDE/FAUX_NUM", Value = "SAUF" });

            foreach (var n in Enum.GetValues(typeof(Note)))
            {

                listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString() });

            }


            return listItems;
        }

        public IEnumerable<SelectListItem> TraiteListRentabiliteForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Traitement Soldé et tranche", Value = "ALL" });
            listItems.Add(new SelectListItem { Text = "Soldé", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "Tranche", Value = "SOLDE_TRANCHE" });


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
        public ActionResult CreerFormulaireNote(string id, string DescriptionAutre, string EtatClient, string RDVDateTime, string RDVReporteDateTime, string soldetranche, HttpPostedFileBase PostedFile)
        {
            ViewBag.TraiteList = new SelectList(TraiteListForDropDownForCreation(), "Value", "Text");

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

                    Formulaire.MontantVerseDeclare = double.Parse(soldetranche);

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

                    Formulaire.DescriptionAutre = DescriptionAutre;

                    break;
                case Note.RAPPEL:

                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.TraiteLe = DateTime.Now;
                    Formulaire.Status = Status.VERIFIE;

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

                    Formulaire.MontantVerseDeclare = double.Parse(soldetranche);

                    break;
            }


            Formulaire.NotifieBanque = false;


            var nbVerfie = (from f in FormulaireService.GetAll()
                            join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                            where a.AffectationId == int.Parse(id) && f.Status == Status.EN_COURS && f.EtatClient == Note.A_VERIFIE
                            select new ClientAffecteViewModel
                            {
                                Formulaire = f,
                                Affectation = a
                            }).ToList();



            if (Formulaire.EtatClient == Note.A_VERIFIE)
            {
                if (nbVerfie.Count() >= 1)
                {
                    return RedirectToAction("CreerFormulaire", new { id = id, msgError = "Une verification est deja en attente!" });
                }
            }


            FormulaireService.Add(Formulaire);
            FormulaireService.Commit();



            Lot Joinedlot = (from f in FormulaireService.GetAll()
                             join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                             join l in LotService.GetAll() on a.LotId equals l.LotId
                             where f.FormulaireId == Formulaire.FormulaireId
                             select new Lot
                             {
                                 NumLot = l.NumLot,
                                 SoldeDebiteur = l.SoldeDebiteur

                             }).FirstOrDefault();


            Formulaire.MontantDebInitial = double.Parse(Joinedlot.SoldeDebiteur);



            if (FormulaireService.GetAll().Where(f => f.AffectationId == int.Parse(id)).Count() == 1)
            {

                Formulaire.MontantDebMAJ = double.Parse(Joinedlot.SoldeDebiteur);

            }
            else
            {
                Formulaire.MontantDebMAJ = (from f in FormulaireService.GetAll()
                                            join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                            where a.AffectationId == int.Parse(id) && f.MontantDebMAJ != 0
                                            orderby f.MontantDebMAJ ascending
                                            select new ClientAffecteViewModel
                                            {
                                                Formulaire = f,
                                                Affectation = a
                                            }).FirstOrDefault().Formulaire.MontantDebMAJ;
            }




            FormulaireService.Update(Formulaire);
            FormulaireService.Commit();

            if (PostedFile != null)
            {
                string filePath = string.Empty;
                string path = Server.MapPath("~/Uploads/Recu/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                filePath = path + Formulaire.FormulaireId.ToString() + Path.GetExtension(PostedFile.FileName);

                PostedFile.SaveAs(filePath);
            }


            switch (Formulaire.EtatClient)
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

            return RedirectToAction("AffectationList", "Affectation", new { traite = "SAUF", numLot = Joinedlot.NumLot, sortOrder = "5" });
        }

        public ActionResult deleteHist(int idHist,int id,string msgError)
        {

            FormulaireService.Delete(FormulaireService.GetById(idHist));
            FormulaireService.Commit();

            return RedirectToAction("CreerFormulaire","Formulaire", new { id = id, msgError = msgError});
        }


        [HttpPost]
        public ActionResult GetFormulaires(int id)
        {

            var joinedAffectation = FormulaireService.GetOrderedFormulaireByAffectationList(id).Select(j => new ClientAffecteViewModel { Formulaire = j });

            var list = joinedAffectation.Select(f => new
            {
                etat = f.Formulaire.EtatClient.ToString(),
                d1 = f.Formulaire.DateRDV.ToString(),
                tranche = f.Formulaire.MontantVerseDeclare.ToString(),
                desc = f.Formulaire.DescriptionAutre,
                verif = f.Formulaire.Status.ToString(),
                traitele = f.Formulaire.TraiteLe.ToString(),
                montantDebInitial = f.Formulaire.MontantDebInitial.ToString(),
                montantDebMaj = f.Formulaire.MontantDebMAJ.ToString()
            });

            return Json(new { list = list });
        }

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
            string path = Server.MapPath("~/Uploads/Recu/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

            if (TempData["IDClient"] == null)
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
        public ActionResult ValiderTraitement(bool IsValid, string numLot, string traite, string agent)
        {
            string status = "";
            if (IsValid == true)
                status = "VERIFIE";
            if (IsValid == false)
                status = "EN_COURS";


            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");

            if (IsValid == false)
                ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
            if (IsValid == true)
                ViewBag.TraiteList = new SelectList(TraiteValidationValideListForDropDown(), "Value", "Text");

            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

            if (traite.Equals("ALL"))
            {
                JoinedList = (from f in FormulaireService.GetAll()
                              join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                              join l in LotService.GetAll() on a.LotId equals l.LotId
                              orderby f.TraiteLe descending
                              select new ClientAffecteViewModel
                              {
                                  Formulaire = f,
                                  Lot = l,
                                  Affectation = a

                              }).Where(j => j.Formulaire.Status == (Status)Enum.Parse(typeof(Status), status)).Where(j => j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE_TRANCHE") || j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "A_VERIFIE")).ToList();

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
                                  Lot = l,
                                  Affectation = a

                              }).Where(j => j.Formulaire.Status == (Status)Enum.Parse(typeof(Status), status)).Where(j => j.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), traite)).ToList();
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
                       Image = getImagePath(j.Formulaire)

                   }
                   );
                int x = JoinedList.Count();

                var info = new { nbTotal = x };

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
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les traitements non validés", Value = "ALL" });
            listItems.Add(new SelectListItem { Text = "Soldé", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "Tranche", Value = "SOLDE_TRANCHE" });
            listItems.Add(new SelectListItem { Text = "A verifié", Value = "A_VERIFIE" });

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
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les traitements validés", Value = "ALL" });
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
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Formulaire.VerifieLe).ToList()
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
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(j => j.Formulaire.VerifieLe).ToList()
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


        public Formulaire GetFormulaire(int affId)
        {
            var forms = (from f in FormulaireService.GetAll()
                         join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                         where a.AffectationId == affId
                         orderby f.MontantDebMAJ ascending
                         select new ClientAffecteViewModel
                         {
                             Formulaire = f,
                             Affectation = a
                         }).FirstOrDefault();


            return forms.Formulaire;

        }

        [HttpPost]
        public ActionResult VerifierEtat(int id, bool valid, string montant)
        {

            var JoinedLot = from f in FormulaireService.GetAll()
                            join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                            join l in LotService.GetAll() on a.LotId equals l.LotId
                            where f.FormulaireId == id
                            select new ClientAffecteViewModel { Lot = l, Formulaire = f };


            Lot Lot = JoinedLot.ToList().FirstOrDefault().Lot;
            Formulaire Formulaire = JoinedLot.ToList().FirstOrDefault().Formulaire;

            double DebMaJ = GetFormulaire(Formulaire.AffectationId).MontantDebMAJ;
            Debug.WriteLine(montant);
            if (valid == false)
            {
                DeleteFromulaire(Formulaire);
                return Json(new { });
            }

            double SoldeDebiteur = double.Parse(Lot.SoldeDebiteur);
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

                    Formulaire.MontantVerseDeclare = double.Parse(montant.Replace('.', ','));

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

            if (NotificationService.GetAll().Where(e => e.FormulaireId == Formulaire.FormulaireId).Count() != 0)
            {
                NotificationService.Delete(NotificationService.GetAll().Where(e => e.FormulaireId == Formulaire.FormulaireId).FirstOrDefault());
                NotificationService.Commit();

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

        public DataTable GenerateDatatableFromJoinedList(List<ClientAffecteViewModel> list, string traite)
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
            else if (traite.Equals("SOLDE"))
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
                dataTable.Columns.Add("Montant", typeof(string));

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
        public ActionResult EnvoyerBanqueLoadData(string traite, string numLot, string objet, string email, bool send, string to)
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

                          }).ToList().Where(j => ((j.Formulaire.Status == Status.VERIFIE || j.Formulaire.EtatClient == Note.A_VERIFIE) && j.Formulaire.NotifieBanque == false)).ToList();

            string subject = "";
            string body = "";
            string name = "";
            string To = "";


            if (traite == "RDV")
            {


                JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.RDV).ToList();
                subject = EmailConstants.RDV_SUBJECT;
                body = EmailConstants.RDV_BODY;
                name = "RDV";
                To = EmailConstants.TO;
            }
            else if (traite == "SOLDE")
            {

                JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.SOLDE || j.Formulaire.EtatClient == Note.SOLDE_TRANCHE).ToList();
                subject = EmailConstants.VERSEMENT_SUBJECT;
                body = EmailConstants.VERSEMENT_BODY;
                name = "SOLDE";
                To = EmailConstants.TO;

            }
            else if (traite == "A_VERIFIE")
            {

                JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.A_VERIFIE).ToList();
                subject = EmailConstants.AVERIFIE_SUBJECT;
                body = EmailConstants.AVERIFIE_BODY;
                name = "A_VERIFIE";
                To = EmailConstants.TO;

            }
            else if (traite == "Autre")
            {

                JoinedList = JoinedList.Where(j => j.Formulaire.EtatClient == Note.FAUX_NUM || j.Formulaire.EtatClient == Note.NRP || j.Formulaire.EtatClient == Note.RACCROCHE || j.Formulaire.EtatClient == Note.INJOIGNABLE || j.Formulaire.EtatClient == Note.RAPPEL || j.Formulaire.EtatClient == Note.REFUS_PAIEMENT).ToList();
                subject = EmailConstants.ENCOURS_SUBJECT;
                body = EmailConstants.ENCOURS_BODY;
                name = "EN_COURS";
                To = EmailConstants.TO;

            }


            if (numLot != "0")
            {
                JoinedList = JoinedList.ToList().Where(j => j.Lot.NumLot.Equals(numLot)).ToList();
            }

            if (send == true)
            {

                //string fileName = Path.GetFileName(fileUploader.FileName);
                //mail.Attachments.Add(new Attachment(fileUploader.InputStream, fileName));

                string path = GetFolderName() + "/" + name + "_MAJ_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() + ".xlsx";

                GenerateExcel(GenerateDatatableFromJoinedList(JoinedList, traite), path);

                SendMail(to, objet, email, path);

                foreach (var j in JoinedList)
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
                var info = new { nbTotal = x, subject = subject, body = body, to = To };

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

            dataTable.TableName = "Table1";

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
                // Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
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

        public void SendMail(string to, string subject, string body, string path)
        {

            MailMessage mm = new MailMessage();
            mm.From = new MailAddress("alwakilrecouvrementmailtest@gmail.com");
            foreach (string t in to.Split(',').ToList())
            {
                Debug.WriteLine(t);
                mm.To.Add(t);
            }

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
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);

            }

            return folderName;
        }

        public string getImagePath(Formulaire formulaire)
        {
            string path = "";
            foreach (string f in Directory.GetFiles(Server.MapPath("~/Uploads/Recu/")))
            {
                string extension = Path.GetExtension(f);
                string name = Path.GetFileName(f);
                if (name.Equals(formulaire.FormulaireId + extension))
                {

                    path = name;

                }

            }
            return path;
        }




        [HttpPost]
        public ActionResult UploadVerifier(HttpPostedFileBase PostedFile)
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteValidationListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");


            //Nthabtou li fichier mahouch feragh makenesh nabaathou erreur lel client
            if (PostedFile != null)
            {
                //nsobou l fichier aana fel serveur
                string filePath = string.Empty;
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(PostedFile.FileName);
                string extension = Path.GetExtension(PostedFile.FileName);
                string conString = string.Empty;

                if (PostedFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    PostedFile.SaveAs(filePath);

                    //besh nakhtarou connectionString selon l version mtaa excel (xls = 2003 o xlsx mel 2013 o ahna tal3in)
                    //L connectionString predefini fel web.config mteena
                    switch (extension)
                    {
                        case ".xls":
                            conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                            break;

                        case ".xlsx":
                            conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                            break;

                    }

                    //Taoua besh nebdew nakraw l fichier Excel bel library OleDd
                    DataTable dt = new DataTable();
                    conString = string.Format(conString, filePath);
                    using (OleDbConnection connExcel = new OleDbConnection(conString))
                    {
                        using (OleDbCommand cmdExcel = new OleDbCommand())
                        {
                            using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                            {
                                // Houni nebdew naakraw awel sheet name mtaa l document excel mteena (eli ken jina fi table SQL rahou le nom de la table) 
                                cmdExcel.Connection = connExcel;
                                connExcel.Open();
                                DataTable dtExcelSchema;
                                dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                                connExcel.Close();
                                Debug.WriteLine(sheetName);
                                //Houni recuperation mtaa les données 
                                connExcel.Open();
                                cmdExcel.CommandText = "SELECT * FROM [Table1$]";
                                odaExcel.SelectCommand = cmdExcel;
                                odaExcel.Fill(dt);
                                connExcel.Close();

                                //string argNumLot = dt.Columns[1].ColumnName;
                                //Debug.WriteLine(argNumLot);
                                string argNumLot = "NumLot";
                                string argIDClient = "IDClient";
                                string argCompte = "Compte";
                                string argNomClient = "NomClient";
                                string argEtat = "Etat";
                                string argMontant = "Montant";


                                foreach (DataRow row in dt.Rows)
                                {
                                    string NumLot = "";
                                    string IDClient = "";
                                    string Compte = "";
                                    string NomClient = "";
                                    string Etat = "";
                                    string Montant = "";


                                    NumLot = row[argNumLot].ToString();
                                    IDClient = row[argIDClient].ToString();
                                    Compte = row[argCompte].ToString();
                                    NomClient = row[argNomClient].ToString();
                                    Etat = row[argEtat].ToString();
                                    Montant = row[argMontant].ToString();

                                    //Debug.WriteLine(IDClient);
                                    VerifyClient(IDClient, Montant);
                                }

                            }
                        }
                    }

                }
                else
                {
                    ModelState.AddModelError("Importer", "Le fichier selectionné n'est pas un fichier Excel");
                }

            }
            else
            {
                ModelState.AddModelError("Importer", "Vous devez sélectionner un fichier");
            }

            return RedirectToAction("ValiderTraitement");

        }


        public void VerifyClient(string idclient, string montant)
        {
            if(montant == "")
            {
                montant = "0";
            }

            var formulaireAtraite = (from f in FormulaireService.GetAll()
                                     join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                     join l in LotService.GetAll() on a.LotId equals l.LotId
                                     where l.IDClient == idclient && f.EtatClient == Note.A_VERIFIE && f.Status == Status.EN_COURS
                                     select new ClientAffecteViewModel
                                     {
                                         Formulaire = f,
                                         Affectation = a,
                                         Lot = l
                                     }).ToList().FirstOrDefault();

            try
            {

                formulaireAtraite.Formulaire.MontantVerseDeclare = double.Parse(montant.Replace('.', ','));

            }
            catch (FormatException)
            {
                DeleteFromulaire(formulaireAtraite.Formulaire);
            }

            Decimal NewSolde = 0;

            NewSolde = Decimal.Subtract(decimal.Parse(formulaireAtraite.Formulaire.MontantDebMAJ.ToString()), decimal.Parse(formulaireAtraite.Formulaire.MontantVerseDeclare.ToString()));
            double DebMaJ = GetFormulaire(formulaireAtraite.Formulaire.AffectationId).MontantDebMAJ;



            NewSolde = Decimal.Subtract(decimal.Parse(DebMaJ.ToString()), decimal.Parse(formulaireAtraite.Formulaire.MontantVerseDeclare.ToString()));

            if (NewSolde <= 0)
            {

                formulaireAtraite.Formulaire.MontantDebMAJ = 0;

                formulaireAtraite.Formulaire.Status = Status.VERIFIE;
                formulaireAtraite.Formulaire.VerifieLe = DateTime.Now;
                formulaireAtraite.Formulaire.EtatClient = Note.SOLDE;

            }
            else if (NewSolde > 0)
            {
                formulaireAtraite.Formulaire.MontantDebMAJ = double.Parse(NewSolde.ToString());

                formulaireAtraite.Formulaire.Status = Status.VERIFIE;
                formulaireAtraite.Formulaire.VerifieLe = DateTime.Now;
                formulaireAtraite.Formulaire.EtatClient = Note.SOLDE_TRANCHE;

            }

        }


        public void DeleteFromulaire(Formulaire formulaire)
        {

            FormulaireService.Delete(formulaire);
            FormulaireService.Commit();

        }

        public ActionResult SuiviRDV(string numLot, string RDVType, string sortOrder, int? page)
        {

            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            ViewBag.CurrentSort = sortOrder;


            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.RDVList = new SelectList(RDVForDropDown(), "Value", "Text");
            ViewData["sortOrder"] = new SelectList(SortOrderSuiviRDVForDropDown(), "Value", "Text");



            JoinedList = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where a.Employe.Username.Equals(Session["username"])
                          orderby f.TraiteLe descending
                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).ToList().DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "RDV")).ToList();





            if (!String.IsNullOrEmpty(numLot))
            {
                if (numLot != "0")
                {
                    JoinedList = JoinedList.ToList().Where(j => j.Lot.NumLot.Equals(numLot)).ToList();
                }
            }

            if (!String.IsNullOrEmpty(RDVType))
            {
                if (RDVType == "RDV_J")
                {
                    JoinedList = JoinedList.ToList().Where(j => j.Formulaire.DateRDV.Date == DateTime.Today.Date).ToList();

                }
                else if (RDVType == "RDV_DEMAIN")
                {

                    JoinedList = JoinedList.ToList().Where(j => j.Formulaire.DateRDV.Date == DateTime.Today.AddDays(1).Date).ToList();

                }
                else if (RDVType == "RDV_JOURS_PROCHAINE")
                {

                    JoinedList = JoinedList.ToList().Where(j => j.Formulaire.DateRDV.Date >= DateTime.Today.AddDays(2).Date && j.Formulaire.DateRDV.Date < DateTime.Today.AddDays(7).Date).ToList();

                }
                else if (RDVType == "RDV_SEMAINE_PROCHAINE")
                {

                    JoinedList = JoinedList.ToList().Where(j => j.Formulaire.DateRDV.Date >= DateTime.Today.AddDays(7).Date && j.Formulaire.DateRDV.Date < DateTime.Today.AddDays(14).Date).ToList();

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


                default:


                    break;
            }



            ViewBag.total = JoinedList.Count();

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(JoinedList.ToPagedList(pageNumber, pageSize));

        }

        public string GetAffectationAgent(Affectation affectation)
        {

            var username = from a in AffectationService.GetAll()
                           join e in EmpService.GetAll() on a.EmployeId equals e.EmployeId
                           select new 
                           {
                               Username = e.Username
                           };

            return username.FirstOrDefault().Username;
        }


        [HttpPost]
        public ActionResult SuiviRDVNB()
        {
            int nb=0;

            nb = (from f in FormulaireService.GetAll()
                          join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                          join l in LotService.GetAll() on a.LotId equals l.LotId
                          where a.Employe.Username.Equals(Session["username"])
                          orderby f.TraiteLe descending
                          select new ClientAffecteViewModel
                          {

                              Formulaire = f,
                              Affectation = a,
                              Lot = l,

                          }).DistinctBy(d => d.Formulaire.AffectationId).Where(f => f.Formulaire.EtatClient == (Note)Enum.Parse(typeof(Note), "RDV")).Where(j => j.Formulaire.DateRDV.Date == DateTime.Today.Date).Count();



            return Json(new { nb = nb });
        }

        [HttpPost]
        public ActionResult UpdateTelFN(int lotId,int affectationId)
        {
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

            lot.TelFixeFN = TelFixe;
            lot.TelPortableFN = TelPortable;


            LotService.Update(lot);
            LotService.Commit();


            return RedirectToAction("CreerFormulaire", "Formulaire", new { id = affectationId, msgError = "" });
        }

        public ActionResult Rentablite(string rentabilite,string numLot, string SearchString, string traite, string agent, string currentFilter, string sortOrder, int? page,string poucentage)
        {


            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            ViewBag.CurrentSort = sortOrder;

            List<RentabiliteViewModel> JoinedList = new List<RentabiliteViewModel>();
            
            
            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteListRentabiliteForDropDown(), "Value", "Text");
            ViewData["sortOrder"] = new SelectList(SortOrderRentabiliteForDropDown(), "Value", "Text");
            ViewData["rentabilite"] = new SelectList(SortOrderRentabiliteDateForDropDown(), "Value", "Text");

            if (SearchString != null)
            {
                page = 1;
            }
            else
            {
                SearchString = currentFilter;
            }

            ViewBag.CurrentFilter = SearchString;
            ViewBag.pourcentage = poucentage;


            if (!String.IsNullOrEmpty(traite))
            {

                JoinedList = (from f in FormulaireService.GetMany(f => f.Status == Status.VERIFIE && (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE)).OrderByDescending(f => f.TraiteLe)
                              join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                              join l in LotService.GetAll() on a.LotId equals l.LotId
                              select new RentabiliteViewModel
                              {

                                  Formulaire = f,
                                  Affectation = a,
                                  Lot = l,
                                  Revenue = CalculeRevenue(float.Parse(poucentage + ""), f.AffectationId)

                              }).DistinctBy(d => d.Formulaire.AffectationId).ToList();


                if (traite == "SOLDE")
                {

                    JoinedList = JoinedList.Where(o => o.Formulaire.EtatClient == Note.SOLDE).ToList();

                }
                else if (traite == "SOLDE_TRANCHE")
                {

                    JoinedList = JoinedList.Where(o => o.Formulaire.EtatClient == Note.SOLDE_TRANCHE).ToList();

                }

            }



            if (!String.IsNullOrEmpty(rentabilite))
            {

                if (int.Parse(rentabilite) == 0)
                {
                    JoinedList = JoinedList.Where(j => j.Formulaire.VerifieLe.Date.Year == DateTime.Now.Year).ToList();
                }
                else if (int.Parse(rentabilite) == 1)
                {


                    JoinedList = JoinedList.Where(j => j.Formulaire.VerifieLe.Date.Month == DateTime.Now.Month).ToList();

                }
                else if (int.Parse(rentabilite) == 2)
                {

                    JoinedList = JoinedList.Where(j => j.Formulaire.VerifieLe.Date.Day == DateTime.Now.Day).ToList();

                }
            }


            
            if (!String.IsNullOrEmpty(agent))
            {
                if (int.Parse(agent) != 0)
                {
                    JoinedList = JoinedList.ToList().Where(j => j.Affectation.EmployeId == int.Parse(agent)).ToList();
                }
            }
            
            if (!String.IsNullOrEmpty(rentabilite))
            {
                
                if (int.Parse(rentabilite) == 0)
                {
                    JoinedList = JoinedList.ToList().Where(f=>f.Formulaire.VerifieLe.Date.Year == DateTime.Now.Year).ToList();
                }

                if (int.Parse(rentabilite) == 1)
                {
                    JoinedList = JoinedList.ToList().Where(f => f.Formulaire.VerifieLe.Date.Month == DateTime.Now.Month).ToList();
                }

                if (int.Parse(rentabilite) == 2)
                {
                    JoinedList = JoinedList.ToList().Where(f => f.Formulaire.VerifieLe.Date.Day == DateTime.Now.Day).ToList();
                }

            }



            if (!String.IsNullOrEmpty(numLot))
            {
                if (numLot.Equals("0") == false)
                    JoinedList = JoinedList.Where(j => j.Lot.NumLot.Equals(numLot)).ToList();

            }

            if (!String.IsNullOrEmpty(SearchString))
            {
                JoinedList = JoinedList.Where(s => s.Lot.Adresse.ToLower().Contains(SearchString.ToLower())
                                       || s.Lot.Compte.ToLower().Contains(SearchString.ToLower())
                                       || s.Lot.DescIndustry.ToLower().Contains(SearchString.ToLower())
                                       || s.Lot.IDClient.ToLower().Contains(SearchString.ToLower())
                                       || s.Lot.NomClient.ToLower().Contains(SearchString.ToLower())
                                       || s.Lot.Numero.ToLower().Contains(SearchString.ToLower())
                                       || s.Lot.SoldeDebiteur.ToLower().Contains(SearchString.ToLower())
                                       || s.Lot.TelFixe.ToLower().Contains(SearchString.ToLower())
                                       || s.Lot.TelPortable.ToLower().Contains(SearchString.ToLower())

                                       ).ToList();
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
                        JoinedList = JoinedList.OrderBy(s => double.Parse(s.Lot.SoldeDebiteur)).ToList();

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
                    JoinedList = JoinedList.OrderByDescending(s => s.Formulaire.TraiteLe).ToList();
                    break;                 
                case "6":
                    JoinedList = JoinedList.OrderBy(s => s.Formulaire.TraiteLe).ToList();
                    break;                
                case "7":
                    JoinedList = JoinedList.OrderByDescending(s => s.Revenue).ToList();
                    break;
                    case "8":
                    JoinedList = JoinedList.OrderBy(s => s.Revenue).ToList();
                    break;


                default:


                    break;
            }

            double x = 0;

            JoinedList.ForEach(e => x += e.Revenue);

            ViewBag.RevenueTotal = x;

            ViewBag.total = JoinedList.Count();
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(JoinedList.ToPagedList(pageNumber, pageSize));

        }


        public double CalculeRevenue(float tauxPoucentage, int affId)
        {
            double tot = 0;
            var x = (from f in FormulaireService.GetMany(f=>f.AffectationId == affId && f.Status== Status.VERIFIE && (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE) )
                      select new { montantVerse = f.MontantVerseDeclare });
            
            foreach(var v in x)
            {

                tot += (double) v.montantVerse * tauxPoucentage / 100;

            }


            return tot;
        }

        public ActionResult DetailsRevenue(int idAff,int idLot, string pourcentage,string total)
        {
            if (Session["username"] == null || Session["username"].ToString().Length < 1)
                return RedirectToAction("Login", "Authentification");

            var joinedList = (from f in FormulaireService.GetMany(f => f.AffectationId == idAff && f.Status == Status.VERIFIE && (f.EtatClient == Note.SOLDE || f.EtatClient == Note.SOLDE_TRANCHE))
                            
                            select new RentabiliteViewModel {Affectation=AffectationService.GetById(idAff),Lot = LotService.GetById(idLot),Formulaire =f,Revenue = (f.MontantVerseDeclare*float.Parse(pourcentage))/100 });

            ViewBag.RevenueTotal = total;

            return View(joinedList.ToList());
        }
         





    }


}