﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WakilRecouvrement.Web.Models
{
    public class EmailConstants
    {

        public const string VERSEMENT_BODY = "Ci-joint vous trouverez une liste des nouveaux versements"; 
        public const string AVERIFIE_BODY = "Ci-joint vous trouverez une liste des clients à verifier"; 
        public const string ENCOURS_BODY = "Ci-joint vous trouverez une liste des clients en cours de traitement"; 
        public const string RDV_BODY = "Ci-joint vous trouverez une liste des nouveaux rendez-vous"; 
        
        public const string VERSEMENT_SUBJECT = "Liste des versements";
        public const string AVERIFIE_SUBJECT = "Liste des clients à verifier";
        public const string ENCOURS_SUBJECT = "Liste des clients en cours de traitement";
        public const string RDV_SUBJECT = "Liste des rendez-vous";
       
        public const string TO = "zaitounabank@gmail.com,aucunnoma@gmail.com";

    }
}