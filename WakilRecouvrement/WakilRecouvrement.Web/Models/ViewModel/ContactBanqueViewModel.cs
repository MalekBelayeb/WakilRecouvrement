﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WakilRecouvrement.Web.Models.ViewModel
{
    public class ContactBanqueViewModel
    {

        public string NumLot { get; set; }
        public string Compte { get; set; }
        public string IDClient { get; set; }
        public string NomClient { get; set; }
        public string Etat { get; set; }

    }
}