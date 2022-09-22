using SpringCaching.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Requirement
{
    public abstract class CacheableRequirementBase
    {
        public CacheableRequirementBase(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public string Value { get; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression for computing the key dynamically.
        /// Default is "", meaning all method parameters are considered as a key, unless a custom keyGenerator has been configured.
        /// </summary>
        public string? Key { get; set; }
        /// <summary>
        /// Spring Expression Language (SpEL) expression used for making the method caching conditional.
        /// Default is "", meaning the method result is always cached.
        /// </summary>
        public string? Condition { get; set; }
        /// <inheritdoc />
        public IKeyGenerator? KeyGenerator { get; set; }
        /// <inheritdoc />
        public IPredicateGenerator? ConditionGenerator { get; set; }

    }
}
