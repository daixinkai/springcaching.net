using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching.Requirement
{
    public interface ICacheableRequirement
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

        ExpirationPolicy ExpirationPolicy { get; }
        ExpirationUnit ExpirationUnit { get; }
        int ExpirationValue { get; }

        //string? Condition { get; }

        IKeyGenerator? KeyGenerator { get; }

    }
}
