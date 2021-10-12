using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Web.Models.ViewModel
{
    public class TraiterDetailViewModel
    {

        public Affectation Affectation { get; set; }

        public Formulaire Formulaire { get; set; }

        public Lot Lot { get; set; }

        public string Username { get; set; }


    }


}