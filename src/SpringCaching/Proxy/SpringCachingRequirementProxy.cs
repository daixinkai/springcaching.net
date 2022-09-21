using SpringCaching.Requirement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Proxy
{
    public class SpringCachingRequirementProxy : ISpringCachingRequirement
    {
        public virtual IList<ICacheableRequirement>? GetCacheableRequirements() => null;
        public virtual IList<ICacheEvictRequirement>? GetCacheEvictRequirements() => null;
        public virtual IList<ICachePutRequirement>? GetCachePutRequirements() => null;
        public virtual IDictionary<string, object>? Arguments => null;

    }
}
