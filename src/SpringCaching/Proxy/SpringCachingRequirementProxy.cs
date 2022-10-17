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
        protected string? ToString<T>(T? value) => value?.ToString();

        protected string? ToNullableString<T>(T? value) where T : struct
            => value?.ToString();

        protected string? ToStructString<T>(T value) where T : struct
             => value.ToString();

        protected string? ToClassString<T>(T? value) where T : class
             => value?.ToString();

        protected bool IsNull<T>(T? value) where T : class => value == null;

        protected bool IsNull<T>(T? value) where T : struct => !value.HasValue;

    }
}
