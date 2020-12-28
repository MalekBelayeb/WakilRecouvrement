using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;
using WakilRecouvrement.Web.Models.ViewModel;

namespace WakilRecouvrement.Web.Controllers
{
    public class StatistiqueController : Controller
    {

        AffectationService AffectationService;
        LotService LotService;
        EmployeService EmpService;
        FormulaireService FormulaireService;

        public StatistiqueController()
        {
            AffectationService = new AffectationService();
            LotService = new LotService();
            EmpService = new EmployeService();
            FormulaireService = new FormulaireService();

        }

        public IEnumerable<SelectListItem> NumLotListForDropDown()
        {

            List<Lot> Lots = LotService.GetAll().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
            });

            return listItems;
        }


        public IEnumerable<SelectListItem> TypeStatForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Lot complet", Value = "1" });
            listItems.Add(new SelectListItem {  Text = "Par Date", Value = "2" });
            listItems.Add(new SelectListItem {  Text = "Par Agent", Value = "3" });
            listItems.Add(new SelectListItem {  Text = "Par Date et agent", Value = "4" });


            return listItems;
        }


        // GET: Statistique
        public ActionResult Index()
        {

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewData["sortOrder"] = new SelectList(TypeStatForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

            StatLot statLot = new StatLot()
            {
                nb = 0,
                rdv = 0,
                fn = 0,
                versement = 0,
                encours = 0,
                avgRdv = 0+"",
                avgFn = 0+"",
                avgVers = 0+"",
                avgencours = 0+""
            };
            return View(statLot);
        }



        [HttpPost]
        public ActionResult StatLot(int numLot, string typeStat, string dateStat,string agent)
        {

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewData["sortOrder"] = new SelectList(TypeStatForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

            int nb = 0;
            int rdv = 0;
            int fn = 0;
            int versement = 0;
            int encours = 0;

            string avgRdv = 0+"";
            string avgVers = 0+"";
            string avgFn = 0+"";
            string avgencours = 0+"";

            List<Affectation> affectations = new List<Affectation>();
            List<Lot> lots = new List<Lot>();
            List<ClientAffecteViewModel> rdvCAVm = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> fnCAVm = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> versCAVm = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> encoursCAVm = new List<ClientAffecteViewModel>();
            List<ClientAffecteViewModel> tot = new List<ClientAffecteViewModel>();

            tot = (from a in AffectationService.GetAll()
            join l in LotService.GetAll() on a.LotId equals l.LotId
                  where l.NumLot == numLot.ToString()
                  select new ClientAffecteViewModel
                  {

                      Affectation = a,
                      Lot = l,

                  }).ToList();


            rdvCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                    join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                    join l in LotService.GetAll() on a.LotId equals l.LotId
                    where l.NumLot == numLot + "" 

                    select new ClientAffecteViewModel
                    {

                        Formulaire = f,
                        Affectation = a,
                        Lot = l,

                    }).DistinctBy(d => d.Formulaire.AffectationId).Where(f=> f.Formulaire.EtatClient == Note.RDV).ToList();
            

            fnCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                   join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                   join l in LotService.GetAll() on a.LotId equals l.LotId
                   where l.NumLot == numLot + ""

                   select new ClientAffecteViewModel
                   {

                       Formulaire = f,
                       Affectation = a,
                       Lot = l,

                   }).DistinctBy(d => d.Formulaire.AffectationId).Where(f=> f.Formulaire.EtatClient == Note.FAUX_NUM).ToList();
            

            versCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                  join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                  join l in LotService.GetAll() on a.LotId equals l.LotId
                  where l.NumLot == numLot + "" 

                  select new ClientAffecteViewModel
                  {

                      Formulaire = f,
                      Affectation = a,
                      Lot = l,

                  }).DistinctBy(d => d.Formulaire.AffectationId).Where(f=> f.Formulaire.EtatClient == Note.SOLDE_TRANCHE || f.Formulaire.EtatClient == Note.SOLDE).ToList();
            

            encoursCAVm = (from f in FormulaireService.GetAll().OrderByDescending(o => o.TraiteLe)
                         join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                         join l in LotService.GetAll() on a.LotId equals l.LotId
                         where l.NumLot == numLot + ""

                         select new ClientAffecteViewModel
                         {

                             Formulaire = f,
                             Affectation = a,
                             Lot = l,

                         }).DistinctBy(d => d.Formulaire.AffectationId).Where(f=>f.Formulaire.EtatClient == Note.REFUS_PAIEMENT || f.Formulaire.EtatClient == Note.RAPPEL || f.Formulaire.EtatClient == Note.RACCROCHE || f.Formulaire.EtatClient == Note.NRP || f.Formulaire.EtatClient == Note.INJOIGNABLE  || f.Formulaire.EtatClient == Note.AUTRE || f.Formulaire.EtatClient == Note.A_VERIFIE).ToList();



            if(typeStat == "1")
            {
                nb = tot.Count();
                rdv = rdvCAVm.Count();
                fn = fnCAVm.Count();
                versement = versCAVm.Count();
                encours = encoursCAVm.Count();

                avgRdv = String.Format("{0:0.00}", ((float)rdv / (float)nb) * 100);
                avgFn = String.Format("{0:0.00}", ((float)fn / (float)nb) * 100);
                avgVers = String.Format("{0:0.00}", ((float)versement / (float)nb) * 100);
                avgencours = String.Format("{0:0.00}", ((float)encours / (float)nb) * 100);

            }
            else if (typeStat == "2")
            {

                rdv = rdvCAVm.Where(f => f.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                fn = fnCAVm.Where(f => f.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                versement = versCAVm.Where(f => f.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                encours = encoursCAVm.Where(f => f.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                
                nb = rdv + fn + versement + encours;

                avgRdv = String.Format("{0:0.00}", ((float)rdv / (float)nb) * 100);
                avgFn = String.Format("{0:0.00}", ((float)fn / (float)nb) * 100);
                avgVers = String.Format("{0:0.00}", ((float)versement / (float)nb) * 100);
                avgencours = String.Format("{0:0.00}", ((float)encours / (float)nb) * 100);
            }
            else if (typeStat == "3")
            {



                rdv = rdvCAVm.Where(a=>a.Affectation.EmployeId+"" == agent).Count();
                fn = fnCAVm.Where(a => a.Affectation.EmployeId + "" == agent).Count();
                versement = versCAVm.Where(a => a.Affectation.EmployeId + "" == agent).Count();
                encours = encoursCAVm.Where(a => a.Affectation.EmployeId + "" == agent).Count();

                nb = rdv + fn + versement + encours;

                avgRdv = String.Format("{0:0.00}", ((float)rdv / (float)nb) * 100);
                avgFn = String.Format("{0:0.00}", ((float)fn / (float)nb) * 100);
                avgVers = String.Format("{0:0.00}", ((float)versement / (float)nb) * 100);
                avgencours = String.Format("{0:0.00}", ((float)encours / (float)nb) * 100);
            }
            else if (typeStat == "4")
            {



                rdv = rdvCAVm.Where(a => a.Affectation.EmployeId + "" == agent && a.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                fn = fnCAVm.Where(a => a.Affectation.EmployeId + "" == agent && a.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                versement = versCAVm.Where(a => a.Affectation.EmployeId + "" == agent && a.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();
                encours = encoursCAVm.Where(a => a.Affectation.EmployeId + "" == agent && a.Formulaire.TraiteLe.Date == DateTime.Parse(dateStat).Date).Count();

                nb = rdv + fn + versement + encours;

                avgRdv = String.Format("{0:0.00}", ((float)rdv / (float)nb) * 100);
                avgFn = String.Format("{0:0.00}", ((float)fn / (float)nb) * 100);
                avgVers = String.Format("{0:0.00}", ((float)versement / (float)nb) * 100);
                avgencours = String.Format("{0:0.00}", ((float)encours / (float)nb) * 100);
            }

            StatLot statLot = new StatLot()
            {
                nb = nb,
                rdv = rdv,
                fn = fn,
                versement = versement,
                encours = encours,
                avgRdv = avgRdv.Replace(",", "."),
                avgFn = avgFn.Replace(",", "."),
                avgVers = avgVers.Replace(",", "."),
                avgencours = avgencours.Replace(",",".")
            };

            return View("Index", statLot);
        }


        public IEnumerable<SelectListItem> AgentListForDropDown()
        {

            List<Employe> agents = EmpService.GetMany(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true).ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();


            agents.ForEach(l => {
                listItems.Add(new SelectListItem { Text = l.Username, Value = l.EmployeId + "" });
            });

            return listItems;
        }

        // GET: Statistique/Details/5

    }
}
