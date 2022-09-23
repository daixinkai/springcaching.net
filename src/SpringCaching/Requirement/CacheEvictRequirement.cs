using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringCaching.Infrastructure;

namespace SpringCaching.Requirement
{
    public class CacheEvictRequirement : CacheRequirementBase, ICacheEvictRequirement
    {
        public CacheEvictRequirement(string value) : base(value)
        {
        }

        /// <inheritdoc />
        public bool AllEntries { get; set; }
        /// <inheritdoc />
        public bool BeforeInvocation { get; set; }

    }
}
