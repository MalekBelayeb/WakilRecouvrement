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
            listItems.Add(new SelectListItem { Selected = true, Text = "Par Date", Value = "2" });


            return listItems;
        }


        // GET: Statistique
        public ActionResult Index()
        {

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewData["sortOrder"] = new SelectList(TypeStatForDropDown(), "Value", "Text");

            return View();
        }



        [HttpPost]
        public ActionResult StatLot(int lot, string type,DateTime date)
        {

            int nb = 0;
            int rdv = 0;
            int versement = 0;
            int encours = 0;
            int fauxnum = 0;

            nb = (from a in AffectationService.GetAll() 
                  join l in LotService.GetAll() on a.LotId equals l.LotId
                  where l.NumLot == lot.ToString()
                  select new ClientAffecteViewModel
                  {

                      Affectation = a,
                      Lot = l,

                  }).Count();

            rdv = (from f in FormulaireService.GetMany(f => f.EtatClient  == Note.RDV).OrderByDescending(o => o.TraiteLe)
                                join a in AffectationService.GetAll() on f.AffectationId equals a.AffectationId
                                join l in LotService.GetAll() on a.LotId equals l.LotId


                                select new ClientAffecteViewModel
                                {

                                    Formulaire = f,
                                    Affectation = a,
                                    Lot = l,


                                }).DistinctBy(d => d.Formulaire.AffectationId).Count();




            Debug.WriteLine(rdv);


            return Json(new { nb = nb });
        }

        // GET: Statistique/Details/5
        
    }
}
