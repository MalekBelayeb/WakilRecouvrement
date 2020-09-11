using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;

namespace WakilRecouvrement.Web.Controllers
{
    public class FormulaireController : Controller
    {

        AffectationService AffectationService;
        LotService LotService;
        EmployeService EmpService;


        public FormulaireController()
        {
            AffectationService = new AffectationService();
            LotService = new LotService();
            EmpService = new EmployeService();

        }


        public ActionResult SuiviClient()
        {
            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");
            ViewBag.AgentList = new SelectList(AgentListForDropDown(), "Value", "Text");

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


        [HttpPost]
        public ActionResult LoadData(string numLot,string agent, string traite )
        {

            List<Lot> Lots = new List<Lot>();

            ViewData["list"] = new SelectList(NumLotListForDropDown(), "Value", "Text");

            if (numLot == "0")
            {
                Lots = LotService.GetAll().ToList();
            }
            else
            {
                Lots = LotService.GetAll().ToList().Where(l => l.NumLot.Equals(numLot)).ToList();
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



                int totalRecords = Lots.Count;

                if (!string.IsNullOrEmpty(search) &&
                    !string.IsNullOrWhiteSpace(search))
                {
                    Lots = Lots.Where(l =>

                        l.Numero.ToString().Contains(search)
                    || l.Adresse.ToString().ToLower().Contains(search.ToLower())
                    || l.TelFixe.ToString().Contains(search)
                    || l.TelPortable.ToString().Contains(search)
                    || l.IDClient.ToString().Contains(search)
                    || l.Compte.ToString().Contains(search)
                    || l.LotId.ToString().Contains(search)
                    || l.NomClient.ToString().ToLower().Contains(search.ToLower())
                    || l.DescIndustry.ToString().ToLower().Contains(search.ToLower())

                        ).ToList();
                }


                Lots = SortTableData(order, orderDir, Lots);

                int recFilter = Lots.Count;

                Lots = Lots.Skip(startRec).Take(pageSize).ToList();
                var modifiedData = Lots.Select(l =>
                   new
                   {
                       l.LotId,
                       l.NumLot,
                       l.Compte,
                       l.IDClient,
                       l.NomClient,
                       l.TelPortable,
                       l.TelFixe,
                       l.SoldeDebiteur,
                       l.DescIndustry,
                       l.Adresse,
                       l.Type,
                       l.Numero
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

        private List<Lot> SortTableData(string order, string orderDir, List<Lot> data)
        {
            List<Lot> lst = new List<Lot>();
            try
            {
                switch (order)
                {
                    case "0":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.LotId).ToList()
                                                                                                 : data.OrderBy(l => l.LotId).ToList();
                        break;

                    case "1":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => long.Parse(l.NumLot)).ToList()
                                                                                                 : data.OrderBy(l => long.Parse(l.NumLot)).ToList();
                        break;
                    case "2":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.Compte).ToList()
                                                                                                 : data.OrderBy(l => l.Compte).ToList();
                        break;
                    case "3":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => long.Parse(l.IDClient)).ToList()
                                                                                                 : data.OrderBy(l => long.Parse(l.IDClient)).ToList();
                        break;
                    case "4":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.NomClient).ToList()
                                                                                                 : data.OrderBy(l => l.NomClient).ToList();
                        break;
                    case "5":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.TelPortable).ToList()
                                                                                                   : data.OrderBy(l => l.TelPortable).ToList();
                        break;
                    case "6":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.TelFixe).ToList()
                                                                                                   : data.OrderBy(l => l.TelFixe).ToList();
                        break;
                    case "7":

                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => Double.Parse(l.SoldeDebiteur)).ToList()
                                                                                              : data.OrderBy(l => Double.Parse(l.SoldeDebiteur)).ToList();

                        break;
                    case "8":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.DescIndustry).ToList()
                                                                                                   : data.OrderBy(l => l.DescIndustry).ToList();
                        break;
                    case "9":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.Adresse).ToList()
                                                                                                   : data.OrderBy(l => l.Adresse).ToList();
                        break;
                    case "10":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.Type).ToList()
                                                                                                   : data.OrderBy(l => l.Type).ToList();
                        break;

                    case "11":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.Numero).ToList()
                                                                                                   : data.OrderBy(l => l.Numero).ToList();
                        break;

                    default:
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(l => l.LotId).ToList()
                                                                                                 : data.OrderBy(l => l.LotId).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return lst;
        }



    }  
}
