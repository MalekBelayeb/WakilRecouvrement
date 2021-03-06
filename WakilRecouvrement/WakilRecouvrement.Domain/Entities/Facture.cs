﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakilRecouvrement.Domain.Entities
{
    public class Facture
    {
        public int FactureId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateExtrait { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime DateDeb { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime DateFin { get; set; }
        public string FacturePathName { get; set; } 
        public string AnnexePathName { get; set; } 

    }
}
