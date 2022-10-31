using SpringCaching.Parsing;
using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    internal class DefaultPredicateGenerator : IPredicateGenerator
    {
        public static readonly DefaultPredicateGenerator Instance = new DefaultPredicateGenerator();
        public bool Predicate(string? expression, ISpringCachingRequirement requirement) => true;
    }
}
