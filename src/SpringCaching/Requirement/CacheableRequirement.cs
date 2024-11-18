using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringCaching.Infrastructure;

namespace SpringCaching.Requirement
{
    public class CacheableRequirement : CacheRequirementBase, ICacheableRequirement
    {
        public CacheableRequirement(string value) : base(value)
        {
        }
        /// <inheritdoc />
        public ExpirationPolicy ExpirationPolicy { get; set; }
        /// <inheritdoc />
        public ExpirationUnit ExpirationUnit { get; set; }
        /// <inheritdoc />
        public int ExpirationValue { get; set; }
        /// <inheritdoc />
        public bool UnlessNull { get; set; }
        /// <inheritdoc />
        public string? ResultCondition { get; set; }
        /// <inheritdoc />
        public IResultPredicateGenerator? ResultConditionGenerator { get; set; }
    }
}
