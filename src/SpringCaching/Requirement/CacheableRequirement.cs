using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringCaching.Infrastructure;

namespace SpringCaching.Requirement
{
    public class CacheableRequirement : CacheableRequirementBase, ICacheableRequirement
    {
        public CacheableRequirement(string value) : base(value)
        {
        }

        public ExpirationPolicy ExpirationPolicy { get; set; }

        public ExpirationUnit ExpirationUnit { get; set; }

        public int ExpirationValue { get; set; }

        public string? Unless { get; set; }

        public IPredicateGenerator? UnlessGenerator { get; set; }
    }
}
