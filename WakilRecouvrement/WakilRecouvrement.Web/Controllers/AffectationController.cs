using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;

namespace WakilRecouvrement.Web.Controllers
{
    public class AffectationController : Controller
    {

        EmployeService EmpService;
        LotService LotService;
        AffectationService AffectationService;
        public AffectationController()
        {
            
            LotService = new LotService();
            AffectationService = new AffectationService();
            EmpService = new EmployeService();

        }

        public ActionResult ChoisirLot()
        {
            List<Lot> Lots = LotService.GetAll().ToList();

            return View(Lots.DistinctBy(l => l.NumLot));
        }


        public ActionResult AffecterAgent(int numLot)
        {
            
            List<Lot> listClients = LotService.GetClientsByLot(numLot + "").ToList();

            var listAffectations = AffectationService.GetAffectationByLot(numLot+"");
            
            var agents = EmpService.GetAll().Where(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true);

            ViewBag.AgentList = new SelectList(agents, "EmployeId", "Username");

            int totalClientParLot = listClients.Count();

            var clientsNonAffecteList = from lot in listClients
                                        where !(from aff in listAffectations
                                                select aff.LotId).Contains(lot.LotId)
                                        select lot;

            int nbClientsNonAffecteParLots = clientsNonAffecteList.Count() ;

            int totalAffectationParLot = listAffectations.Count();

            float pourcentageAffectationParLot = ((float)totalAffectationParLot / (float)totalClientParLot) * 100;

            ViewData["numLot"] = numLot;
            ViewData["totalClientsParLots"] = totalClientParLot;
            ViewData["totalAffectationParLots"] = totalAffectationParLot;
            ViewData["totalClientsRestantParLots"] = nbClientsNonAffecteParLots;
            ViewData["pourcentageAffectationParLot"] = (int)pourcentageAffectationParLot;
            return View();
        }
    
        
        public void updateView(int numLot)
        {

            
        }

        [HttpPost]
        public ActionResult AffecterAgents(int numLot, int nbClient,int agent)
        {

            List<Lot> listClients = LotService.GetClientsByLot(numLot + "").ToList();

            List<Affectation> listAffectations = AffectationService.GetAffectationByLot(numLot+"").ToList();

            var clientsNonAffecteList = from lot in listClients
                        where !(from aff in listAffectations 
                                select aff.LotId).Contains(lot.LotId)
                        select lot;

            int nbClientsNonAffecteParLots = clientsNonAffecteList.Count();
           
            for (int i=0;i<nbClient;i++)
            {
             
                AffectationService.Add(new Affectation { EmployeId = agent, AffectePar = "", DateAffectation = DateTime.Now, LotId = clientsNonAffecteList.ToList()[i].LotId });

            }

            AffectationService.Commit();

            var result = UpdateView(numLot);

            return result;
           }

        public ActionResult UpdateView(int numLot)
        {

            List<Lot> listClientsUpdated = LotService.GetClientsByLot(numLot + "").ToList();

            var listAffectationsUpdated = AffectationService.GetAffectationByLot(numLot + "");

            int totalClientParLotUpdated = listClientsUpdated.Count();

            var clientsNonAffecteListUpdated = from lot in listClientsUpdated
                                               where !(from aff in listAffectationsUpdated
                                                       select aff.LotId).Contains(lot.LotId)
                                               select lot;

            int nbClientsNonAffecteParLotsUpdated = clientsNonAffecteListUpdated.Count();

            int totalAffectationParLotUpdated = listAffectationsUpdated.Count();

            float pourcentageAffectationParLotUpdated = ((float)totalAffectationParLotUpdated / (float)totalClientParLotUpdated) * 100;


            return Json(new { totalClientParLotUpdated = totalClientParLotUpdated, nbClientsNonAffecteParLots = nbClientsNonAffecteParLotsUpdated, pourcentageAffectationParLotUpdated = pourcentageAffectationParLotUpdated, totalAffectationParLotUpdated = totalAffectationParLotUpdated });

        }



    }
}
