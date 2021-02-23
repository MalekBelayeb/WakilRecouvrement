using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WakilRecouvrement.Web.Models.ViewModel
{
    public class ClientCompte
    {

        public string Nom { get; set; }
        public string Compte { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ClientCompte compte &&
                   Nom == compte.Nom &&
                   Compte == compte.Compte;
        }



    }
}