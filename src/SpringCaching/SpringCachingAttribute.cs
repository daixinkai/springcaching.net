using System;
using System.Collections.Generic;
using System.Text;

namespace SpringCaching
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SpringCachingAttribute : Attribute
    {
        /// <summary>
        /// The bean name of the custom <see cref="ICacheProvider"/> to use.
        /// Mutually exclusive with the key attribute.
        /// </summary>
        public Type? CacheProvider { get; set; }
        /// <summary>
        /// The bean name of the custom <see cref="ICacheProviderFactory"/> to use.
        /// Mutually exclusive with the key attribute.
        /// </summary>
        public Type? CacheProviderFactory { get; set; }
    }
}
