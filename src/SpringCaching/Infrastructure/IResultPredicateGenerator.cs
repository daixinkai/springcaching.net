using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Infrastructure
{
    public interface IResultPredicateGenerator
    {
        bool Predicate(string? expression, ISpringCachingRequirement requirement, object? result);
    }

    public interface IResultPredicateGenerator<TResult> : IResultPredicateGenerator
    {
        bool Predicate(string? expression, ISpringCachingRequirement requirement, TResult? result);
    }
}
