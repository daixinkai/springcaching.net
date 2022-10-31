using SpringCaching.Parsing;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    public class PredicateGenerator : IPredicateGenerator
    {
        public PredicateGenerator(bool value)
        {
            _value = value;
        }

        private readonly bool _value;

        bool IPredicateGenerator.Predicate(string? expression, ISpringCachingRequirement requirement)
        {
            return _value;
        }

    }
}
