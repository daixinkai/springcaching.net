using SpringCaching.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Requirement
{
    public interface ICacheRequirement
    {
        /// <summary>
        /// Names of the caches in which method invocation results are stored.
        /// Names may be used to determine the target cache(or caches), matching the qualifier value or bean name of a specific bean definition.
        /// </summary>
        string Value { get; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression for computing the key dynamically.
        /// Default is "", meaning all method parameters are considered as a key, unless a custom keyGenerator has been configured.
        /// </summary>
        string? Key { get; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression used for making the method caching conditional.
        /// Default is "", meaning the method result is always cached.
        /// </summary>
        string? Condition { get; }
        IKeyGenerator? KeyGenerator { get; }
        IPredicateGenerator? ConditionGenerator { get; }
    }
}
