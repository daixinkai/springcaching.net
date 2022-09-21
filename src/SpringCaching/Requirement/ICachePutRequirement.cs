using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching.Requirement
{
    public interface ICachePutRequirement
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

        string? Condition { get; }
        ///// <summary>
        ///// Spring Expression Language (SpEL) expression used to veto the cache put operation.
        ///// Default is "", meaning that caching is never vetoed.
        ///// The SpEL expression evaluates against a dedicated context that provides the following meta-data
        ///// </summary>
        //string? Unless { get; }
    }
}
