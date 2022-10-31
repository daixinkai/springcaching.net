using SpringCaching.Parsing;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    public class FuncPredicateGenerator : IPredicateGenerator
    {
        public FuncPredicateGenerator(Func<bool> func)
        {
            _func = func;
        }

        private readonly Func<bool> _func;

        bool IPredicateGenerator.Predicate(string? expression, ISpringCachingRequirement requirement)
        {
            return _func.Invoke();
        }

    }
}
