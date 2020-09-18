using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WakilRecouvrement.Domain.Entities;

namespace WakilRecouvrement.Service
{
    public interface IFormulaireService : IService<Formulaire>
    {
        Formulaire GetOrderedFormulaireByAffectation(int AffectationId);
        IEnumerable<Formulaire> GetOrderedFormulaireByAffectationList(int AffectationId);

    }
}
