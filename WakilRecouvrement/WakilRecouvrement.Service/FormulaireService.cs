﻿using MyFinance.Data.Infrastructure;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Service
{
    public class FormulaireService : Service<Formulaire>, IFormulaireService
    {

        public FormulaireService(UnitOfWork UOW) : base(UOW)
        {
            //UOW = new UnitOfWork(wakilRecouvContext);
        }


        public Formulaire GetOrderedFormulaireByAffectation(int AffectationId)
        {
            return GetMany(f=>f.AffectationId == AffectationId).OrderByDescending(f=>f.TraiteLe).FirstOrDefault();
        }

        public IEnumerable<Formulaire> GetOrderedFormulaireByAffectationList(int AffectationId)
        {
            return GetMany(f => f.AffectationId == AffectationId).OrderByDescending(f => f.TraiteLe).ToList();
        }




    }
}
