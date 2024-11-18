using SpringCaching.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching.Requirement
{
    public interface ICacheableRequirement : ICacheRequirement
    {
        /// <summary>
        /// if result is null, don't cache it
        /// </summary>
        bool UnlessNull { get; set; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression used for making the method caching conditional.
        /// Default is "", meaning the method result is always cached.
        /// </summary>
        string? ResultCondition { get; set; }
        IResultPredicateGenerator? ResultConditionGenerator { get; }

        ExpirationPolicy ExpirationPolicy { get; }
        ExpirationUnit ExpirationUnit { get; }
        int ExpirationValue { get; }

    }
}
