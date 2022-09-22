using SpringCaching.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Requirement
{
    public abstract class CachePutRequirement : CacheableRequirementBase, ICachePutRequirement
    {
        public CachePutRequirement(string value) : base(value)
        {
        }
        public string? Unless { get; set; }
        public IPredicateGenerator? UnlessGenerator { get; set; }
    }
}
