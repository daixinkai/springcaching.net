using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    public interface IPredicateGenerator
    {
        bool Predicate(string? expression, ISpringCachingRequirement requirement);
    }
}
