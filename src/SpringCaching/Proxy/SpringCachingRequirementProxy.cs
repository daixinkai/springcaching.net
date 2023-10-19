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

        public virtual bool DefaultNullValue => true;

        private const string NullValue = "null";

        public virtual IList<ICacheableRequirement>? GetCacheableRequirements() => null;
        public virtual IList<ICacheEvictRequirement>? GetCacheEvictRequirements() => null;
        public virtual IList<ICachePutRequirement>? GetCachePutRequirements() => null;
        public virtual IDictionary<string, object>? Arguments => null;

        protected string? ToString<T>(T? value) => value?.ToString();


        protected string? ToStringFromString(string? value)
        {
            if (!DefaultNullValue)
            {
                return value;
            }
            return value ?? NullValue;
        }

        protected string? ToStringFromNullable<T>(T? value) where T : struct
        {
            if (value.HasValue)
            {
                return value.Value.ToString();
            }
            if (DefaultNullValue)
            {
                return NullValue;
            }
            return null;
        }

        protected string ToStringFromStruct<T>(T value) where T : struct
             => value.ToString();

        protected string? ToStringFromClass<T>(T? value) where T : class
             => value?.ToString() ?? NullValue;

        protected bool IsNull<T>(T? value) where T : class => value == null;

        protected bool IsNull<T>(T? value) where T : struct => !value.HasValue;

    }
}
