using SpringCaching.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Requirement
{
    public abstract class CacheRequirementBase : ICacheRequirement
    {
        public CacheRequirementBase(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public string Value { get; }
        /// <inheritdoc />
        public string? Key { get; set; }
        /// <inheritdoc />
        public string? Condition { get; set; }
        /// <inheritdoc />
        public IKeyGenerator? KeyGenerator { get; set; }
        /// <inheritdoc />
        public IPredicateGenerator? ConditionGenerator { get; set; }

    }
}
