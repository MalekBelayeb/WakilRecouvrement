using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;

namespace WakilRecouvrement.Web.Controllers
{
    public class FormulaireController : Controller
    {

        AffectationService AffectationService;
        LotService LotService;
        EmployeService EmpService;
        FormulaireService FormulaireService;
   
        public int id = 0;

        public FormulaireController()
        {
            AffectationService = new AffectationService();
            LotService = new LotService();
            EmpService = new EmployeService();
            FormulaireService = new FormulaireService();

        }

        public ActionResult CreerFormulaire(string id)
        {
            ViewBag.TraiteList = new SelectList(TraiteListForDropDownForCreation(), "Value", "Text");
            ViewBag.id = id;
            ViewBag.affectation = AffectationService.GetById(long.Parse(id));
            return View(FormulaireService.GetAll().ToList().Where(f=>f.AffectationId==int.Parse(id)));
        }

        [HttpPost]
        public ActionResult CreerFormulaireIntermediate(string id)
        {
            return Json(new { redirectUrl = Url.Action("CreerFormulaire", "Formulaire", new { id = id }) });

        }

        public ActionResult SuiviClient()
        {
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
            listItems.Add(new SelectListItem { Selected = true, Text = "Touts sauf SOLDE/FAUX_NUM", Value = "SAUF" });

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
            List<Lot> listLot = new List<Lot>();
            List<Affectation> listAffectation = new List<Affectation>();
            List<Formulaire> listFormulaire =new List<Formulaire>();
            List<ClientAffecteViewModel> JoinedList = new List<ClientAffecteViewModel>();

            
            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");
            ViewBag.TraiteList = new SelectList(TraiteListForDropDown(), "Value", "Text");

            if (numLot == "0")
            {
                listLot = LotService.GetAll().ToList();
            }
            else
            {
                listLot = LotService.GetAll().ToList().Where(l => l.NumLot.Equals(numLot)).ToList();
            }

            if(int.Parse(agent)==0)
            {
               
                listAffectation = AffectationService.GetAll().ToList();

            }
            else
            {

                listAffectation = AffectationService.GetAll().ToList().Where(a=>a.EmployeId == int.Parse(agent)).ToList();

            }

          

            if (traite == "ALL")
            {


            }else if (traite == "SAUF")
            {

                listAffectation = listAffectation.Where(a => a.Formulaires.Count() > 0).Where(a => a.Formulaires.Last().EtatClient != (Note)Enum.Parse(typeof(Note), "SOLDE" ) && a.Formulaires.Last().EtatClient != (Note)Enum.Parse(typeof(Note), "FAUX_NUM")).ToList();

            }
            else
            {

                listAffectation = listAffectation.Where(a => a.Formulaires.Count() > 0).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), traite)).ToList();

            }

          JoinedList = (from a in listAffectation
                 join l in listLot on a.LotId equals l.LotId
                select new ClientAffecteViewModel
                 {
                 Affectation = a,
                 Lot = l,
                 }).ToList();


            ViewData["nbTotal"] = JoinedList.Count(); 
            ViewData["nbSoldeTotal"] = listAffectation.Where(a => a.Formulaires.Count() > 0).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE")).ToList(); ; 
            ViewData["nbTrancheSoldeTotal"] = listAffectation.Where(a => a.Formulaires.Count() > 0).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "SOLDE_TRANCHE")).ToList(); ; 
            ViewData["nbFNTotal"] = listAffectation.Where(a => a.Formulaires.Count() > 0).Where(a => a.Formulaires.Last().EtatClient == (Note)Enum.Parse(typeof(Note), "FAUX_NUM")).ToList(); ; 


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
                       Etat = GetEtat(j.Affectation.Formulaires).ToString()
                   }
                   ); 
                result = this.Json(new
                {
                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecords,
                    recordsFiltered = recFilter,
                    data = modifiedData
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
            //Debug.WriteLine(id);

            Formulaire Formulaire = new Formulaire();

            switch ((Note)Enum.Parse(typeof(Note), EtatClient))
            {
                case Note.INJOIGNABLE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.INJOIGNABLE;
                    
                    break;
                case Note.NRP:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.NRP;

                    break;
                case Note.RACCROCHE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.RACCROCHE;

                    break;
                case Note.RDV:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.RDV;
                    Formulaire.DateRDV = DateTime.Parse(RDVDateTime);

                    break;
                case Note.RDV_REPORTE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.RDV_REPORTE;
                    Formulaire.DateRDVReporte = DateTime.Parse(RDVReporteDateTime);


                    break; 
                case Note.REFUS_PAIEMENT:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.REFUS_PAIEMENT;

                    break;
                case Note.SOLDE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.SOLDE;

                    break;
                case Note.FAUX_NUM:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.FAUX_NUM;

                    break;
                case Note.A_VERIFIE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.A_VERIFIE;

                    break;
                case Note.AUTRE:
                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.AUTRE;
                    Formulaire.DescriptionAutre = DescriptionAutre;

                    break;                
                case Note.RAPPEL:

                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.RAPPEL;

                    break;
                case Note.SOLDE_TRANCHE:

                    Formulaire.AffectationId = int.Parse(id);
                    Formulaire.EtatClient = Note.SOLDE_TRANCHE;
                    Formulaire.TrancheSolde = double.Parse(soldetranche.Replace('.', ','));

                    break;
            }

            FormulaireService.Add(Formulaire);
            FormulaireService.Commit();

            return RedirectToAction("AffectationList", "Affectation");
        }

        

        [HttpPost]
        public ActionResult GetFormulaires(int id)
        {
            Affectation aff = AffectationService.GetById(id);
            var list = aff.Formulaires.Select(f => new
            {
                etat=f.EtatClient.ToString(),
                d1=f.DateRDV.ToString(),
                d2=f.DateRDVReporte.ToString(),
                tranche=f.TrancheSolde.ToString(),
                desc=f.DescriptionAutre
            });

            return Json(new { list=list });
        }


        public string GetEtat(ICollection<Formulaire> Formulaires)
        {
            if(Formulaires.Count()==0)
            {
                return "";
            }
            else
            {
                return Formulaires.LastOrDefault().IfNotNull(i => i.EtatClient).ToString();
            }

        }


       

    }  
}
