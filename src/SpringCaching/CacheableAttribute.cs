using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace SpringCaching
{
    /// <summary>
    /// <para>Annotation indicating that the result of invoking a method (or all methods in a class) can be cached.</para>
    /// <para>Each time an advised method is invoked, caching behavior will be applied, checking whether the method has been already invoked for the given arguments.A sensible default simply uses the method parameters to compute the key, but a SpEL expression can be provided via the key attribute, or a custom org.springframework.cache.interceptor.KeyGenerator implementation can replace the default one (see keyGenerator).</para>
    /// <para>If no value is found in the cache for the computed key, the target method will be invoked and the returned value stored in the associated cache.Note that Java8's Optional return types are automatically handled and its content is stored in the cache if present.</para>
    /// <para>This annotation may be used as a meta-annotation to create custom composed annotations with attribute overrides.</para>
    /// </summary>
    public class CacheableAttribute : CacheBaseAttribute
    {
        public CacheableAttribute(string value) : base(value)
        {
        }
        /// <summary>
        /// ExpirationPolicy
        /// </summary>
        public ExpirationPolicy ExpirationPolicy { get; set; }
        /// <summary>
        /// ExpirationUnit
        /// </summary>
        public ExpirationUnit ExpirationUnit { get; set; }
        /// <summary>
        /// ExpirationValue
        /// </summary>
        public int ExpirationValue { get; set; }
        /// <summary>
        /// if result is null, don't cache it
        /// </summary>
        public bool UnlessNull { get; set; }
    }
}
