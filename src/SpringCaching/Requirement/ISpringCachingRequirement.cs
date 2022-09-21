using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching.Requirement
{
    public interface ISpringCachingRequirement
    {
        IList<ICacheableRequirement>? GetCacheableRequirements();
        IList<ICacheEvictRequirement>? GetCacheEvictRequirements();
        IList<ICachePutRequirement>? GetCachePutRequirements();
        IDictionary<string, object>? Arguments { get; }
    }
}
