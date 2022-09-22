using SpringCaching.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Requirement
{
    public class CachePutRequirement : CacheableRequirement, ICachePutRequirement
    {
        public CachePutRequirement(string value) : base(value)
        {
        }
    }
}
