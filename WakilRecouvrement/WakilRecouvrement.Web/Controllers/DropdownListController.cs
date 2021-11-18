using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WakilRecouvrement.Domain.Entities;
using WakilRecouvrement.Service;
using WakilRecouvrement.Web.Models;

namespace WakilRecouvrement.Web.Controllers
{
    public static class DropdownListController
    {


        public static IEnumerable<SelectListItem> TraiteListForDropDownForCreation()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();

            foreach (var n in Enum.GetValues(typeof(Note)))
            {

                listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString() });

            }


            return listItems;
        }

        public static IEnumerable<SelectListItem> NumLotListForDropDown(LotService LotService)
        {

            List<Lot> Lots = LotService.GetAll().ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les lots", Value = "0" });

            Lots.DistinctBy(l => l.NumLot).ForEach(l => {
                listItems.Add(new SelectListItem { Text = "Lot " + l.NumLot, Value = l.NumLot });
            });

            return listItems;
        }

        public static IEnumerable<SelectListItem> RDVForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "RDV du jour", Value = "RDV_J" });
            listItems.Add(new SelectListItem { Text = "Mes RDV pour demain", Value = "RDV_DEMAIN" });
            listItems.Add(new SelectListItem { Text = "Mes RDV pour les prochains jours", Value = "RDV_JOURS_PROCHAINE" });
            listItems.Add(new SelectListItem { Text = "Mes RDV pour la semaine prochaine", Value = "RDV_SEMAINE_PROCHAINE" });
            listItems.Add(new SelectListItem { Text = "Tous mes RDV du:", Value = "RDVDate" });
            listItems.Add(new SelectListItem { Text = "Tous mes RDV", Value = "ALL" });


            return listItems;
        }
        public static IEnumerable<SelectListItem> SortOrderSuiviRDVForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date RDV (o. decroissant)", Value = "5" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date RDV (o. croissant)", Value = "6" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date Traitement (o. decroissant)", Value = "7" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date Traitement (o. croissant)", Value = "8" });


            return listItems;
        }
        public static IEnumerable<SelectListItem> TraiteListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les clients affectés", Value = "ALL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les clients non traités", Value = "NON_TRAITE" });

            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les clients traités sauf SOLDE/FAUX_NUM", Value = "SAUF" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous mes traitements (VERSEMENT/VERIFIE)", Value = "VERS" });

            foreach (var n in Enum.GetValues(typeof(Note)))
            {

                listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString() });

            }

            return listItems;
        }

        public static IEnumerable<SelectListItem> SortOrderSuiviTousClientForDropDown()
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
        public static IEnumerable<SelectListItem> SortOrderSuiviClientForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Nom (A-Z)", Value = "0" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde supérieur à", Value = "7" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde inférieur à", Value = "8" });

            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. decroissant)", Value = "1" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Solde debiteur (o. croissant)", Value = "2" });

            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. decroissant)", Value = "5" });
            listItems.Add(new SelectListItem { Selected = true, Text = "Date traitement (o. croissant)", Value = "6" });


            return listItems;
        }

        public static IEnumerable<SelectListItem> TraiteListSuiviTraitForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les clients traités", Value = "ALL" });

            foreach (var n in Enum.GetValues(typeof(Note)))
            {

                listItems.Add(new SelectListItem { Text = n.ToString(), Value = n.ToString() });

            }


            return listItems;
        }

        public static IEnumerable<SelectListItem> typeListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les traitements", Value = "ALL_TRAIT" });
            listItems.Add(new SelectListItem { Text = "Traitements par date", Value = "DATE_TRAIT" });

            return listItems;
        }

        public static IEnumerable<SelectListItem> TraiteValidationListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les traitements non validés", Value = "ALL" });
            listItems.Add(new SelectListItem { Text = "Soldé", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "Tranche", Value = "SOLDE_TRANCHE" });
            listItems.Add(new SelectListItem { Text = "A verifié", Value = "A_VERIFIE" });

            return listItems;
        }
        public static IEnumerable<SelectListItem> TraiteListSuiviTraitHistoriqueForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Selected = true, Text = "SOLDE ET TRANCHE", Value = "ALL" });
            listItems.Add(new SelectListItem { Selected = true, Text = "SOLDE", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Selected = true, Text = "TRANCHE", Value = "SOLDE_TRANCHE" });

            return listItems;
        }

        public static IEnumerable<SelectListItem> AgentListForDropDown(EmployeService EmpService)
        {

            List<Employe> agents = EmpService.GetMany(emp => emp.Role.role.Equals("agent") && emp.IsVerified == true).ToList();
            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem { Selected = true, Text = "Tous les agents", Value = "0" });

            agents.ForEach(l =>
            {
                listItems.Add(new SelectListItem { Text = l.Username, Value = l.Username + "" });
            });

            return listItems;


        }


        public static IEnumerable<SelectListItem> EnvoyerTraiteListForDropDown()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Text = "Versement", Value = "SOLDE" });
            listItems.Add(new SelectListItem { Text = "RDV", Value = "RDV" });
            listItems.Add(new SelectListItem { Text = "Verification", Value = "A_VERIFIE" });
            listItems.Add(new SelectListItem { Text = "En cours de traitement", Value = "Autre" });

            return listItems;
        }

        public static IEnumerable<SelectListItem> TraiteTypeForValiderListForDropDown()
        {

            List<SelectListItem> listItems = new List<SelectListItem>();
            
            listItems.Add(new SelectListItem { Text = "Par date de vérification", Value = "P_DATE" });
            listItems.Add(new SelectListItem { Text = "Par interval de temps", Value = "P_INTERVAL" });
            listItems.Add(new SelectListItem { Text = "Tous les traitements", Value = "P_ALL" });

            return listItems;
        }

    }
}